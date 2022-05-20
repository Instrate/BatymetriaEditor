using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    static class WallsCubic
    {
        public static float[] coords = {
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,

            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,

             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,

            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f
        };

        public static float[] color = {
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f
        };

        public static float[] texture = {
            0.0f, 0.0f,
            1.0f, 0.0f,
            1.0f, 1.0f,
            1.0f, 1.0f,
            0.0f, 1.0f,
            0.0f, 0.0f,

            0.0f, 0.0f,
            1.0f, 0.0f,
            1.0f, 1.0f,
            1.0f, 1.0f,
            0.0f, 1.0f,
            0.0f, 0.0f,

            1.0f, 0.0f,
            1.0f, 1.0f,
            0.0f, 1.0f,
            0.0f, 1.0f,
            0.0f, 0.0f,
            1.0f, 0.0f,

            1.0f, 0.0f,
            1.0f, 1.0f,
            0.0f, 1.0f,
            0.0f, 1.0f,
            0.0f, 0.0f,
            1.0f, 0.0f,

            0.0f, 1.0f,
            1.0f, 1.0f,
            1.0f, 0.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f,

            0.0f, 1.0f,
            1.0f, 1.0f,
            1.0f, 0.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f
        };

        public static float[] normal = {
            0.0f,  0.0f, -1.0f,
            0.0f,  0.0f, -1.0f,
            0.0f,  0.0f, -1.0f,
            0.0f,  0.0f, -1.0f,
            0.0f,  0.0f, -1.0f,
            0.0f,  0.0f, -1.0f,

            0.0f,  0.0f,  1.0f,
            0.0f,  0.0f,  1.0f,
            0.0f,  0.0f,  1.0f,
            0.0f,  0.0f,  1.0f,
            0.0f,  0.0f,  1.0f,
            0.0f,  0.0f,  1.0f,

            -1.0f,  0.0f,  0.0f,
            -1.0f,  0.0f,  0.0f,
            -1.0f,  0.0f,  0.0f,
            -1.0f,  0.0f,  0.0f,
            -1.0f,  0.0f,  0.0f,
            -1.0f,  0.0f,  0.0f,

             1.0f,  0.0f,  0.0f,
             1.0f,  0.0f,  0.0f,
             1.0f,  0.0f,  0.0f,
             1.0f,  0.0f,  0.0f,
             1.0f,  0.0f,  0.0f,
             1.0f,  0.0f,  0.0f,

             0.0f, -1.0f,  0.0f,
             0.0f, -1.0f,  0.0f,
             0.0f, -1.0f,  0.0f,
             0.0f, -1.0f,  0.0f,
             0.0f, -1.0f,  0.0f,
             0.0f, -1.0f,  0.0f,

             0.0f,  1.0f,  0.0f,
             0.0f,  1.0f,  0.0f,
             0.0f,  1.0f,  0.0f,
             0.0f,  1.0f,  0.0f,
             0.0f,  1.0f,  0.0f,
             0.0f,  1.0f,  0.0f
        };
    }

    internal class Cube : Transformable, IDrawable
    {
        private float[] vertices;

        private int offset;

        private int VBO = -1;
        private int VAO = -1;


        private void concatSet(float[]? builder = default, Vector3? Color = default)
        {
            float[] coords = builder == default ? WallsCubic.coords : builder;

            float[] color = WallsCubic.color;
            if (Color != null)
            {
                for (int i = 0; i < color.Length; i++)
                {
                    color[i] = Color.Value[i % 3];
                }
            }

            int size = WallsCubic.coords.Length + WallsCubic.color.Length * 6 + WallsCubic.normal.Length + WallsCubic.texture.Length;
            offset = size / 36;

            vertices = new float[size];
            for (int i = 0; i < 36; i++)
            {
                for (int j = 0; j < offset; j++)
                {
                    //Console.WriteLine("[" + i + "][" + j + "]");
                    vertices[i * offset + j] = j < 3 ? coords[i * 3 + j] : j < 6 ? color[i % 6 * 3 + j - 3] : j < 8 ? WallsCubic.texture[i * 2 + j - 6] : WallsCubic.normal[i * 3 + j - 8];
                }
            }
           
        }

        private float[] _buildVertice(Vector2 v1, Vector2 v2, Vector4 v3)
        {
            float[] coords = new float[12];
            int off = 3;
            if (v3 == default)
            {
                v3 = Vector4.Zero;
            }

            coords[0] = v2.X; // x
            coords[1] = v2.Y; // y
            coords[2] = v3[0]; // z

            coords[0 + off] = v1.X; // x
            coords[1 + off] = v2.Y; // y
            coords[2 + off] = v3[1]; // z

            coords[0 + 2 * off] = v1.X; // x
            coords[1 + 2 * off] = v1.Y; // y
            coords[2 + 2 * off] = v3[2]; // z

            coords[0 + 3 * off] = v2.X; // x
            coords[1 + 3 * off] = v1.Y; // y
            coords[2 + 3 * off] = v3[3]; // z

            return coords;
        }

        private Vector4 GetCorrectPosition(Vector4 heigths)
        {
            float min = heigths[0];
            for (int i = 1; i < 4; i++)
            {
                if (min > heigths[i])
                {
                    min = heigths[i];
                }
            }
            for (int i = 0; i < 4; i++)
            {
                heigths[i] -= min;
            }
            position.Z = min;
            Move(position);
            return heigths;
        }

        public Cube(string[]? textureSet = null, Vector3 pos = default, Vector3? color = default)
        {
            concatSet(Color: color);

            BindObject();

            if (pos != default)
            {
                position = pos;
                Move(position);
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

        private void BindObject()
        {
            GL.GenBuffers(1, out VBO);
            GL.GenVertexArrays(1, out VAO);

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // coord
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, offset * sizeof(float), IntPtr.Zero);
            GL.EnableVertexAttribArray(0);

            // color
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, offset * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // texture
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, offset * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            // normal
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, offset * sizeof(float), 8 * sizeof(float));
            GL.EnableVertexAttribArray(3);
        }

        public override void Move(Vector3 shifts)
        {
            position += shifts;
            originShift = originShift * Matrix4.CreateTranslation(shifts);

            TransformCombiner();
        }

        public void Render(int shaderHandle)
        {
            //texture loading
            _applyTextures();

            //geometry and color changes
            var id = GL.GetUniformLocation(shaderHandle, "transform");
            GL.UniformMatrix4(id, false, ref transform);

            // drawing processed geometry
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // for safe drawing
            //GL.BindVertexArray(0);
        }
    }
}
