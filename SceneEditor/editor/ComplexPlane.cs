using OpenTK.Graphics.OpenGL4;
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
            for (int i = 0; i < size; i++)
            {
                res[i] = min + i * step;
            }
            return res;
        }

        public static Vector3[] CreatePlane(Vector2 size = default)
        {
            if (size == default)
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
            for (int i = 0; i < X.Length; i++)
            {
                res[i] = new float[Y.Length];
                for (int j = 0; j < Y.Length; j++)
                {
                    float x = X[i];
                    float y = Y[j];
                    //res[i][j] = MathF.Sin(x);
                    //res[i][j] = x;
                    //res[i][j] = 1.3f * MathF.Atan((x + y) / 2) * MathF.Sin(MathF.Cos(x / 2)) * MathF.Cosh(MathF.Sin(y / 2));
                    res[i][j] = (MathF.Cos(x / 3f) - MathF.Sin(y / 2f)) * (MathF.Cos(y / 3f) - MathF.Sin(x / 2f)) / 1.5f;
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

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
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

            for (int i = 0; i < x.Length; i++)
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
    
        public static float[][] MatrixFindMinor(float[][] arr, int ii, int jj)
        {
            float[][] matrix = new float[arr.Length - 1][];
            for(int i = 0; i < matrix.Length; i++)
            {
                matrix[i] = new float[arr[0].Length-1];
            }
            int a;
            int b;
            a = b = 0;
            for(int i = 0; i < arr.Length; i++)
            {
                for(int j = 0; j < arr.Length; j++)
                {
                    if(i != ii && j != jj)
                    {
                        matrix[a][b] = arr[i][j];
                        b++;
                        if(b == arr.Length - 1)
                        {
                            b = 0;
                            a++;
                        }
                    }
                }
            }
            return matrix;
        }

        public static float[][] MatrixFindMinors(float[][] arr)
        {
            float[][] minor_matrix = new float[arr.Length][];
            for(int i = 0; i < arr.Length; i++)
            {
                minor_matrix[i] = new float[arr[0].Length];
                for(int j = 0; j < arr[0].Length; j++)
                {
                    float[][] Am = MatrixFindMinor(arr, i, j);
                    float det = MatrixDeterminant(Am);
                    minor_matrix[i][j] = det;
                }
            }
            return minor_matrix;
        }

        private static float Matrix3x3Determinant(float[][] arr)
        {
            float[] n = new float[6];
            n[0] = arr[0][0] * arr[1][1] * arr[2][2];
            n[1] = arr[0][1] * arr[1][2] * arr[2][0];
            n[2] = arr[0][2] * arr[1][0] * arr[2][1];
            n[3] = arr[0][2] * arr[1][1] * arr[2][0];
            n[4] = arr[0][0] * arr[1][2] * arr[2][1];
            n[5] = arr[0][1] * arr[1][0] * arr[2][2];
            float left = n[0] + n[1] + n[2];
            float right = n[3] + n[4] + n[5];
            return left - right;
        }

        public static float MatrixDeterminant(float[][] arr, float[][]? minors = null)
        {
            if (arr.Length > 3)
            {
                if(minors == null)
                {
                    minors = MatrixFindMinors(arr);
                }
                float det = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    int j = arr[0].Length - 1;
                    det += minors[i][j] * MathF.Pow((-1), (i + j)) * arr[i][j];
                }
                return det;
            }
            else
            {
                if(arr.Length == 2)
                {
                    return arr[0][0] * arr[1][1] - arr[0][1] * arr[1][0];
                }
                return Matrix3x3Determinant(arr);
            }
        }
    
        public static float[][] MatrixInversed(float[][] arr, float det)
        {
            float[][] matrix = new float[arr.Length][];
            for(int i = 0; i < arr.Length; i++)
            {
                matrix[i] = new float[arr.Length];
                for(int j = 0; j < arr.Length; j++)
                {
                    matrix[i][j] = arr[i][j] / det;
                }
            }
            return matrix;
        }
    
        public static float[]? MatrixSolution(float[][] A, float[] B)
        {
            float[] X = new float[B.Length];
            float det = MatrixDeterminant(A);
            if(det == 0)
            {
                return null;
            }
            try
            {
                for (int j = 0; j < B.Length; j++)
                {
                    float[][] A_t = MatrixCopy(A);
                    for (int i = 0; i < A.Length; i++)
                    {
                        A_t[i][j] = B[i];
                    }
                    float te = MatrixDeterminant(A_t, MatrixFindMinors(A_t));
                    X[j] = te / det;
                }
                return X;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public static float[][] MatrixCopy(float[][] arr)
        {
            float[][] res = new float[arr.Length][];
            for(int i = 0; i < arr.Length; i++)
            {
                res[i] = new float[arr[i].Length];
                for(int j = 0; j < arr[i].Length; j++)
                {
                    res[i][j] = arr[i][j];
                }
            }
            return res;
        }
    }

    internal class ComplexPlaneTile : IRenderable
    {
        public bool isLoaded = false;

        public Square[] tiles;

        public Vector3 position = Vector3.Zero;

        public Vector3[] data = new Vector3[0];

        public int[] textureHandlers;

        public float[] Xmesh;
        public float[] Ymesh;

        public float[][] DataStock;

        public float[][] DataBuffer;

        public int[] Range = new int[4];

        Dot[] dots;
        bool showDots = false;

        public PrimitiveType drawStyle = PrimitiveType.Triangles;
        float lineWidth;


        public ComplexPlaneTile(Vector3[] inputData = default,
                                float[]? X = null,
                                float[]? Y = null,
                                float[][]? Z = null,
                                string[]? textureSet = null,
                                float lineWidth = 2f)
        {
            if (inputData == default)
            {
                if (X == null && Y == null && Z == null)
                {
                    float start = -20f;
                    float end = 20f;
                    float step = (end - start) / 60;
                    X = Functions.Arrange(start, end, step);
                    Y = Functions.Arrange(start, end, step);
                    Z = Functions.FigureTest(X, Y);
                }
                LoadData(X, Y, Z);
                this.lineWidth = lineWidth;
            }
            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for (int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                }
            }

            isLoaded = true;
        }

        public void LoadData(float[] X, float[] Y, float[][] Data)
        {
            Xmesh = X;
            Ymesh = Y;
            DataStock = Data;
            DataBuffer = Functions.MatrixCopy(DataStock);

            ResetMeshVisibility();
            MeshCompatibleRange();
            RebuildRelief();
        }

        public void Interp(float scale = 1, float shift = 0)
        {
            alglib.spline2dinterpolant interp = Functions.Interpolate(Xmesh, Ymesh, DataBuffer);


            float dx = (Xmesh[1] - Xmesh[0]) * scale;
            float dy = (Ymesh[1] - Ymesh[0]) * scale;

            float[] X = Functions.Arrange(Xmesh[0] - shift, Xmesh[Xmesh.Length - 1] + shift, dx);
            float[] Y = Functions.Arrange(Ymesh[0] - shift, Ymesh[Ymesh.Length - 1] + shift, dy);

            Xmesh = X;
            Ymesh = Y;

            DataBuffer = Functions.recalculateBySpline(interp, X, Y);

            RebuildRelief();
        }

        public void MeshCompatibleRange()
        {
            // appropriate limit for 2Gb of VRAM
            int limit = 60;
            int xfactor = 0;
            int yfactor = 0;
            if (Xmesh.Length > limit)
            {
                xfactor = Xmesh.Length - limit;
            }
            if (Ymesh.Length > limit)
            {
                yfactor = Ymesh.Length - limit;
            }
            MoveVisibleMesh(0, 0, 0, 0, xfactor / 2, yfactor / 2);
        }

        public void MoveVisibleMesh(int x = 0, int y = 0, int scalex = 1, int scaley = 1, int shrinkx = 0, int shrinky = 0)
        {
            int rows = Xmesh.Length - 1;
            int cols = Ymesh.Length - 1;

            // shrink
            // x
            if (shrinkx != 0 && Range[1] + shrinkx * 2 < Range[3])
            {
                if (Range[1] + shrinkx >= 0)
                {
                    Range[1] += shrinkx;
                }
                if (Range[3] - shrinkx < cols)
                {
                    Range[3] -= shrinkx;
                }
            }
            // y
            if (shrinky != 0 && Range[0] + shrinky * 2 < Range[2])
            {
                if (Range[0] + shrinky >= 0)
                {
                    Range[0] += shrinky;
                }
                if (Range[2] - shrinky < rows)
                {
                    Range[2] -= shrinky;
                }
            }

            // shift
            // x
            int temp = Range[1] + x;
            Range[1] = temp >= 0 && temp < Range[3] ? temp : Range[1];

            temp = Range[3] + x;
            Range[3] = temp > Range[1] && temp < cols ? temp : Range[3];

            // y
            temp = Range[0] + y;
            Range[0] = temp >= 0 && temp < Range[2] ? temp : Range[0];

            temp = Range[2] + y;
            Range[2] = temp > Range[0] && temp < rows ? temp : Range[2];

            //scale

            int dx = Range[3] - Range[1];
            int dy = Range[2] - Range[0];

            int cx = Range[0] + dx / 2;
            int cy = Range[1] + dy / 2;

            //Range[0] = (int) (cx - dx * scalex * 0.5 - 1);
            //Range[1] = (int) (cy - dy * scaley * 0.5 - 1);
            //Range[2] = (int) (cx + dx * scalex * 0.5);
            //Range[3] = (int) (cy + dy * scaley * 0.5);

            Range[0] = Range[0] < 0 ? 0 : Range[0];
            Range[1] = Range[1] < 0 ? 0 : Range[1];
            Range[2] = Range[2] >= rows ? rows - 1 : Range[2];
            Range[3] = Range[3] >= cols ? cols - 1 : Range[3];
            //Console.WriteLine(Range);
        }

        public void ResetMeshVisibility()
        {
            var rows = Xmesh.Length - 1;
            var cols = Ymesh.Length - 1;
            Range[0] = Range[1] = 0;
            Range[2] = rows - 1;
            Range[3] = cols - 1;
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
                    Vector4 heights = new Vector4(DataBuffer[i + 1][j + 1], DataBuffer[i][j + 1], DataBuffer[i][j], DataBuffer[i + 1][j]);
                    tiles[i * cols + j] = new Square(builder: new Vector2[] { top_left, bottom_right }, heights: heights, fixHeight: false);
                }
            }

            dots = new Dot[Xmesh.Length * Ymesh.Length];
            for (int i = 0; i <= rows; i++)
            {
                for(int j = 0; j <= cols; j++)
                {
                    dots[i * (rows + 1) + j] = new Dot(pos: new Vector3(Xmesh[j], Ymesh[i], DataBuffer[j][i]));
                }
            }
            ResetMeshVisibility();
            MeshCompatibleRange();
        }
        
        public void SwitchDotsVisibility()
        {
            showDots = !showDots;
        }

        public void SwitchDrawStyle()
        {
            drawStyle = drawStyle == PrimitiveType.Triangles ? PrimitiveType.Lines : PrimitiveType.Triangles;
        }

        public void Render(int shader, PrimitiveType primitiveType = 0)
        {
            if (!isLoaded)
            {
                return;
            }

            //texture loading
            if (textureHandlers != null && textureHandlers.Length > 0)
            {
                for (int i = 0; i < textureHandlers.Length && i < 32; i++)
                {
                    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
                }
            }

            if (showDots)
            {
                for (int i = 0; i < dots.Length; i++)
                {
                    dots[i].Render(shader);
                }
            }

            //var rows = Xmesh.Length - 1;
            var cols = Ymesh.Length - 1;
            //var amount = tiles.Length;
            if(primitiveType != 0)
            {
                drawStyle = primitiveType;
            }
            GL.LineWidth(lineWidth);
            for (int i = Range[0]; i <= Range[2]; i++)
            {
                int ii = i * cols;
                for (int j = Range[1]; j <= Range[3]; j++)
                {
                    tiles[ii + j].Render(shader, drawStyle);
                }
            }
        }
    }
}
