using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Fractals
{
    public class BoidsComponent : GH_Component
    {
        private bool SetupDone = false;
        private Boids BoidsInstance = null;
        private bool attract = true;
        private bool Bounce = false;

        /// <summary>
        /// Initializes a new instance of the BoidsComponent class.
        /// </summary>
        public BoidsComponent()
          : base("Boids", "Boids",
              "Boids Component. Please add a Timer to this Component",
              "Fractals", "Boids")
        {
            this.Bounce = false;
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("bounce", this.Bounce);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            if (!reader.TryGetBoolean("bounce", ref this.Bounce)) this.Bounce = false;
            return base.Read(reader);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Bounce off Bounds", Bounce_Click, true, this.Bounce);
            base.AppendAdditionalMenuItems(menu);
        }
        private void Bounce_Click(object sender, EventArgs e)
        {
            this.Bounce = !this.Bounce;
            this.ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Bounds", "B", "Bounds for the Boids", GH_ParamAccess.item, new Box(Plane.WorldXY, new Interval(-15, 15), new Interval(-15, 15), new Interval(-15, 15)));
            pManager.AddPointParameter("Starting Points", "P", "Starting Points for each Boid", GH_ParamAccess.list, Point3d.Origin);
            pManager.AddNumberParameter("Perception Radius", "PR", "Perception Radius of Boids", GH_ParamAccess.item, 15.0);
            pManager.AddNumberParameter("Alignment Multiplier", "Am", "Alignment Multiplier of Boids, must be between 0 and 1", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Cohesion Multiplier", "Cm", "Cohesion Multiplier of Boids, must be between 0 and 1", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Separation Multiplier", "Sm", "Separation Multiplier of Boids, must be between 0 and 1", GH_ParamAccess.item, 0.1);
            pManager.AddCurveParameter("Attractor Curves", "AttC", "Curves to Attract the Boids to", GH_ParamAccess.list);
            pManager.AddNumberParameter("Attractor Multiplier", "AttM", "Attractor Strength, must be between -1 and 1", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Attractor Velocity", "AttV", "Attractor Velocity, must be between -1 and 1", GH_ParamAccess.item, 0.05);
            pManager.AddBooleanParameter("Reset", "R", "Reset Simpulation Button", GH_ParamAccess.item, false);
            Params.Input[6].Optional = true;
            Params.Input[9].Optional = true;
            Params.Input[6].DataMapping = GH_DataMapping.Flatten;
            Params.ParameterChanged += Params_ParameterChanged;
        }

        private void Params_ParameterChanged(object sender, GH_ParamServerEventArgs e)
        {
            if (e.ParameterSide == GH_ParameterSide.Input)
            {
                if (e.ParameterIndex == 0 || e.ParameterIndex == 1 || e.ParameterIndex == 6)
                {
                    SetupDone = false;
                }
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            Params.ParameterChanged -= Params_ParameterChanged;
            base.RemovedFromDocument(document);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Boid Positions", "P", "Positions of each Boid", GH_ParamAccess.list);
            pManager.AddVectorParameter("Boid Orientation", "O", "Velocity Vectors of each Boid", GH_ParamAccess.list);
            pManager.AddVectorParameter("Boid Acceleration", "A", "Acceleration Vectors of each Boid", GH_ParamAccess.list);
            pManager.AddCurveParameter("Boid Representations", "B", "Polyline Representations of the Boids", GH_ParamAccess.list);
            pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Box bounds = Box.Unset;
            List<Point3d> pointsInitial = new List<Point3d>();
            double pr = 15.0;
            double am = 0.1;
            double cm = 0.1;
            double sm = 0.1;
            List<Curve> geo = new List<Curve>();
            double gm = 0.1;
            double gv = 0.05;
            bool reset = false;
            if (!(DA.GetData(0, ref bounds))) return;
            if (!(DA.GetDataList(1, pointsInitial))) return;
            if (!(DA.GetData(2, ref pr))) return;
            if ((!(DA.GetData(3, ref am))) && ((0.0 < am) && (am < 1.0))) return;
            if ((!(DA.GetData(4, ref cm))) && ((0.0 < cm) && (cm < 1.0))) return;
            if ((!(DA.GetData(5, ref sm))) && ((0.0 < sm) && (sm < 1.0))) return;
            attract = (DA.GetDataList(6, geo));
            if ((!(DA.GetData(7, ref gm))) && ((-1.0 < gm) && (gm < 1.0))) return;
            if ((!(DA.GetData(8, ref gv))) && ((-1.0 < gv) && (gv < 1.0))) return;
            if ((DA.GetData(9, ref reset)))
            {
                if (reset) { SetupDone = false; }
            }
            if (!SetupDone)
            {
                Setup(bounds, pointsInitial, pr, am, cm, sm, geo, gm, gv);
            }
            else
            {
                Draw(DA, pr, am, cm, sm, gm, gv);
            }
        }

        private void Setup(Box bounds, List<Point3d> pointsInitial, double pr, double am, double cm, double sm, List<Curve> geo, double gm, double gv)
        {
            BoidsInstance = new Boids(bounds, pointsInitial, pr, am, cm, sm, gm, gv, this.Bounce);
            if (attract)
            {
                BoidsInstance.AttractorGeometry = geo;
            }
            SetupDone = true;
        }

        private void Draw(IGH_DataAccess DA, double pr, double am, double cm, double sm, double gm, double gv)
        {
            BoidsInstance.Perception = pr;
            BoidsInstance.AlignMultiplier = am;
            BoidsInstance.CohesionMultiplier = cm;
            BoidsInstance.SeparationMultiplier = sm;
            BoidsInstance.AttractionMultiplier = gm;
            BoidsInstance.AttractionVelocity = gv;
            List<Point3d> pOut = new List<Point3d>();
            List<Vector3d> vOut = new List<Vector3d>();
            List<Vector3d> aOut = BoidsInstance.Update();
            foreach (Boid b in BoidsInstance.BoidsList)
            {
                pOut.Add(b.Position);
                vOut.Add(b.Velocity);
            }
            DA.SetDataList(0, pOut);
            DA.SetDataList(1, vOut);
            DA.SetDataList(2, aOut);
            DA.SetDataList(3, BoidsInstance.Representations);
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
            get { return new Guid("417bae1e-8d24-4621-a1fe-ed6c794ab0af"); }
        }

        public class Boids
        {
            public Boids(Box bounds, List<Point3d> pointsInitial, double perception = 15.0, double alignMult = 0.1, double cohesionMult = 0.1, double separationMult = 0.1, double attractionMult = 0.1, double attractionVel = 0.05, bool mirror = false)
            {
                this.BoidsList = new List<Boid>();
                for (int i = 0; i < pointsInitial.Count; i++)
                {
                    this.BoidsList.Add(new Boid(pointsInitial[i], bounds, i, mirror));
                }
                this.Perception = perception;
                this.AlignMultiplier = alignMult;
                this.CohesionMultiplier = cohesionMult;
                this.SeparationMultiplier = separationMult;
                this.AttractorGeometry = new List<Curve>();
                this.AttractionMultiplier = attractionMult;
                this.AttractionVelocity = attractionVel;
                this.rep = new List<Polyline>();
            }
            public List<Boid> BoidsList { get; set; }
            public double Perception { get; set; }
            public double AlignMultiplier { get; set; }
            public double CohesionMultiplier { get; set; }
            public double SeparationMultiplier { get; set; }
            public List<Curve> AttractorGeometry { get; set; }
            public double AttractionMultiplier { get; set; }
            public double AttractionVelocity { get; set; }
            public List<Polyline> Representations
            {
                get
                {
                    return rep;
                }
            }
            private List<Polyline> rep;
            public List<Vector3d> Update()
            {
                List<Vector3d> aOut = new List<Vector3d>();
                this.rep.Clear();
                foreach (Boid b in this.BoidsList)
                {
                    Align(b, AlignMultiplier);
                    Cohesion(b, CohesionMultiplier);
                    Separation(b, SeparationMultiplier);
                    Attraction(b, AttractionMultiplier, AttractionVelocity);
                    Vector3d aV = b.Update();
                    rep.Add(GetRepresentation(b, aV));
                    aOut.Add(aV);
                }
                return aOut;
            }
            public void Align(Boid b, Double multiplier)
            {
                Vector3d AvgVelocity = new Vector3d(0.0, 0.0, 0.0);
                int Total = 0;
                foreach (Boid bOther in this.BoidsList)
                {
                    if (bOther.Position != b.Position)
                    {
                        if ((bOther.Position - b.Position).Length < this.Perception)
                        {
                            AvgVelocity += bOther.Velocity;
                            Total += 1;
                        }
                    }
                }
                if (Total > 0)
                {
                    AvgVelocity /= Total;
                }
                b.SteerTowards(AvgVelocity, multiplier);
            }

            public void Cohesion(Boid b, double multiplier)
            {
                Point3d AvgPosition = b.Position;
                int Total = 0;
                foreach (Boid bOther in this.BoidsList)
                {
                    if (bOther.Position != b.Position)
                    {
                        if ((bOther.Position - b.Position).Length < this.Perception)
                        {
                            AvgPosition += bOther.Position;
                            Total += 1;
                        }
                    }
                }
                if (Total > 0)
                {
                    AvgPosition /= Total;
                }
                Vector3d steerVector = AvgPosition - b.Position;
                b.SteerTowards(steerVector, multiplier);
            }

            public void Separation(Boid b, double multiplier)
            {
                Vector3d AvgProximity = new Vector3d(0.0, 0.0, 0.0);
                int Total = 0;
                foreach (Boid bOther in this.BoidsList)
                {
                    if (bOther.Position != b.Position)
                    {
                        Vector3d prox = (b.Position - bOther.Position);
                        if (prox.Length < this.Perception)
                        {
                            AvgProximity += prox;
                            Total += 1;
                        }
                    }
                }
                if (Total > 0)
                {
                    AvgProximity /= Total;
                }
                b.SteerTowards(AvgProximity, multiplier);
            }

            public void Attraction(Boid b, double multiplier, double velocity)
            {
                if (AttractorGeometry.Count > 0)
                {
                    Point3d attPoint = CurvesClosestPoint(b.Position, AttractorGeometry, velocity);
                    Vector3d attDir = (attPoint - b.Position);
                    b.SteerTowards(attDir, multiplier);
                }
            }

            public Polyline GetRepresentation(Boid b, Vector3d aV)
            {
                Polyline p = new Polyline();
                Point3d p2 = b.Position - b.Velocity;
                p.Add(p2);
                p.Add(b.Position);
                Point3d p3 = b.Position + aV;
                p.Add(p3);
                return p;
            }

            public static Point3d CurvesClosestPoint(Point3d point, List<Curve> curves, double v)
            {
                double minDist = -1.0;
                int closestIndex = -1;
                int i = 0;
                Point3d outPoint = point;
                foreach (Curve c in curves)
                {
                    Point3d p = CurveClosestPoint(point, c, v);
                    double d = (p - point).Length;
                    if (closestIndex == -1)
                    {
                        minDist = d;
                        closestIndex = i;
                        outPoint = p;
                    }
                    else
                    {
                        if (d < minDist)
                        {
                            minDist = d;
                            closestIndex = i;
                            outPoint = p;
                        }
                    }
                    i++;
                }
                return outPoint;
            }

            public static Point3d CurveClosestPoint(Point3d point, Curve curve, double v)
            {
                curve.ClosestPoint(point, out double t);
                return curve.PointAt(t + v);
            }
        }
        
        public class Boid
        {
            public Boid(Point3d initial, Box bounds, int seed, bool mirror = false)
            {
                this.Position = initial;
                this.Velocity = RandomVector(2.0, seed);
                this.Acceleration = new Vector3d(0, 0, 0);
                this.Bounds = bounds;
                this.MinVelocity = 1.5;
                this._Bounce = mirror;
            }
            public Point3d Position { get; set; }
            public Vector3d Velocity { get; set; }
            public Vector3d Acceleration { get; set; }
            public Box Bounds { get; set; }
            public double MinVelocity { get; set; }
            public bool _Bounce;
            public void SteerTowards(Vector3d desiredVelocityVector, double multiplier = 1.0)
            {
                Vector3d initialVelocityVector = this.Velocity;
                Vector3d a = desiredVelocityVector - initialVelocityVector;
                a *= multiplier;
                this.Acceleration += a;
            }
            public Vector3d Update()
            {
                this.Position = UpdatedPosition();
                this.Velocity = UpdatedVelocity();
                Vector3d a = this.Acceleration;
                this.Acceleration = new Vector3d(0, 0, 0);
                return a;
            }
            public Point3d UpdatedPosition()
            {
                Point3d p1 = this.Position;
                p1 += this.Velocity;
                p1 = p1.BoundPoint(this.Bounds, this._Bounce);
                return p1;
            }
            public Vector3d UpdatedVelocity()
            {
                Vector3d v1 = this.Velocity;
                v1 += this.Acceleration;
                if (v1.Length < this.MinVelocity)
                {
                    v1.Unitize();
                    v1 *= this.MinVelocity;
                }
                return v1;
            }
            public static Point3d RandomPosition(Box bounds, int seed)
            {
                Random rnd = new Random(seed);
                double x = bounds.X.ParameterAt(rnd.NextDouble());
                double y = bounds.Y.ParameterAt(rnd.NextDouble());
                double z = bounds.Z.ParameterAt(rnd.NextDouble());
                Point3d vOut = new Point3d(x, y, z);
                return vOut;
            }
            public static Vector3d RandomVector(Interval MagnitudeRange)
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                double x = rnd.NextDouble();
                double y = rnd.NextDouble();
                double z = rnd.NextDouble();
                double Magnitude = MagnitudeRange.ParameterAt(rnd.NextDouble());
                Vector3d vOut = new Vector3d(x, y, z);
                vOut.Unitize();
                vOut *= Magnitude;
                return vOut;
            }
            public static Vector3d RandomVector(double Magnitude, int seed)
            {
                Random rnd = new Random(DateTime.Now.Millisecond + seed);
                double x = rnd.NextDouble();
                double y = rnd.NextDouble();
                double z = rnd.NextDouble();
                Vector3d vOut = new Vector3d(x, y, z);
                vOut.Unitize();
                vOut *= Magnitude;
                return vOut;
            }
            public static Vector3d RandomVector(bool unitVector = false)
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                double x = rnd.NextDouble();
                double y = rnd.NextDouble();
                double z = rnd.NextDouble();
                Vector3d vOut = new Vector3d(x, y, z);
                if (unitVector)
                {
                    vOut.Unitize();
                }
                return vOut;
            }
        }
    }
}