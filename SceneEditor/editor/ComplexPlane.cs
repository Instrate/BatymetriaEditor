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
    }

    internal class ComplexPlaneTile : IRenderable
    {
        public Square[] tiles;

        public Vector3 position = Vector3.Zero;

        public Vector3[] data = new Vector3[0];

        public int[] textureHandlers;

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
                    float step = 0.5f;
                    X = Functions.Arrange(start, end, step);
                    Y = Functions.Arrange(start, end, step);
                    Z = Functions.FigureTest(X, Y);
                }
                var rows = X.Length - 1;
                var cols = Y.Length - 1;
                tiles = new Square[rows * cols];
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Vector2 top_left = new Vector2(X[i], Y[j]);
                        Vector2 bottom_right = new Vector2(X[i + 1], Y[j + 1]);
                        Vector4 heights = new Vector4(Z[i + 1][j + 1], Z[i][j + 1], Z[i][j], Z[i + 1][j]);
                        tiles[i * cols + j] = new Square(builder: new Vector2[] { top_left, bottom_right }, heights: heights);
                    }
                }
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
