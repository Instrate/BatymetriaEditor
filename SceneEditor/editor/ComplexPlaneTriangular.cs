using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Smoothing;

namespace SceneEditor.editor
{

    public class ComplexPlaneTriangular : Transformable
    {
        Triangle[]? connections = null;
        public bool showGeometry = false;

        public Vector3[] data;

        Dot[] dots;
        public bool showDots = true;

        private alglib.spline2dinterpolant? interp = null;

        //public new bool isEnabled;

        int primitiveCurrent = 1;
        float lineWidth;
        PrimitiveType[] stylesSwitcher = new PrimitiveType[]
            {
                PrimitiveType.Triangles,
                PrimitiveType.LineStrip,
                PrimitiveType.Points
            };
        public PrimitiveType drawStyle;

        public ComplexPlaneTriangular(Vector3[]? inputData = null,
                                      float[]? X = null,
                                      float[]? Y = null,
                                      float[]? Z = null,
                                      string[]? textureSet = null,
                                      bool shouldTriangulate = true)
        {
            if(inputData == null)
            {
                if(X == null || Y == null || Z == null)
                {
                    if(shouldTriangulate)
                        Functions.GenerateRandomPoints(out data, start: -10f, end: 10f, amountLow:100, amountHigh: 200);
                    else
                        Functions.GenerateRandomPoints(out data, start: -10f, end: 10f, amountLow: 1000, amountHigh: 3000);
                    data = Functions.FixLowHight(data);
                }
            }
            else
            {

            }
            lineWidth = 1;
            dots = new Dot[data.Length];
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i] = new Dot(data[i], ((Vector4)Color4.Red).Xyz, 6);
            }

            interp = Functions.Interpolate(data);

