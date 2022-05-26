using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace SceneEditor.editor
{
    public static class Use
    {
        public static void ShowArray(string name, float[] arr)
        {
            Console.WriteLine("Array " + name + ":\n");
            for (int i = 0; i < arr.Length; i++)
            {
                Console.WriteLine("[" + i.ToString() + "]: " + arr[i].ToString() + "");
            }
            Console.WriteLine("\n");
        }

        public static void ShowVectors(string name, Vector3[] v)
        {
            Console.WriteLine("Vectors " + name + ":\n");
            for (int i = 0; i < v.Length; i++)
            {
                Console.WriteLine("[" + i.ToString() + "]: " + v[i].ToString() + "");
            }
            Console.WriteLine("\n");
        }

        public static void ShowArray(string name, float[][] arr)
        {
            Console.WriteLine("Matrix " + name + ":\n");
            for (int i = 0; i < arr.Length; i++)
            {
                string line = "";
                for (int j = 0; j < arr[i].Length; j++)
                {
                    line += " " + arr[i][j].ToString();
                    if ((j + 1) % 15 == 0)
                    {
                        line += " \\/\n";
                    }
                }
                Console.WriteLine(line + "\n");
            }
            Console.WriteLine("\n");
        }
    }

    public class LineFunction
    {
        // x - x1   y - y1   z - z1
        // ------ = ------ = ------
        //   m1       p1       l1
        public float x1, m1, y1, p1, z1, l1;

        public Vector3 start;
        public Vector3 end;

        public LineFunction(float[] chars, bool norm = false)
        {
            this.x1 = chars[0];
            this.y1 = chars[2];
            this.z1 = chars[4];

            this.m1 = chars[1];
            this.p1 = chars[3];
            this.l1 = chars[5];

            if (!norm)
            {
                m1 -= x1;
                p1 -= y1;
                l1 -= z1;
            }

            start = new Vector3(x1, y1, z1);
            end = new Vector3(m1 + x1, p1 + y1, l1 + z1);
        }

        public LineFunction(Vector3 a, Vector3 b)
        {
            start = a;
            end = b;

            x1 = a.X;
            y1 = a.Y;
            z1 = a.Z;

            m1 = b.X - x1;
            p1 = b.Y - y1;
            l1 = b.Z - z1;
        }
 
        public Vector3? Intersect(float A, float B, float C, float D)
        {
            Vector3 n1 = new Vector3(A, B, C); 
            Vector3 n2 = new Vector3(m1, p1, l1);

            if((A*m1 + B*p1 + C*l1) / (n1.Length * n2.Length) == 0)
            {
                return new Vector3(x1, y1, z1);
            }

            float[][] arr = new float[3][];
            float[] right = new float[3];
            for (int i = 0; i < 3; i++)
            {
                arr[i] = new float[3];
            }
            arr[0][0] = p1;
            arr[0][1] = -m1;
            arr[0][2] = 0;

            arr[1][0] = 0;
            arr[1][1] = l1;
            arr[1][2] = -p1;

            arr[2][0] = A;
            arr[2][1] = B;
            arr[2][2] = C;

            right[0] = p1 * x1 - m1 * y1;
            right[1] = l1 * y1 - p1 * z1;
            right[2] = -D;

            float[]? X = Functions.MatrixSolution(arr, right);
            if (X != null)
            {
                return new Vector3(X[0], X[1], X[2]);
            }
            return null;
        }

        public bool DotOnLine(Vector3 dot)
        {
            float eps = MathF.Pow(10, -4);
            float x = (dot.X - x1);
            float y = (dot.Y - y1);
            float z = (dot.Z - z1);

            float dz = MathF.Abs(start.Z - end.Z);
            float dz1 = MathF.Abs(dot.Z - MathF.Min(start.Z, end.Z));
            if(dz1 > dz || dot.Z < MathF.Min(start.Z, end.Z))
            {
                return false;
            }

            List<float> c = new List<float>();
            if(m1 != 0)
            {
                c.Add(x/m1);
            }
            if(p1 != 0)
            {
                c.Add(y / p1);
            }
            if(l1 != 0)
            {
                c.Add(z / l1);
            }
            float[] arr = c.ToArray();
            switch (arr.Length)
            {
                case 1: return true;
                case 2: {
                        return Math.Abs(arr[0] - arr[1]) < eps;
                    };
                default: return Math.Abs(arr[0] - arr[1]) < eps && Math.Abs(arr[1] - arr[2]) < eps;
            }
        }

        private Vector3 _RotateDot(Vector3 dot, float angle)
        {
            float x = dot.X;
            float y = dot.Y;
            dot.X = -MathF.Sin(angle) * y + MathF.Cos(angle) * x;
            dot.Y = MathF.Cos(angle) * y + MathF.Sin(angle) * x;
            return dot;
        }

        public LineFunction Rotate(float angle)
        {
            return new LineFunction(_RotateDot(start, angle), _RotateDot(end, angle));
        }

        public float AngleBetweenLines(Vector3 n)
        {
            Vector3 v = new Vector3(m1, p1, l1);

            float cos = Vector3.Dot(v, n) / v.Length / n.Length;
            return MathF.Acos(cos);
        }
    }

    public class PlaneFunction
    {
        public float A;
        public float B;
        public float C;
        public float D;


        public PlaneFunction(Vector3 a, Vector3 b)
        {
            Vector3 c = new Vector3(b.X, b.Y, a.Z);

            float[] arr = new float[6];
            arr[0] = b.X - a.X; arr[1] = b.Y - a.Y; arr[2] = b.Z - a.Z;

            arr[3] = c.X - a.X; arr[4] = c.Y - a.Y; arr[5] = c.Z - a.Z;

            A = arr[1] * arr[5] - arr[2] * arr[4];
            B = -(arr[0] * arr[5] - arr[2] * arr[3]);
            C = arr[0] * arr[4] - arr[1] * arr[3];
            D = A * (-1f) * a.X + B * (-1f) * a.Y + C * (-1f) * a.Z;

        }

        public PlaneFunction(float A, float B, float C, float D)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        public void ShowFunction()
        {
            Console.WriteLine("Plane function:");
            Console.WriteLine("(" + A + ")x+(" + B + ")y+(" + C + ")z+(" + D + ")= 0");
            Console.WriteLine();
        }
    }

    public class Section : IRenderable
    {
        private int[]? textureHandlers = null;

        private Square sqr;
        public bool isEnabled = false;


        public Vector3[] chars = new Vector3[2];

        private PlaneFunction plane;

        private Dot[] dots;

        private Line[] areaMesh;
        public bool showAreaMesh = false;

        private Line[] funcLine;

        private Line[] funcPolar;
        private bool hasPolar = false;
        public bool showPolar = false;

        private float[][] funcPolarArgs;
        //Line[] funcSphere;
        //bool hasSphere = false;
        //float[][] funcSphereArgs;

        public bool intersected = false;

        public Section(
            Vector3 start,
            Vector3 end,
            string[]? textureSet = null,
            Vector3? color = default
            )
        {
            Replace(start, end);

            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for (int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                }
            }
        }

        public void Replace(Vector3 start, Vector3 end)
        {
            chars[0] = start;
            chars[1] = end;

            plane = new PlaneFunction(start, end);

            Vector2 v1 = new Vector2(start.X, start.Y);
            Vector2 v2 = new Vector2(end.X, end.Y);

            Vector4 heights = new Vector4(end.Z, start.Z, start.Z, end.Z);

            sqr = new Square(builder: new Vector2[] { v1, v2 }, heights: heights, isSection: true);

            showAreaMesh = false;
            intersected = false;
            showPolar = false;
        }

        // recheck formulae
        public Vector3[]? IntersectTiled(float[] X, float[] Y, float[][] Data)
        {


            int cols = X.Length - 1;
            int rows = Y.Length - 1;

            //float dx = X[1] - X[0];
            //float dy = Y[1] - Y[0];

            // hard stuff appears
            float xmax = MathF.Max(chars[0].X, chars[1].X);
            xmax = X[cols] < xmax ? X[cols] : xmax;

            float xmin = MathF.Min(chars[0].X, chars[1].X);
            xmin = X[0] > xmin ? X[0] : xmin;

            float ymax = MathF.Max(chars[0].Y, chars[1].Y);
            ymax = Y[rows] < ymax ? Y[rows] : ymax;

            float ymin = MathF.Min(chars[0].Y, chars[1].Y);
            ymin = Y[0] > ymin ? Y[0] : ymin;

            if (xmax < xmin)
            {
                float temp = xmin;
                xmin = xmax;
                xmax = temp;
            }
            if (ymax < ymin)
            {
                float temp = ymin;
                ymin = ymax;
                ymax = temp;
            }

            int iis = 0;
            int iie = 0;
            int jjs = 0;
            int jje = 0;

            for (iis = 0; iis < cols && xmin > X[iis]; iis++) ;
            for (iie = jjs; iie < cols && xmax > X[iie]; iie++) ;
            for (jjs = 0; jjs < rows && ymin > Y[jjs]; jjs++) ;
            for (jje = jjs; jje < rows && ymax > Y[jje]; jje++) ;

            float[] x = new float[iie - iis + 1];
            float[] y = new float[jje - jjs + 1];
            float[][] data = new float[x.Length][];
            for (int i = 0; i < x.Length; i++)
            {
                data[i] = new float[y.Length];
            }

            //fill data
            for (int j = jjs; j <= jje; j++)
            {
                y[j - jjs] = Y[j];
            }

            for (int i = iis; i <= iie; i++)
            {
                x[i - iis] = X[i];
                for (int j = jjs; j <= jje; j++)
                {
                    data[i - iis][j - jjs] = Data[i][j];
                }
            }

            //Use.ShowArray("X", x);
            //Use.ShowArray("Y", y);
            //Use.ShowArray("Data", data);

            // get lines' params
            int amount = (x.Length - 1) * y.Length + (y.Length - 1) * x.Length;
            LineFunction[] lines = new LineFunction[amount];

            for (int i = 0; i < x.Length - 1; i++)
            {
                for (int j = 0; j < y.Length; j++)
                {
                    lines[i * y.Length + j] = new LineFunction(new float[] { x[i], x[i + 1], y[j], y[j], data[i][j], data[i + 1][j] });
                }
            }

            for (int i = 0; i < x.Length; i++)
            {
                for (int j = 0; j < y.Length - 1; j++)
                {
                    lines[(x.Length - 1) * y.Length + i * (y.Length - 1) + j] = new LineFunction(new float[] { x[i], x[i], y[j], y[j + 1], data[i][j], data[i][j + 1] });
                }
            }

            // create working area mesh
            _assignAreaMesh(lines);


            // convert result
            List<Vector3> res = new List<Vector3>();
            for (int i = 0; i < amount; i++)
            {
                LineFunction line = lines[i];
                Vector3? pos = line.Intersect(plane.A, plane.B, plane.C, plane.D);

                if (pos != null && line.DotOnLine(pos.Value)) 
                {
                    res.Add(new Vector3(pos.Value));
                }
            }

            // create dots for the result displaying
            amount = res.Count;
            if (amount != 0)
            {
                intersected = true;
                dots = new Dot[amount];
                funcLine = new Line[amount - 1];
                Vector3[] intersects = res.ToArray();

                for (int i = 0; i < amount; i++)
                {
                    dots[i] = new Dot(intersects[i], color: ((Vector4)Color4.Red).Xyz, size: 6);

                    if(i != 0)
                    {
                        funcLine[i - 1] = new Line(dots[i - 1].position, dots[i].position, color: ((Vector4)Color4.Yellow).Xyz, width: 5);
                    }
                }

                CountPolarFunction();
                return intersects;
            }
            return null;
        }

        private void _assignAreaMesh(LineFunction[] lines)
        {
            areaMesh = new Line[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                areaMesh[i] = new Line(lines[i].start, lines[i].end, ((Vector4)Color4.Cyan).Xyz, width: 3);
            }
            showAreaMesh = true;
        }

        private float _countAngleToRotateYZ(Vector3 a, Vector3 b)
        {
            LineFunction line = new LineFunction(a, b);
            LineFunction line1 = new LineFunction(new Vector3(0, 1, a.Z), new Vector3(0, 0, b.Z));
            return (line1.AngleBetweenLines(new Vector3(line.m1, line.p1, line.l1)) - MathF.PI / 2) / 2;
        }

        public void ShowPolarFuncs()
        {
            if (intersected)
            {
                Console.WriteLine("Polar functions:\n\tx(r,phi)=r*cos(phi);\n\ty(r,phi)=r*sin(phi);\n");
                for(int i = 0; i < funcPolarArgs.Length; i++)
                {
                    float r = funcPolarArgs[i][0];
                    float p = funcPolarArgs[i][1];
                    Console.WriteLine("[" + i + "]:\n\tx=" + r + "*cos(" + p + ");\n\ty=" + r + "*sin(" + p + ");\n");
                }
            }
            else
            {
                Console.WriteLine("\n!Do an intersection!\n");
            }
        }

        private void CountPolarFunction()
        {
            if (intersected)
            {
                if (dots.Length > 1)
                {
                    Vector3 a = dots[0].position;
                    Vector3 b = dots[1].position;

                    float angle = _countAngleToRotateYZ(a, b);

                    LineFunction[] rotated = new LineFunction[funcLine.Length];

                    funcPolar = new Line[rotated.Length];
                    for(int i = 0; i < rotated.Length; i++)
                    {
                        
                        rotated[i] = new LineFunction(dots[i].position, dots[i+1].position);
                        
                        rotated[i] = rotated[i].Rotate(angle);

                        //for (int j = 1; j < accuracyStrength; j++)
                        //{
                        //    angle = _countAngleToRotateYZ(rotated[i].start, rotated[i].end);
                        //    rotated[i] = rotated[i].Rotate(angle);
                        //}
                        funcPolar[i] = new Line(
                            new Vector3(0, rotated[i].start.Y, rotated[i].start.Z),
                            new Vector3(0, rotated[i].end.Y, rotated[i].end.Z),
                            color: ((Vector4)Color4.Purple).Xyz, 
                            width: 3
                            );
                    }
                    
                    // TODO: recheck it
                    //for (int i = 0; i < areaMesh.Length; i++)
                    //{
                    //    Vector3 start, end;
                    //    start = areaMesh[i].start;
                    //    end = areaMesh[i].end;
                    //    var line = new LineFunction(new float[] { start.X, end.X, start.Y, end.Y, start.Z, end.Z });
                    //    line.Rotate(angle);
                    //    areaMesh[i] = new Line(line.start, line.end, ((Vector4)Color4.Cyan).Xyz, width: 3);
                    //}

                    funcPolarArgs = new float[dots.Length][];
                    for(int i = 0; i < funcPolarArgs.Length; i++)
                    {
                        funcPolarArgs[i] = new float[2];

                        float x, y;
                        if (i < funcPolar.Length)
                        {
                            x = funcPolar[i].position.Y;
                            y = funcPolar[i].position.Z;
                        }
                        else
                        {
                            x = rotated[i - 1].end.Y;
                            y = rotated[i - 1].end.Z;
                        }
                        funcPolarArgs[i][0] = MathF.Sqrt(x * x + y * y);
                        funcPolarArgs[i][1] = MathF.Atan(y / x);
                    }
                    ShowPolarFuncs();
                    hasPolar = true;
                    showPolar = true;
                }
            }
        }

        // maybe later
        private void CountSphericalFunction()
        {

        }

        public void SwitchPlaneDisplaying()
        {
            isEnabled = !isEnabled;
        }

        public void Render(Shader shader, PrimitiveType? primitive = null)
        {
            if (intersected)
            {
                for (int i = 0; i < dots.Length && showAreaMesh; i++)
                {
                    dots[i].Render(shader, primitive);
                }
                for (int i = 0; i < funcLine.Length && showAreaMesh; i++)
                {
                    funcLine[i].Render(shader, primitive);
                }
                for (int i = 0; i < areaMesh.Length && showAreaMesh; i++)
                {
                    areaMesh[i].Render(shader, primitive);
                }
                if (hasPolar && showPolar)
                {
                    for(int i = 0; i < funcPolar.Length; i++)
                    {
                        funcPolar[i].Render(shader, primitive);
                    }
                }
            }

            if (isEnabled)
            {
                TextureLoader.UseMany(shader, textureHandlers);

                sqr.Render(shader);
            }

        }
    }
}
