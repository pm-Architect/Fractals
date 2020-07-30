using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.DocObjects;

namespace Fractals
{
    public class KochComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public KochComponent()
          : base("Koch Snowflake", "Snow",
              "Koch Snowflake Fractal",
              "Fractals", "2D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Sides", "S", "Number of sides", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("Iter", "I", "Number of iterations", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("Length", "L", "Side length", GH_ParamAccess.item, 5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Curve", "C", "Curve", GH_ParamAccess.list);
            pManager.AddPointParameter("Vertices", "v", "Vertices", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int sides = 3;
            int iter = 3;
            int length = 5;
            if (!DA.GetData(0, ref sides)) return;
            if (!DA.GetData(1, ref iter)) return;
            if (!DA.GetData(2, ref length)) return;
          
            if (sides <= 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of sides must be greater than 2.");
                return;
            }
            if (iter < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of iterations must be equal to or greater than 0.");
                return;
            }
            if (length <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Length of side must be a natural number.");
                return;
            }

            Double l = length;
            Double s = sides;
            List<Point3d> VerticesOut = new List<Point3d>();
            List<LineCurve> CurvesOut = new List<LineCurve>();


            List<Point3d> points = new List<Point3d>();
            List<LineCurve> segments = new List<LineCurve>();

            // making the initial polygon

            Point3d p0 = new Point3d(l / (2 * (Math.Cos(Math.PI * (s - 2) / (2 * s)))), 0, 0);
            points.Add(p0);
            for (var i = 0; i < s-1; i++)
            {
                Point3d p1 = new Point3d (p0);
                p1.Transform(Transform.Rotation(2 * (i + 1) * (Math.PI * (s - 2)) / ((s - 2) * s),new Vector3d(0,0,1), new Point3d(0,0,0)));
                points.Add(p1);
            }
            VerticesOut = points;
            for (var i = 0; i < s; i++)
            {
                if (i != s - 1)
                {
                    LineCurve pl = new LineCurve(points[i], points[i + 1]);
                    segments.Add(pl);
                }
                else
                {
                    LineCurve pl = new LineCurve(points[i], points[0]);
                    segments.Add(pl);
                }
                
            }
            for (var i = 0; i < iter; i++)
            {
                List<LineCurve> frag = new List<LineCurve>();
                for (var j = 0; j < segments.Count; j++)
                {
                    Point3d[] p;
                    segments[j].DivideByCount(3, true, out p);
                    for (var k = 0; k < 2; k++)
                    {
                        points.Insert(j * 3 + k + 1, p[k + 1]);
                    }
                    for (var m = 0; m < 3; m++)
                    {
                        if (m + 1 + 3*j == points.Count)
                        {
                            LineCurve pll = new LineCurve(points[m + 3 * j], points[0]);
                            frag.Add(pll);
                        }
                        else
                        {
                            LineCurve pll = new LineCurve(points[m + 3 * j], points[m + 3 * j + 1]);
                            frag.Add(pll);
                        }
                    }
                }
                int a = 0;
                int w = 0;
                LineCurve copie = new LineCurve();
                for(var n = 0; n<frag.Count; n++)
                {
                    if (n % 3 == 1)
                    {
                        a = a + 1;
                        frag[n].Transform(Transform.Rotation(-Math.PI * ((s - 2) / s), new Vector3d(0, 0, 1), points[n + a + w - 1]));
                        Point3d[] o;
                        frag[n].DivideByCount(1, true, out o);
                        Point3d np = new Point3d(o[1]);
                        points.Insert(n + a + w, np);
                        if (s > 3)
                        {
                            for (var zz = 0; zz < s-3; zz++)
                            {
                                if (zz == 0)
                                {
                                    LineCurve copy = new LineCurve(frag[n]);
                                    copie = copy;
                                }
                                Vector3d translation = new Vector3d(points[n + a + w - 1] - points[n + a + w]);
                                copie.Transform(Transform.Translation(translation));
                                copie.Transform(Transform.Rotation(-Math.PI * ((s - 2) / s), new Vector3d(0, 0, 1), points[n + a + w]));
                                Point3d[] yy;
                                copie.DivideByCount(1, true, out yy);
                                Point3d nps = new Point3d(yy[1]);
                                points.Insert(n + a + w + 1, nps);
                                w = w + 1;
                            }
                        }

                    }
                }
                List<LineCurve> finall = new List<LineCurve>();
                for (var q = 0; q < points.Count; q ++)
                {
                    if (q == points.Count - 1)
                    {
                        LineCurve plll = new LineCurve(points[q], points[0]);
                        finall.Add(plll);
                    }
                    else
                    {
                        LineCurve plll = new LineCurve(points[q], points[q + 1]);
                        finall.Add(plll);
                    }
                }
                segments = finall;

            }


            CurvesOut = segments;
            VerticesOut = points;

            DA.SetDataList(0, CurvesOut);
            DA.SetDataList(1, VerticesOut);
            
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
            get { return new Guid("3d6c256a-bbaa-47e1-898f-b2938fb92291"); }
        }
    }
}