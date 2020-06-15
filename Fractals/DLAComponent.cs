using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Fractals
{
    public class DLAComponent : GH_Component
    {
        public bool HV2D;
        public bool HV3D;
        public bool FaceHV2D;
        public bool FaceHV3D;
        public bool Diag;

        /// <summary>
        /// Initializes a new instance of the PlaneDLAComponent class.
        /// </summary>
        public DLAComponent()
          : base("DLA", "DLA",
              "Diffusion Limited Aggregation. Also known as Random Walk or Drunk Man Algorithm.",
              "Fractals", "DLA")
        {
            this.HV2D = true;
            this.HV3D = true;
            this.FaceHV2D = true;
            this.FaceHV3D = true;
            this.Diag = true;
        }

        public override bool Read(GH_IReader reader)
        {
            if (!reader.TryGetBoolean("HV2D", ref this.HV2D))
                this.HV2D = true;
            if (!reader.TryGetBoolean("HV3D", ref this.HV3D))
                this.HV3D = true;
            if (!reader.TryGetBoolean("FaceHV2D", ref this.FaceHV2D))
                this.FaceHV2D = true;
            if (!reader.TryGetBoolean("FaceHV3D", ref this.FaceHV3D))
                this.FaceHV3D = true;
            if (!reader.TryGetBoolean("Diag", ref this.Diag))
                this.Diag = true;
            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("HV2D", this.HV2D);
            writer.SetBoolean("HV3D", this.HV3D);
            writer.SetBoolean("FaceHV2D", this.FaceHV2D);
            writer.SetBoolean("FaceHV3D", this.FaceHV3D);
            writer.SetBoolean("Diag", this.Diag);
            return base.Write(writer);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Bounds", "B", "Bounds of DLA Fractal", GH_ParamAccess.item, new Box(Plane.WorldXY, new Interval(-15, 15), new Interval(-15, 15), new Interval(-15, 15)));
            Params.Input[pManager.AddPointParameter("Start Point", "P", "Start Point of Drunken Man", GH_ParamAccess.item)].Optional = true;
            pManager.AddNumberParameter("Step Size", "D", "Step Size for positions", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Seed", "S", "Seed for Randomness", GH_ParamAccess.item, 50.0);
            pManager.AddIntegerParameter("Iterations", "I", "Number of Iterations", GH_ParamAccess.item, 1000);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points Output", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Box box = Box.Unset;
            Point3d point = Point3d.Unset;
            Double size = 1.0;
            Double seed = 50.0;
            int iterations = 1000;
            if (!DA.GetData(0, ref box)) return;
            if (!DA.GetData(2, ref size)) return;
            if (!DA.GetData(3, ref seed)) return;
            if (!DA.GetData(4, ref iterations)) return;
            if (DA.GetData(1, ref point))
            {
                if (!box.Contains(point))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Start point not within Bounds");
                    point = box.Center;
                }
            }
            else
            {
                point = box.Center;
            }
            if (size <= 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Step Size must be a Positive value");
                return;
            }
            List<Point3d> pointsOut = new List<Point3d>();
            DrunkMan drunkMan = new DrunkMan(point, seed, box, size, this.HV2D, this.HV3D, this.FaceHV2D, this.FaceHV3D, this.Diag);
            for (var i = 0; i < iterations; i++)
            {
                pointsOut.Add(drunkMan.NextPoint());
            }
            DA.SetDataList(0, pointsOut);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Horizontal and Vertical Points (2D)", HV2D_Click, true, this.HV2D);
            Menu_AppendItem(menu, "Horizontal and Vertical Points (3D)", HV3D_Click, true, this.HV3D);
            Menu_AppendItem(menu, "Face Centers Horizontal and Vertical (2D)", FaceHV2D_Click, true, this.FaceHV2D);
            Menu_AppendItem(menu, "Face Centers Horizontal and Vertical (3D)", FaceHV3D_Click, true, this.FaceHV3D);
            Menu_AppendItem(menu, "Diagonal Brace Points", Diag_Click, true, this.Diag);
            base.AppendAdditionalMenuItems(menu);
        }

        private void HV2D_Click(object sender, EventArgs e)
        {
            this.HV2D = !this.HV2D;
            ((ToolStripMenuItem)sender).Checked = this.HV2D;
            this.ExpireSolution(true);
        }
        private void HV3D_Click(object sender, EventArgs e)
        {
            this.HV3D = !this.HV3D;
            this.ExpireSolution(true);
            ((ToolStripMenuItem)sender).Checked = this.HV3D;
        }
        private void FaceHV2D_Click(object sender, EventArgs e)
        {
            this.FaceHV2D = !this.FaceHV2D;
            ((ToolStripMenuItem)sender).Checked = this.FaceHV2D;
            this.ExpireSolution(true);
        }
        private void FaceHV3D_Click(object sender, EventArgs e)
        {
            this.FaceHV3D = !this.FaceHV3D;
            ((ToolStripMenuItem)sender).Checked = this.FaceHV3D;
            this.ExpireSolution(true);
        }
        private void Diag_Click(object sender, EventArgs e)
        {
            this.Diag = !this.Diag;
            ((ToolStripMenuItem)sender).Checked = this.Diag;
            this.ExpireSolution(true);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("effe70ec-5db4-479b-8f09-f9d8f9362d9e"); }
        }
    }

    public class DrunkMan
    {
        public DrunkMan(Point3d start, double seed, Box bounds, double step, bool HV2D = true, bool HV3D = true, bool FaceHV2D = true, bool FaceHV3D = true, bool Diag = true)
        {
            if (bounds.Contains(start))
            {
                this.StartPoint = start;
                this.currentPoint = start;
                this.StepSize = step;
                this.Bounds = bounds;
                this.Rand = new Random((int)(seed * 1000.0));
                this._Diag = Diag;
                this._FaceHV2D = FaceHV2D;
                this._HV2D = HV2D;
                this._HV3D = HV3D;
                this._FaceHV3D = FaceHV3D;
            }
            else
            {
                throw new Exception("Start point cannot be outside Bounds");
            }
        }
        public Point3d StartPoint { get; set; }
        public Box Bounds { get; set; }
        public double StepSize { get; set; }
        private Random Rand { get; set; }
        private Point3d currentPoint;
        public bool _HV2D;
        public bool _HV3D;
        public bool _FaceHV2D;
        public bool _FaceHV3D;
        public bool _Diag;
        public Point3d NextPoint()
        {
            Point3d pOut = this.currentPoint;
            Box reachBox = new Box(new Plane(currentPoint, Vector3d.ZAxis), new Interval((0 - this.StepSize), this.StepSize), new Interval((0 - this.StepSize), this.StepSize), new Interval((0 - this.StepSize), this.StepSize));
            DoFPointsArray points = reachBox.GetFreePoints();
            List<Point3d> pointCloud = points.GetPoints(_HV2D, _HV3D, _FaceHV2D, _FaceHV3D, _Diag);
            int index = this.Rand.Next(0, (pointCloud.Count - 1));
            pOut = pointCloud[index];
            pOut = pOut.BoundPoint(this.Bounds, true);
            this.currentPoint = pOut;
            return pOut;
        }
    }

    public class DoFPointsArray
    {
        public DoFPointsArray()
        {
            this.HV2D = new List<Point3d>();
            this.HV3D = new List<Point3d>();
            this.FaceHV2D = new List<Point3d>();
            this.FaceHV3D = new List<Point3d>();
            this.Diag = new List<Point3d>();
        }
        // 4
        public List<Point3d> HV2D { get; set; }
        // 2
        public List<Point3d> HV3D { get; set; }
        // 4
        public List<Point3d> FaceHV2D { get; set; }
        // 8
        public List<Point3d> FaceHV3D { get; set; }
        // 8
        public List<Point3d> Diag { get; set; }
        public List<Point3d> GetPoints(bool HV2D, bool HV3D, bool FaceHV2D, bool FaceHV3D, bool Diag)
        {
            List<Point3d> all = new List<Point3d>();
            if (HV2D)
                all.AddRange(this.HV2D);
            if (HV3D)
                all.AddRange(this.HV3D);
            if (FaceHV2D)
                all.AddRange(this.FaceHV2D);
            if (FaceHV3D)
                all.AddRange(this.FaceHV3D);
            if (Diag)
                all.AddRange(this.Diag);
            return all;
        }
        // 26
        public List<Point3d> All
        {
            get
            {
                List<Point3d> all = new List<Point3d>();
                all.AddRange(this.HV2D);
                all.AddRange(this.HV3D);
                all.AddRange(this.FaceHV2D);
                all.AddRange(this.FaceHV3D);
                all.AddRange(this.Diag);
                return all;
            }
        }
        // 8
        public List<Point3d> Planar
        {
            get
            {
                List<Point3d> all = new List<Point3d>();
                all.AddRange(this.HV2D);
                all.AddRange(this.FaceHV2D);
                return all;
            }
        }
    }

    public static class PointUtil
    {
        public static Point3d BoundPoint(this Point3d point, Box bounds, bool mirror = false)
        {
            Point3d p1 = point;
            if (!mirror)
            {
                if (p1.X > (bounds.X.T1 + bounds.Center.X))
                    p1 = new Point3d((bounds.X.T0 + (p1.X - bounds.X.T1)), (p1.Y), (p1.Z));
                if (p1.Y > (bounds.Y.T1 + bounds.Center.Y))
                    p1 = new Point3d((p1.X), (bounds.Y.T0 + (p1.Y - bounds.Y.T1)), (p1.Z));
                if (p1.Z > (bounds.Z.T1 + bounds.Center.Z))
                    p1 = new Point3d((p1.X), (p1.Y), (bounds.Z.T0 + (p1.Z - bounds.Z.T1)));
                if (p1.X < (bounds.X.T0 + bounds.Center.X))
                    p1 = new Point3d((bounds.X.T1 - (bounds.X.T0 - p1.X)), (p1.Y), (p1.Z));
                if (p1.Y < (bounds.Y.T0 + bounds.Center.Y))
                    p1 = new Point3d((p1.X), (bounds.Y.T1 - (bounds.Y.T0 - p1.Y)), (p1.Z));
                if (p1.Z < (bounds.Z.T0 + bounds.Center.Z))
                    p1 = new Point3d((p1.X), (p1.Y), (bounds.Z.T1 - (bounds.Z.T0 - p1.Z)));
            }
            else
            {
                if (p1.X > (bounds.X.T1 + bounds.Center.X))
                    p1 = new Point3d((bounds.X.T1 - (p1.X - bounds.X.T1)), (p1.Y), (p1.Z));
                if (p1.Y > (bounds.Y.T1 + bounds.Center.Y))
                    p1 = new Point3d((p1.X), (bounds.Y.T1 - (p1.Y - bounds.Y.T1)), (p1.Z));
                if (p1.Z > (bounds.Z.T1 + bounds.Center.Z))
                    p1 = new Point3d((p1.X), (p1.Y), (bounds.Z.T1 - (p1.Z - bounds.Z.T1)));
                if (p1.X < (bounds.X.T0 + bounds.Center.X))
                    p1 = new Point3d((bounds.X.T0 + (bounds.X.T0 - p1.X)), (p1.Y), (p1.Z));
                if (p1.Y < (bounds.Y.T0 + bounds.Center.Y))
                    p1 = new Point3d((p1.X), (bounds.Y.T0 + (bounds.Y.T0 - p1.Y)), (p1.Z));
                if (p1.Z < (bounds.Z.T0 + bounds.Center.Z))
                    p1 = new Point3d((p1.X), (p1.Y), (bounds.Z.T0 + (bounds.Z.T0 - p1.Z)));
            }
            return p1;
        }

        public static DoFPointsArray GetFreePoints(this Box box)
        {
            double x0 = box.X.T1 + box.Center.X;
            double y0 = box.Y.T1 + box.Center.Y;
            double z0 = box.Y.T1 + box.Center.Z;
            double x1 = box.X.T0 + box.Center.X;
            double y1 = box.Y.T0 + box.Center.Y;
            double z1 = box.Z.T0 + box.Center.Z;
            DoFPointsArray outPoints = new DoFPointsArray();
            outPoints.HV2D.Add(new Point3d(x0, 0, 0)); //1
            outPoints.HV2D.Add(new Point3d(0, y0, 0)); //2
            outPoints.HV2D.Add(new Point3d(x1, 0, 0)); //3
            outPoints.HV2D.Add(new Point3d(0, y1, 0)); //4
            outPoints.HV3D.Add(new Point3d(0, 0, z0)); //1
            outPoints.HV3D.Add(new Point3d(0, 0, z1)); //2
            outPoints.FaceHV2D.Add(new Point3d(x0, y1, 0)); //1
            outPoints.FaceHV2D.Add(new Point3d(x1, y0, 0)); //2
            outPoints.FaceHV2D.Add(new Point3d(x0, y0, 0)); //3
            outPoints.FaceHV2D.Add(new Point3d(x1, y1, 0)); //4
            outPoints.FaceHV3D.Add(new Point3d(x0, 0, z0)); //1
            outPoints.FaceHV3D.Add(new Point3d(x0, 0, z1)); //2
            outPoints.FaceHV3D.Add(new Point3d(0, y0, z0)); //3
            outPoints.FaceHV3D.Add(new Point3d(0, y0, z1)); //4
            outPoints.FaceHV3D.Add(new Point3d(x1, 0, z0)); //5
            outPoints.FaceHV3D.Add(new Point3d(x1, 0, z1)); //6
            outPoints.FaceHV3D.Add(new Point3d(0, y1, z0)); //7
            outPoints.FaceHV3D.Add(new Point3d(0, y1, z1)); //8
            outPoints.Diag.Add(new Point3d(x0, y0, z0)); //1
            outPoints.Diag.Add(new Point3d(x1, y0, z0)); //2
            outPoints.Diag.Add(new Point3d(x1, y1, z0)); //3
            outPoints.Diag.Add(new Point3d(x0, y1, z0)); //4
            outPoints.Diag.Add(new Point3d(x0, y0, z1)); //5
            outPoints.Diag.Add(new Point3d(x1, y0, z1)); //6
            outPoints.Diag.Add(new Point3d(x1, y1, z1)); //7
            outPoints.Diag.Add(new Point3d(x0, y1, z1)); //8
            return outPoints;
        }
    }
}