            //_generateFastTriangulationFromData();
            if (shouldTriangulate)
            {
                generateDelaunayTriangulationFromData();
            }

            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for (int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                }
            }

            isEnabled = true;
            drawStyle = stylesSwitcher[primitiveCurrent];
        }

        public void generateDelaunayTriangulationFromData()
        {
            Polygon poly = new Polygon();

            for(int i = 0; i < data.Length - 1; i++)
            {
                poly.Add(new Vertex(data[i].X, data[i].Y));
                poly.Add(new Segment(
                        new Vertex(data[i].X, data[i].Y),
                        new Vertex(data[i+1].X, data[i + 1].Y)
                        ));
            }
            poly.Add(new Vertex(data[data.Length - 1].X, data[data.Length - 1].Y));
            poly.Add(new Segment(
                        new Vertex(data[data.Length - 1].X, data[data.Length - 1].Y),
                        new Vertex(data[0].X, data[0].Y)
                        ));

            var Coptions = new ConstraintOptions();
            //Coptions.UseRegions = true;
            Coptions.Convex = true;
            //Coptions.SegmentSplitting = 10;
            //Coptions.ConformingDelaunay = true;

            var Qoptions = new QualityOptions();
            //Qoptions.MinimumAngle = 30;
            //Qoptions.MaximumAngle = 60;

            var mesh = poly.Triangulate(Coptions, Qoptions);

            List<Vector2> trgs = new List<Vector2>();
            List<int> indices = new List<int>();

            foreach (var t in mesh.Triangles)
            {
                for (int j = 2; j >= 0; j--)
                {
                    int amount = trgs.Count;
                    bool found = false;
                    var vx = t.GetVertex(j);
                    for (int i = 0; i < amount && !found; i++)
                    {
                        if((trgs[i].X == vx.X) && (trgs[i].Y == vx.Y))
                        {
                            indices.Add(i);
                            found = true;
                        }

                    }
                    if (!found)
                    {
                        trgs.Add(new Vector2((float)vx.X, (float)vx.Y));
                        indices.Add(amount);
                    }
                }
            }

            int[] iRes = indices.ToArray();

            Vector3[] vector3s = Functions.recalculateBySpline(interp, trgs.ToArray());

            //Use.ShowVectors("res", vector3s);

            //dots = new Dot[vector3s.Length];
            //for (int i = 0; i < dots.Length; i++)
            //{
            //    dots[i] = new Dot(vector3s[i], ((Vector4)Color4.LimeGreen).Xyz, 8);
            //}

            connections = new Triangle[iRes.Length/3];

            for (int i = 0; i < connections.Length; i++)
            {
                Vector3[] points = new Vector3[3];
                for (int j = 0; j < 3; j++)
                {
                    points[j] = new Vector3(vector3s[iRes[i * 3 + j]]);
                }
                connections[i] = new Triangle(points);
            }

            showGeometry = true;
        }

        private void _generateFastTriangulationFromData(int amountOfConnections = 6)
        {
            Vector3[][] cons = new Vector3[data.Length][];

            float maxDist = 0;
            float minDist = (data[0].Xy - data[1].Xy).Length;

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data.Length; j++)
                {
                    if (i != j)
                    {
                        Vector2 test = data[i].Xy - data[j].Xy;
                        float length = test.Length;
                        if (length > maxDist)
                        {
                            maxDist = length;
                        }
                        else
                        {
                            if (length < minDist)
                            {
                                minDist = length;
                            }
                        }
                    }
                }
            }

            List<int> points;

            for (int i = 0; i < data.Length; i++)
            {
                points = new List<int>();
                points.Add(i);

                int count = 0;
                
                float dist = minDist;

                while (count < amountOfConnections && dist <= maxDist)
                {
                    float minNext = maxDist;
                    for (int j = 0; j < data.Length; j++)
                    {
                        if (i != j && !points.Contains(j))
                        {
                            Vector2 temp = data[i].Xy - data[j].Xy;
                            float length = temp.Length;
                            if(length <= dist)
                            {
                                points.Add(j);
                                count++;
                            }
                            else
                            {
                                if(length < minNext)
                                {
                                    minNext = length;
                                }
                            }
                        }
                    }
                    dist = minNext;
                }

                int[] res = points.ToArray();
                cons[i] = new Vector3[res.Length];
                for(int j = 0; j < res.Length; j++)
                {
                    cons[i][j] = data[res[j]];
                }
            }

            // ?!
            //_ShowConnections(cons);
            _generateTrianglesFromPoints(cons);
        }

        private void _generateTrianglesFromPoints(Vector3[][] cons)
        {
            List<Triangle> trgs = new List<Triangle>();

            for(int i = 0; i < cons.Length; i++)
            {
                for (int j = 0; j < cons[i].Length - 2; j++)
                {
                    trgs.Add(new Triangle(new Vector3[]
                    {
                        cons[i][j],
                        cons[i][j + 1],
                        cons[i][j + 2]
                    }));
                }
            }
            connections = trgs.ToArray();
        }

        private void _ShowConnections(Vector3[][] cons)
        {
            Console.WriteLine("\n Connections:");
            for(int i = 0; i < cons.Length; i++)
            {
                Console.WriteLine("[" + i + "]: " + cons[i][0].ToString());
                Vector2 parent = cons[i][0].Xy;
                for (int j = 1; j < cons[i].Length; j++)
                {
                    Vector2 child = cons[i][j].Xy;
                    Console.WriteLine("\t[" + j + "]: " + cons[i][j].ToString() + " - " + (parent - child).Length);
                }
                Console.WriteLine();
            }
        }

        public ComplexPlaneTile ConvertToTiledByInterpolation(int limitSide = 100, bool copyCurrentTexturePack = false)
        {
            float xmin = data[0].X;
            float xmax = xmin;
            float ymin = data[0].Y;
            float ymax = ymin;

            float mindifx = float.MaxValue;
            float mindify = float.MaxValue;

            for (int i = 1; i < data.Length; i++)
            {
                float vx = data[i].X;
                float vy = data[i].Y;
                float lengthx = MathF.Abs(data[i - 1].X - data[i].X);
                float lengthy = MathF.Abs(data[i - 1].Y - data[i].Y);
                if (mindifx > lengthx)
                {
                    mindifx = lengthx;
                }
                if (mindify > lengthy)
                {
                    mindify = lengthy;
                }

                if (xmin > vx)
                {
                    xmin = vx;
                }
                else
                {
                    if(xmax < vx)
                    {
                        xmax = vx;
                    }
                }
                if (ymin > vy)
                {
                    ymin = vy;
                }
                else
                {
                    if (ymax < vy)
                    {
                        ymax = vy;
                    }
                }
            }

            float dividerx = (xmax - xmin) / limitSide / 2;
            float dividery = (ymax - ymin) / limitSide / 2;
            float[] X = Functions.Arrange(xmin, xmax, (xmax - xmin) / mindifx > limitSide ? dividerx : mindifx);
            float[] Y = Functions.Arrange(ymin, ymax, (ymax - ymin) / mindify > limitSide ? dividery : mindify);
            float[][] Z = Functions.recalculateBySpline(interp, X, Y);

            return new ComplexPlaneTile(X: X, Y: Y, Z:Z,
                textureHandlersCopy: copyCurrentTexturePack ? (textureHandlers != null ? textureHandlers : null) : null
                );
        }

        public void SwitchDrawStyle()
        {
            primitiveCurrent++;
            if (stylesSwitcher.Length == primitiveCurrent)
            {
                primitiveCurrent = 0;
            }
            drawStyle = stylesSwitcher[primitiveCurrent];
        }

        private protected override void _renderObjects(int shaderHandle, PrimitiveType? primitive)
        {
            if (showDots)
            {
                for (int i = 0; i < dots.Length; i++)
                {
                    dots[i].Render(shaderHandle);
                }
            }

            if (connections != null && showGeometry)
            {
                GL.LineWidth(lineWidth);
                GL.PointSize(lineWidth * 5);
                for (int i = 0; i < connections.Length; i++)
                {
                    connections[i].Render(shaderHandle, drawStyle);
                }
            }
        }
    }
}
