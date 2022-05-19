using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{

    public class ComplexPlaneTriangular : IRenderable
    {
        SquareTriangular[] tiles;

        public Vector3 position = Vector3.Zero;

        public Vector3[] data = new Vector3[0];

        public int[] textureHandlers;

        public ComplexPlaneTriangular(Vector3[]? inputData = null, float[]? X = null, float[]? Y = null, float[][]? Z = null, string[]? textureSet = null)
        {
            //if (inputData == default)
            //{
            //    if (X != null && Y != null && Z != null)
            //    {

            //    }
            //    else
            //    {
            //        float start = -1f;
            //        float end = 1f;
            //        float step = 1f;
            //        X = Functions.Arrange(start, end, step);
            //        Y = Functions.Arrange(start, end, step);
            //        Z = Functions.FigureTest(X, Y);
            //    }
            //    var rows = X.Length - 1;
            //    var cols = Y.Length - 1;
            //    tiles = new SquareTriangular[rows * cols];
            //    for (int i = 0; i < rows; i++)
            //    {
            //        for (int j = 0; j < cols; j++)
            //        {
            //            Vector2 top_left = new Vector2(X[i], Y[j]);
            //            Vector2 bottom_right = new Vector2(X[i + 1], Y[j + 1]);
            //            Vector4 heights = new Vector4(Z[i + 1][j + 1], Z[i][j + 1], Z[i][j], Z[i + 1][j]);
            //            tiles[i * cols + j] = new SquareTriangular(builder: new Vector2[] { top_left, bottom_right }, heights: heights);
            //        }
            //    }
            //}
            //if (textureSet != null)
            //{
            //    textureHandlers = new int[textureSet.Length];
            //    for (int i = 0; i < textureSet.Length; i++)
            //    {
            //        textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
            //    }
            //}
        }

        public void Render(int shader, PrimitiveType primitiveType = 0)
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
