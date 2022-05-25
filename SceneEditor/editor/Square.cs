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
            0, 1, 2,
            0, 3, 2
        };

        public static int offset = 11;
    }

    public class Square : Transformable, IRenderable
    {
        private float[] vertices;

        private int offset;

        private int VBO = -1;
        private int VAO = -1;
        private int EBO = -1;

        private void concatSet(float[]? builder = default, Vector3? Color = default)
        {
            float[] coords = builder == default ? Tile.coords : builder;
            
            //Console.WriteLine(builder);

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

        private float[] _buildVertice(Vector2 v1, Vector2 v2, Vector4 v3, bool isSection = false)
        {
            float[] coords = new float[12];
            int off = 3;
            if(v3 == default)
            {
                v3 = Vector4.Zero;
            }

            // bootom left
            if (isSection)
            {
                coords[0] = v1.X; // x
                coords[1] = v1.Y; // y
            }
            else
            {
                coords[0] = v2.X; // x
                coords[1] = v2.Y; // y
            }
            coords[2] = v3[0]; // z

            // top left
            if (isSection)
            {
                coords[0 + off] = v1.X; // x
                coords[1 + off] = v1.Y; // y
            }
            else
            {
                coords[0 + off] = v1.X; // x
                coords[1 + off] = v2.Y; // y
            }
            coords[2 + off] = v3[1]; // z

            // top right
            if (isSection)
            {
                coords[0 + 2 * off] = v2.X; // x
                coords[1 + 2 * off] = v2.Y; // y
            }
            else
            {
                coords[0 + 2 * off] = v1.X; // x
                coords[1 + 2 * off] = v1.Y; // y
            }
            coords[2 + 2 * off] = v3[2]; // z

            // bootom right
            if (isSection)
            {
                coords[0 + 3 * off] = v2.X; // x
                coords[1 + 3 * off] = v2.Y; // y
            }
            else
            {
                coords[0 + 3 * off] = v2.X; // x
                coords[1 + 3 * off] = v1.Y; // y
            }
            coords[2 + 3 * off] = v3[3]; // z

            return coords;
        }

        private Vector4 GetCorrectPosition(Vector4 heigths)
        {
            //Console.WriteLine(heigths + "\n");
            
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

        public Square(string[]? textureSet = null,
                      Vector2[]? builder = null,
                      Vector4? heights = default,
                      Vector3? pos = null,
                      Vector3? color = default,
                      bool fixHeight = false,
                      bool isSection = false,
                      Transformable? parent = null
                      )
        {
            this.parent = parent;

            if(builder == null) 
            {
                concatSet();
            }
            else
            {
                // change the application not to vertices but to position if possible
                concatSet(_buildVertice(builder[0], builder[1], heights == default ? Vector4.Zero : fixHeight == true ? GetCorrectPosition(heights.Value) : heights.Value, isSection), color);
            }

            BindObject();
            isEnabled = true;

            if (pos != null)
            {
                position = pos.Value;
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

        private void BindObject()
        {
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
        }

        private protected override void _renderObjects(Shader shader, PrimitiveType? primitive)
        {
            GL.BindVertexArray(VAO);
            GL.DrawElements(
                primitive.HasValue ? primitive.Value : PrimitiveType.Triangles,
                Tile.indices.Length,
                DrawElementsType.UnsignedInt, 0);
        }
    }
}
