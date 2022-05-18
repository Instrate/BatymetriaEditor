using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public static class TriangleCoords
    {
        public static float[] coords1 =
        {
            -0.5f, -0.5f, 0.0f,
             -0.5f, 0.5f, 0.0f,
             0.5f,  -0.5f, 0.0f
        };

        public static float[] coords2 =
        {
            -0.5f, 0.5f, 0.0f,
             0.5f, 0.5f, 0.0f,
             0.5f,  -0.5f, 0.0f
        };

        public static float[] texture1 =
        {
            0, 0,
            0, 1,
            1, 0
        };

        public static float[] texture2 =
        {
            1, 1,
            1, 0,
            0, 1
        };

        public static float[] color =
        {
            1f, 1f, 1f,
            1f, 1f, 1f,
            1f, 1f, 1f
        };

        public static float[] normal =
        {
            0f, 0f, 1f,
            0f, 0f, 1f,
            0f, 0f, 1f
        };

        public static float[] vertices1 = Vertices();

        public static float[] vertices2 = Vertices(true);

        public static int offset = 11;

        private static float[] Vertices(bool use_texture1 = false)
        {
            int size = coords1.Length + color.Length + texture1.Length + normal.Length;
            float[] v = new float[size];

            float[] texture = use_texture1 ? texture1 : texture2;
            float[] coords = use_texture1 ? coords1 : coords2;

            int offset = size / 3;

            for(int i = 0; i < size / offset; i++)
            {
                for(int j = 0; j < offset; j++)
                {
                    v[i * offset + j] = j < 3 ? coords[i * 3 + j] : j < 6 ? color[i * 3 + j - 3] : j < 8 ? texture[i * 2 + j - 6] : normal[i * 3 + j - 8];
                }
            }

            return v;
        }
    }


    public class Triangle : IDrawable
    {
        public Vector3 position = Vector3.Zero;

        private Matrix4 transform = Matrix4.Identity;
        private Matrix4 rotation = Matrix4.Identity;
        private Matrix4 scale = Matrix4.Identity;
        private Matrix4 originShift = Matrix4.Identity;


        private int VBO = -1;
        private int VAO = -1;

        public int[] textureHandlers = null;

        public Triangle(string[]? textureSet = null, bool secondTriangle = false, Vector2[]? builder = default, Vector4? heights = default, Vector3 pos = default, Vector3? color = default)
        {
            GL.GenBuffers(1, out VBO);
            GL.GenVertexArrays(1, out VAO);

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            if (!secondTriangle)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, TriangleCoords.vertices1.Length * sizeof(float), TriangleCoords.vertices1, BufferUsageHint.StaticDraw);
            }
            else
            {
                GL.BufferData(BufferTarget.ArrayBuffer, TriangleCoords.vertices2.Length * sizeof(float), TriangleCoords.vertices2, BufferUsageHint.StaticDraw);
            }
            

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TriangleCoords.offset * sizeof(float), IntPtr.Zero);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, TriangleCoords.offset * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, TriangleCoords.offset * sizeof(float), 6 * sizeof(float));

            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, TriangleCoords.offset * sizeof(float), 8 * sizeof(float));


            if (pos != default)
            {
                position = pos;

                //Questionable
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

        public void Move(Vector3 shifts)
        {
            originShift = originShift * Matrix4.CreateTranslation(shifts);

            TransformCombiner();
        }

        public void Rotate(Vector3 angles)
        {
            Matrix4 X = Matrix4.CreateRotationX(angles.X);
            Matrix4 Y = Matrix4.CreateRotationY(angles.Y);
            Matrix4 Z = Matrix4.CreateRotationZ(angles.Z);
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
            transform = scale * originShift * rotation;

        }

        public void Render(int shaderHandle, PrimitiveType primitiveType = PrimitiveType.Triangles)
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
            GL.DrawArrays(primitiveType, 0, 3);

            // for safe drawing
            //GL.BindVertexArray(0);

        }

        public void TransformClean()
        {
            scale = originShift = rotation = Matrix4.Identity;
        }
    }
}
