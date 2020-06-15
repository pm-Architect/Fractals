using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Fractals
{
    public class SunflowerComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SunflowerComponent class.
        /// </summary>
        public SunflowerComponent()
          : base("Vogel Sunflower", "Sunflower",
              "Vogel Sunflower Fractal",
              "Fractals", "2D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Plane", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("Number of Iterations", "N", "Number of Iterations", GH_ParamAccess.item, 3000);
            pManager.AddIntegerParameter("Degree", "D", "Degree", GH_ParamAccess.item, 5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius Multiplier", "R", "Radius Multiplier for circles at points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = Plane.WorldXY;
            int iterations = 3000;
            int deg = 5;
            if (!DA.GetData(0, ref plane)) return;
            if (!DA.GetData(1, ref iterations)) return;
            if (!DA.GetData(2, ref deg)) return;
            if (deg < 1.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Degree must be greater than 1");
                return;
            }
            if ((Math.Sqrt(deg) - (Math.Floor(Math.Sqrt(deg)))) == 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Degree must not be a perfect square");
                return;
            }
            double c = (Math.Sqrt(deg) + 1) / 2;
            Vector3d trans = Vector3d.Unset;
            List<Plane> pointsOut = new List<Plane>();
            List<double> radiiOut = new List<double>();
            for (var i = 0; i < iterations; i++)
            {
                double r = Math.Pow((double)i, c) / (iterations / 2);
                double angle = 2 * Math.PI * c * i;
                double x = (r * Math.Sin(angle) + (iterations));
                double y = (r * Math.Cos(angle) + (iterations));
                Point3d p = new Point3d(x, y, 0.0);
                if (i == 0)
                    trans = (Point3d.Origin - p);
                p.Transform(new Grasshopper.Kernel.Types.Transforms.Translation(trans).ToMatrix());
                Point3d p1 = plane.PointAt(p.X, p.Y, p.Z);
                Plane plane1 = new Plane(p1, plane.XAxis, plane.YAxis);
                pointsOut.Add(plane1);
                double radius = (double)i / (double)iterations;
                radiiOut.Add(radius);
            }
            DA.SetDataList(0, pointsOut);
            DA.SetDataList(1, radiiOut);
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
            get { return new Guid("d20b9365-83ed-4636-91ef-da2deaa6e7cd"); }
        }
    }
}