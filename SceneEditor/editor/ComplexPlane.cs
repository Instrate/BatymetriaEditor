using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public static class Functions
    {
        public static float[] Arrange(float min, float max, float step)
        {
            int size = (int)((max - min) / step + 1);
            float[] res = new float[size];
            for(int i = 0; i < size; i++)
            {
                res[i] = min + i * step;
            }
            return res;
        }

        public static Vector3[] CreatePlane(Vector2 size = default)
        {
            if(size == default)
            {
                size = new Vector2(100, 100);
            }


            return default;
        }
        
        public static float[][] RaiseToZero(float[][] income)
        {
            float min = income[0][0];
            for (int i = 0; i < income.Length; i++)
            {
                for (int j = 0; j < income[i].Length; j++)
                {
                    if (min > income[i][j])
                    {
                        min = income[i][j];
                    }
                }
            }
            for (int i = 0; i < income.Length; i++)
            {
                for (int j = 0; j < income[i].Length; j++)
                {
                    income[i][j] -= min;
                }
            }
            return income;
        }

        public static float[][] FigureTest(float[] X, float[] Y)
        {
            float[][] res = new float[X.Length][];
            for(int i = 0; i < X.Length; i++)
            {
                res[i] = new float[Y.Length];
                for(int j = 0; j < Y.Length; j++)
                {
                    float x = X[i];
                    float y = Y[j];
                    //res[i][j] = MathF.Sin(x);
                    //res[i][j] = x;
                    res[i][j] = 1.3f * MathF.Atan((x + y) / 2) * MathF.Sin(MathF.Cos(x / 2)) * MathF.Cosh(MathF.Sin(y / 2));
                }
            }
            res = RaiseToZero(res);
            return res;
        }

        public static alglib.spline2dinterpolant Interpolate(float[] X, float[] Y, float[][] data, int type = 0)
        {
            int m = X.Length;
            int n = Y.Length;
            int d = m * n;

            double[] x = Array.ConvertAll(X, var => (double)var);
            double[] y = Array.ConvertAll(Y, var => (double)var);

            double[] f = new double[d];

            for(int i = 0; i < n; i++)
            {
                for(int j = 0; j < m; j++)
                {
                    f[i * n + j] = data[j][i];
                }
            }

            alglib.spline2dinterpolant res;


            //alglib.spline2dbuildbilinearv(x, m, y, n, f, 1, out res);
            alglib.spline2dbuildbicubicv(x, m, y, n, f, 1, out res);


            return res;
        }

        public static float[][] recalculateBySpline(alglib.spline2dinterpolant interp, float[] x, float[] y)
        {
            float[][] data = new float[x.Length][];
            
            for(int i = 0; i < x.Length; i++)
            {
                data[i] = new float[y.Length];
                for (int j = 0; j < y.Length; j++)
                {
                    double vx = x[i];
                    double vy = y[j];
                    data[i][j] = (float)alglib.spline2dcalc(interp, vx, vy);
                }
            }
            return data;
        }
    }

    internal class ComplexPlaneTile : IRenderable
    {
        public Square[] tiles;

        public Vector3 position = Vector3.Zero;

        public Vector3[] data = new Vector3[0];

        public int[] textureHandlers;

        public float[] Xmesh;
        public float[] Ymesh;

        public float[][] Data;

        public ComplexPlaneTile(Vector3[] inputData = default, float[]? X = null, float[]? Y = null, float[][]? Z = null, string[]? textureSet = null)
        {
            if(inputData == default)
            {
                if(X != null && Y != null && Z != null)
                {

                }
                else
                {
                    float start = -20f;
                    float end = 20f;
                    float step = (end - start) / 50;
                    X = Functions.Arrange(start, end, step);
                    Y = Functions.Arrange(start, end, step);
                    Z = Functions.FigureTest(X, Y);
                }

                Xmesh = X;
                Ymesh = Y;
                Data = Z;

                RebuildRelief();
            }
            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for (int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                }
            }
        }

        public void Interp(float scale = 1, float shift = 0)
        {
            alglib.spline2dinterpolant interp = Functions.Interpolate(Xmesh, Ymesh, Data);

            
            float dx = (Xmesh[1] - Xmesh[0]) * scale;
            float dy = (Ymesh[1] - Ymesh[0]) * scale;

            float[] X = Functions.Arrange(Xmesh[0] - shift, Xmesh[Xmesh.Length - 1] + shift, dx);
            float[] Y = Functions.Arrange(Ymesh[0] - shift, Ymesh[Ymesh.Length - 1] + shift, dy);

            Xmesh = X;
            Ymesh = Y;

            Data = Functions.recalculateBySpline(interp, Xmesh, Ymesh);

            RebuildRelief();
        }

        public void RebuildRelief()
        {
            var rows = Xmesh.Length - 1;
            var cols = Ymesh.Length - 1;
            tiles = new Square[rows * cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Vector2 top_left = new Vector2(Xmesh[i], Ymesh[j]);
                    Vector2 bottom_right = new Vector2(Xmesh[i + 1], Ymesh[j + 1]);
                    Vector4 heights = new Vector4(Data[i + 1][j + 1], Data[i][j + 1], Data[i][j], Data[i + 1][j]);
                    tiles[i * cols + j] = new Square(builder: new Vector2[] { top_left, bottom_right }, heights: heights);
                }
            }
        }

        public void Render(int shader)
        {
            //texture loading
            if (textureHandlers != null && textureHandlers.Length > 0)
            {
                for (int i = 0; i < textureHandlers.Length && i < 32; i++)
                {
                    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
                }
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].Render(shader);
            }
        }
    }
}
