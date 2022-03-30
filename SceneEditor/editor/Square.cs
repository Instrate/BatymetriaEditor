using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using OpenTK.Graphics.OpenGL4;

using LearnOpenTK.Common;
using OpenTK.Mathematics;

namespace SceneEditor.editor
{
    public static class Tile
    {
        public static float[] color =
        {
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f
        };

        public static float[] normal =
        {
            0.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 1.0f
        };

        public static float[] coords = {
             0.5f,  0.5f, 0f,
             0.5f, -0.5f, 0f,
            -0.5f, -0.5f, 0f,
            -0.5f,  0.5f, 0f
        };

        public static float[] texture =
        {
            1.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f
        };

        public static uint[] indices = {
            0, 1, 3,
            1, 2, 3
        };
    }

    public class Square : IDrawable
    {
        private float[] vertices;

        public Vector3 position = Vector3.Zero;

        private Matrix4 transform = Matrix4.Identity;
        private Matrix4 rotation = Matrix4.Identity;
        private Matrix4 scale = Matrix4.Identity;
        private Matrix4 originShift = Matrix4.Identity;

        private int offset;

        private int VBO = -1;
        private int VAO = -1;
        private int EBO = -1;

        public int[] textureHandlers = null;

        private void concatSet(float[]? builder = default, Vector3? Color = default)
        {
            float[] coords = builder == default ? Tile.coords : builder;
            float[] color = Tile.color;
            if(Color != null)
            {
                for(int i = 0; i < color.Length; i++)
                {
                    color[i] = Color.Value[i % 3];
                }
            }

            int size = Tile.coords.Length + Tile.color.Length + Tile.texture.Length;
            offset = size / 4;

            vertices = new float[size];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < offset; j++)
                {
                    vertices[i * offset + j] = j < 3 ? coords[i * 3 + j] : j < 6 ? Tile.color[i * 3 + j - 3] : j < 8 ? Tile.texture[i * 2 + j - 6] : Tile.normal[i * 3 + j - 8];
                }
            }
        }

        private float[] _buildVertice(Vector2 v1, Vector2 v2, Vector4 v3)
        {
            float[] coords = new float[12];
            int off = 3;
            if(v3 == default)
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
            for(int i = 1; i < 4; i++)
            {
                if(min > heigths[i])
                {
                    min = heigths[i];
                }
            }
            for(int i = 0; i < 4; i++)
            {
                heigths[i] -= min;
            }
            position.Z = min;
            Move(position);
            return heigths;
        }

        public Square(string[]? textureSet = null, Vector2[]? builder = default, Vector4? heights = default, Vector3 pos = default, Vector3? color = default)
        {

            if(builder == default) 
            {
                concatSet();
            }
            else
            {
                // change the application not to vertices but to position
                
                concatSet(_buildVertice(builder[0], builder[1], heights == default ? Vector4.Zero : GetCorrectPosition(heights.Value)), color);
            }

            GL.GenBuffers(1, out VBO);
            GL.GenVertexArrays(1, out VAO);
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Tile.indices.Length * sizeof(uint), Tile.indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, offset * sizeof(float), IntPtr.Zero);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, offset * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, offset * sizeof(float), 6 * sizeof(float));

            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, offset * sizeof(float), 8 * sizeof(float));


            if (pos != default)
            {
                position = pos;
                Move(position);
            }

            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for(int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                } 
            }
        }

        public void Move(Vector3 shifts)
        {
            originShift = originShift * Matrix4.CreateTranslation(shifts);

            TransformCombiner();
        }

        public void Rotate(Vector3 angles)
        {
            Matrix4 X;
            Matrix4 Y;
            Matrix4 Z;
            Matrix4.CreateRotationX(angles.X, out X);
            Matrix4.CreateRotationY(angles.Y, out Y);
            Matrix4.CreateRotationZ(angles.Z, out Z);

            rotation = rotation * X * Y * Z;

            TransformCombiner();
        }

        public void Scale(Vector3 scalar)
        {
            scale = scale * Matrix4.CreateScale(scalar);
            TransformCombiner();
        }

        public void TransformCombiner()
        {
            transform = scale * rotation * originShift;
            
        }

        public void Render(int shaderHandle)
        {

            //texture loading
            if (textureHandlers != null && textureHandlers.Length > 0)
            {
                
                for (int i = 0; i < textureHandlers.Length && i < 32; i++)
                {
                    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
                }
            }

            //geometry
            var id = GL.GetUniformLocation(shaderHandle, "transform");
            GL.UniformMatrix4(id, false, ref transform);

            // drawing processed geometry
            GL.BindVertexArray(VAO);
            GL.DrawElements(BeginMode.Triangles, Tile.indices.Length, DrawElementsType.UnsignedInt, 0);

            // for safe drawing
            //GL.BindVertexArray(0);

        }

        public void TransformClean()
        {
            scale = originShift = rotation = Matrix4.Identity;
        }

        ~Square()
        {
            //if(VAO != -1)
            //    GL.DeleteVertexArray(VAO);
            //if (VBO != -1)
            //    GL.DeleteBuffer(VBO);
            //if (EBO != -1)
            //    GL.DeleteBuffer(EBO);
        }
    }
}
