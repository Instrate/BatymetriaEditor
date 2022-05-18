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

    public class LineFunction {
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

        public Vector3? Intersect(float A, float B, float C, float D)
        {
            
            float[][] arr = new float[3][];
            float[] right = new float[3];
            for(int i = 0; i < 3; i++)
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
            if(X != null)
            {
                return new Vector3(X[0], X[1], X[2]);
            }
            return null;
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
        public int[]? textureHandlers = null;

        Square sqr;

        Vector3[] chars = new Vector3[2];

        PlaneFunction plane;

        Dot[] dots;

        Line[] lines;
        bool intersected = false;

        // TODO: fix the rotation bug
        public Section(Vector3 start, Vector3 end, string[]? textureSet = null, Vector3? color = default)
        {
            chars[0] = start;
            chars[1] = end;

            plane = new PlaneFunction(start, end);
            //plane.ShowFunction();
            

            Vector2 v1 = new Vector2(start.X, start.Y);
            Vector2 v2 = new Vector2(end.X, end.Y);

            Vector4 heights = new Vector4(end.Z, start.Z, start.Z, end.Z);

            sqr = new Square(builder: new Vector2[] { v1, v2 }, heights: heights, isSection: true);

            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for (int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                }
            }
        }

        public Vector3[]? IntersectTiled(float[] X, float[] Y, float[][] Data)
        {
            

            int cols = X.Length - 1;
            int rows = Y.Length - 1;

            float dx = X[1] - X[0];
            float dy = Y[1] - Y[0];

            // hard stuff appears
            float xmax = MathF.Max(chars[0].X, chars[1].X);
            xmax = X[cols] < xmax ? X[cols] : xmax;

            float xmin = MathF.Min(chars[0].X, chars[1].X);
            xmin = X[0] > xmin ? X[0] : xmin;

            float ymax = MathF.Max(chars[0].Y, chars[1].Y);
            ymax = Y[rows] < ymax ? Y[rows] : ymax;

            float ymin = MathF.Min(chars[0].Y, chars[1].Y);
            ymin = Y[0] > ymin ? Y[0] : ymin;

            if(xmax < xmin)
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
            for (int i = 0; i < x.Length; i++) {
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
                    lines[i * y.Length + j] = new LineFunction(new float[] {x[i], x[i+1], y[j], y[j], data[i][j], data[i+1][j]});
                }
            }

            for (int i = 0; i < x.Length; i++)
            {
                for (int j = 0; j < y.Length - 1; j++)
                {
                    lines[(x.Length - 1) * y.Length + i * (y.Length - 1) + j] = new LineFunction(new float[] { x[i], x[i], y[j], y[j + 1], data[i][j], data[i][j + 1] });
                }
            }

            this.lines = new Line[amount];

            for (int i = 0; i < amount; i++) 
            {

                this.lines[i] = new Line(lines[i].start, lines[i].end, ((Vector4)Color4.Orange).Xyz);
            }


            // convert result
            List<Vector3> res = new List<Vector3>();
            for(int i = 0; i < amount; i++)
            {
                Vector3? pos = lines[i].Intersect(plane.A, plane.B, plane.C, plane.D);
                if(pos != null)
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
                Vector3[] intersects = res.ToArray();
                for (int i = 0; i < amount; i++)
                {
                    dots[i] = new Dot(intersects[i], color: ((Vector4)Color4.Red).Xyz ,size: 5);
                }
                return intersects;
            }
            return null;
        }


        public void Render(int shaderHandle, PrimitiveType primitiveType = 0)
        {
            if (intersected)
            {
                for(int i = 0; i < dots.Length; i++)
                {
                    dots[i].Render(shaderHandle);
                }
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i].Render(shaderHandle);
                }
            }

            if (textureHandlers != null && textureHandlers.Length > 0)
            {
                for (int i = 0; i < textureHandlers.Length && i < 32; i++)
                {
                    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
                }
            }

            sqr.Render(shaderHandle);
        }
    }
}
