using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Smoothing;

namespace SceneEditor.editor
{
    public class ComplexPlaneTriangular : Transformable
    {
        Triangle[]? connections = null;
        private TriangularedDataSet? dataTriangulared = null;
        public bool showGeometry = false;

        public Vector3[] data;
        public List<Vector3> dataSetAdjustable;

        Dot[] dots;
        private int dotsize = 6;
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
            if (inputData == null)
            {
                if (X == null || Y == null || Z == null)
                {
                    Functions.GenerateRandomPoints(out data, start: -20f, end: 20f, amountLow: 100, amountHigh: 200);
                    //if (shouldTriangulate)
                    //    Functions.GenerateRandomPoints(out data, start: -20f, end: 20f, amountLow:100, amountHigh: 200);
                    //else
                    //Functions.GenerateRandomPoints(out data, start: -20f, end: 20f, amountLow: 1000, amountHigh: 3000);
                    data = Functions.FixLowHight(data);
                }
            }
            else
            {
                data = inputData;
            }

            dataSetAdjustable = data.ToList();
            lineWidth = 1;

            UpdateVertices();

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

        public ComplexPlaneTriangular(TriangularedDataSet dataSet,
                                      string[]? textureSet = null)
        {
            connections = new Triangle[dataSet.Triangles.Length];
            dataSetAdjustable = new();
            for (int i = 0; i < connections.Length; i++)
            {
                Vector3[] points = dataSet.Triangles[i].ToVector3Set();
                connections[i] = new Triangle(points);
                for (int j = 0; j < points.Length; j++)
                {
                    dataSetAdjustable.Add(points[j]);
                }
            }

            lineWidth = 1;
            UpdateVertices();

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

        public void UpdateVertices()
        {
            data = dataSetAdjustable.ToArray();
            dots = new Dot[data.Length];
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i] = new Dot(data[i], ((Vector4)Color4.Red).Xyz, dotsize);
            }
        }

        private Polygon _createPolyFromPoints()
        {
            Polygon poly = new Polygon();

            for (int i = 0; i < data.Length - 1; i++)
            {
                poly.Add(new Vertex(data[i].X, data[i].Y));
                poly.Add(new Segment(
                        new Vertex(data[i].X, data[i].Y),
                        new Vertex(data[i + 1].X, data[i + 1].Y)
                        ));
            }
            poly.Add(new Vertex(data[data.Length - 1].X, data[data.Length - 1].Y));
            poly.Add(new Segment(
                        new Vertex(data[data.Length - 1].X, data[data.Length - 1].Y),
                        new Vertex(data[0].X, data[0].Y)
                        ));
            return poly;
        }

        public void generateDelaunayTriangulationFromData(bool isConvex = false, bool fixArea = false, bool accurateTriangulation = false)
        {
            interp = Functions.Interpolate(data);
            var Coptions = new ConstraintOptions();

            Coptions.Convex = isConvex;
            if (accurateTriangulation)
            {
                Coptions.ConformingDelaunay = true;
            }
            var Qoptions = new QualityOptions();
            if (fixArea)
            {
                Qoptions.MaximumArea = (Math.Sqrt(3) / 4 * 0.2 * 0.2) * 1.45;
            }

            var mesh = _createPolyFromPoints().Triangulate(Coptions, Qoptions);

            if (accurateTriangulation && fixArea)
            {
                try
                {
                    var smoother = new SimpleSmoother();
                    smoother.Smooth(mesh, 25);
                    _readConnectionsFromMesh(mesh);
                }
                catch (Exception ex)
                {
                    _readConnectionsFromMesh(mesh);
                    MessageBox.Show("Failed to make mesh smoother");
                }
            }
            else
            {
                _readConnectionsFromMesh(mesh);
            }
        }

        public void HiglightDot(int index)
        {
            for (int i = 0; i < index; i++)
            {
                dots[i] = new Dot(dots[i].position, ((Vector4)Color4.Red).Xyz, dotsize);
            }
            dots[index] = new Dot(dots[index].position, new Vector3(0, 255, 0), dotsize * 3);
            for (int i = index + 1; i < dots.Length; i++)
            {
                dots[i] = new Dot(dots[i].position, ((Vector4)Color4.Red).Xyz, dotsize);
            }
        }

        private void _readConnectionsFromMesh(IMesh mesh)
        {
            List<Vector2> trgs = new();
            List<int> indices = new();

            foreach (var t in mesh.Triangles)
            {
                for (int j = 2; j >= 0; j--)
                {
                    int amount = trgs.Count;
                    bool found = false;
                    var vx = t.GetVertex(j);
                    for (int i = 0; i < amount && !found; i++)
                    {
                        if ((trgs[i].X == vx.X) && (trgs[i].Y == vx.Y))
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

            connections = new Triangle[iRes.Length / 3];
            TriangleInfo[] triangles = new TriangleInfo[connections.Length];
            for (int i = 0; i < connections.Length; i++)
            {
                Vector3[] points = new Vector3[3];
                for (int j = 0; j < 3; j++)
                {
                    points[j] = new(vector3s[iRes[i * 3 + j]]);
                }
                triangles[i] = new(points);
                connections[i] = new(points);
            }
            dataTriangulared = new(triangles);
            showGeometry = true;
        }

        private void _ShowConnections(Vector3[][] cons)
        {
            Console.WriteLine("\n Connections:");
            for (int i = 0; i < cons.Length; i++)
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

        public ComplexPlaneTile ConvertToTiledByInterpolation(int limitSide = 200, bool copyCurrentTexturePack = false)
        {
            if (interp == null)
            {
                interp = Functions.Interpolate(data);
            }
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
                    if (xmax < vx)
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

            return new ComplexPlaneTile(X: X, Y: Y, Z: Z,
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

        public TriangularedDataSet? ExportPointDataSet()
        {
            return dataTriangulared;
        }

        public Vector3[]? ExportPoints()
        {
            return data;
        }

        private protected override void _renderObjects(Shader shader, PrimitiveType? primitive)
        {
            if (showDots)
            {
                for (int i = 0; i < dots.Length; i++)
                {
                    dots[i].Render(shader);
                }
            }

            if (connections != null && showGeometry)
            {
                GL.LineWidth(lineWidth);
                GL.PointSize(lineWidth * 5);
                for (int i = 0; i < connections.Length; i++)
                {
                    connections[i].Render(shader, drawStyle);
                }
            }
        }
    }
}
