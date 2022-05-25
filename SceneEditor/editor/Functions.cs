using OpenTK.Mathematics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public static class Functions
    {
        private static readonly Random rand = new System.Random();

        public static float CutFraction(float income, int amountToKeep = 0)
        {
            float temp = MathF.Pow(10, amountToKeep);
            return MathF.Truncate(income * temp) / temp;
        }

        public static float Random(int min = int.MinValue, int max = int.MaxValue, float amountOfAfterPoint = 0)
        {
            float num = rand.Next(min, max);
            if (amountOfAfterPoint > 0)
            {
                float fraction;
                float temp = MathF.Pow(10, amountOfAfterPoint + 1);
                fraction = rand.Next(0, (int)(temp - 1));
                fraction /= temp;
                if (num + fraction <= max)
                {
                    num += fraction;
                }
            }
            return num;
        }

        public static Vector3[] FixLowHight(Vector3[] data)
        {
            float min = data[0].Z;
            for (int i = 1; i < data.Length; i++)
            {
                if (min > data[i].Z)
                {
                    min = data[i].Z;
                }
            }
            for (int i = 0; i < data.Length; i++)
            {
                data[i].Z -= min;
            }
            return data;
        }

        public static void GenerateMesh(
            out float[] X,
            out float[] Y,
            out float[][] Z,
            float start = -20f,
            float end = 20f,
            float divider = 60
            )
        {
            float step = (end - start) / divider;
            X = Arrange(start, end, step);
            Y = Arrange(start, end, step);
            Z = FigureTest(X, Y);
        }

        public static void GenerateRandomPoints(
            out Vector3[] data,
            float start = -20f,
            float end = 20f,
            int amountLow = 40,
            int amountHigh = 60
            )
        {
            int amount = (int)Random(amountLow, amountHigh);
            float[] X = new float[amount];
            float[] Y = new float[amount];
            float[] Z = new float[amount];

            data = new Vector3[amount];

            for (int i = 0; i < amount; i++)
            {
                X[i] = Random((int)start, (int)end, 3);
                Y[i] = Random((int)start, (int)end, 3);
                Z[i] = FuncTestValue(X[i], Y[i]);
                data[i] = new Vector3(X[i], Y[i], Z[i]);
            }
        }

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
                    res[i][j] = FuncTestValue(x, y);
                }
            }
            //res = RaiseToZero(res);
            return res;
        }

        public static float FuncTestValue(float x, float y)
        {
            return (MathF.Cos(x / 3f) - MathF.Sin(y / 2f)) * (MathF.Cos(y / 3f) - MathF.Sin(x / 2f)) / 1.5f;
        }

        public static float[][] FunctWaterLine(float[] X, float[] Y, float height)
        {
            float[][] res = new float[X.Length][];
            for (int i = 0; i < X.Length; i++)
            {
                res[i] = new float[Y.Length];
                for (int j = 0; j < Y.Length; j++)
                {
                    float x = X[i];
                    float y = Y[j];
                    res[i][j] = height;
                }
            }
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

            alglib.spline2dinterpolant res = default;

            switch (type)
            {
                case 1: alglib.spline2dbuildbilinearv(x, m, y, n, f, 1, out res); break;
                default: alglib.spline2dbuildbicubicv(x, m, y, n, f, 1, out res); break;
            }

            return res;
        }

        [MTAThread]
        public static alglib.spline2dinterpolant Interpolate(Vector3[] points, int type = 0)
        {
            double[,] f = new double[points.Length,3];

            for(int i = 0; i < points.Length; i++)
            {
                f[i, 0] = points[i].X;
                f[i, 1] = points[i].Y;
                f[i, 2] = points[i].Z;
            }

            alglib.spline2dinterpolant res;
            alglib.spline2dfitreport rep;

            //alglib.spline2dbuildbicubicv(x, m, y, n, f, 1, out res);
            alglib.spline2dbuilder state;
            alglib.spline2dbuildercreate(1, out state);

            alglib.spline2dbuildersetpoints(state, f, points.Length);
            alglib.spline2dbuildersetareaauto(state);
            //alglib.spline2dbuildersetconstterm(state);
            alglib.spline2dbuildersetlinterm(state);
            //alglib.spline2dbuildersetgrid(state, points.Length * 5, points.Length * 5);
            alglib.spline2dbuildersetalgoblocklls(state, 0);
            //alglib.spline2dbuildersetalgofastddm(state, 0, 0);

            alglib.spline2dfit(state, out res, out rep);

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

        public static Vector3[] recalculateBySpline(alglib.spline2dinterpolant interp, Vector2[] data)
        {
            Vector3[] res = new Vector3[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                res[i] = new Vector3(
                        data[i].X,
                        data[i].Y,
                        (float)alglib.spline2dcalc(interp, data[i].X, data[i].Y)
                    );
            }
            return res;
        }

        public static float[][] MatrixFindMinor(float[][] arr, int ii, int jj)
        {
            float[][] matrix = new float[arr.Length - 1][];
            for (int i = 0; i < matrix.Length; i++)
            {
                matrix[i] = new float[arr[0].Length - 1];
            }
            int a;
            int b;
            a = b = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < arr.Length; j++)
                {
                    if (i != ii && j != jj)
                    {
                        matrix[a][b] = arr[i][j];
                        b++;
                        if (b == arr.Length - 1)
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
            for (int i = 0; i < arr.Length; i++)
            {
                minor_matrix[i] = new float[arr[0].Length];
                for (int j = 0; j < arr[0].Length; j++)
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
                if (minors == null)
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
                if (arr.Length == 2)
                {
                    return arr[0][0] * arr[1][1] - arr[0][1] * arr[1][0];
                }
                return Matrix3x3Determinant(arr);
            }
        }

        public static float[][] MatrixInversed(float[][] arr, float det)
        {
            float[][] matrix = new float[arr.Length][];
            for (int i = 0; i < arr.Length; i++)
            {
                matrix[i] = new float[arr.Length];
                for (int j = 0; j < arr.Length; j++)
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
            if (det == 0)
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
            catch (Exception ex)
            {
                return null;
            }
        }

        public static float[][] MatrixCopy(float[][] arr)
        {
            float[][] res = new float[arr.Length][];
            for (int i = 0; i < arr.Length; i++)
            {
                res[i] = new float[arr[i].Length];
                for (int j = 0; j < arr[i].Length; j++)
                {
                    res[i][j] = arr[i][j];
                }
            }
            return res;
        }
    }
}
