﻿using OpenTK.Graphics.OpenGL4;
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
        public static float[] coords =
        {
            -0.5f, -0.5f, 0.0f,
             -0.5f, 0.5f, 0.0f,
             0.5f,  -0.5f, 0.0f
        };

        public static float[] texture =
        {
            0, 0,
            0, 1,
            1, 0
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

        public static float[] vertices = Vertices();


        public static int offset = 11;

        public static float[] Vertices(float[]? customCoords = null)
        {
            float[] coords = customCoords == null ? TriangleCoords.coords : customCoords;

            int size = coords.Length + color.Length + texture.Length + normal.Length;
            float[] v = new float[size];

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

        float[] vertices;

        private int VBO = -1;
        private int VAO = -1;

        public int[]? textureHandlers = null;

        //implement color usage
        public Triangle(Vector3[] pointsV, Vector3? color = default, string[]? textureSet = null, bool keepHeight = true)
        {
            float min1 = pointsV[0].Z;
            float min2 = MathF.Min(pointsV[1].Z, pointsV[2].Z);
            min1 = MathF.Min(min1, pointsV[1].Z);

            if (keepHeight)
            {
                Move(new Vector3(0, 0, min1));
            }

            float[] points = new float[9];
            for(int i = 0; i < 3; i++)
            {
                points[3 * i] = pointsV[i].X;
                points[3 * i + 1] = pointsV[i].Y;
                points[3 * i + 2] = pointsV[i].Z - min1;
            }

            vertices = TriangleCoords.Vertices(points);

            BindObject();

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


            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TriangleCoords.offset * sizeof(float), IntPtr.Zero);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, TriangleCoords.offset * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, TriangleCoords.offset * sizeof(float), 6 * sizeof(float));

            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, TriangleCoords.offset * sizeof(float), 8 * sizeof(float));

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
        }

        public void TransformClean()
        {
            scale = originShift = rotation = Matrix4.Identity;
        }
    }
}
