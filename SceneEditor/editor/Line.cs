using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;

namespace SceneEditor.editor
{
    public static class LineDefaultBuilder
    {
        public static float[] color = { 0f, 0f, 0f };


    }

    public class Line : IRenderable, IDrawable
    {
        private float[] vertices;

        private int VBO = -1;
        private int VAO = -1;

        public Line(Vector3 start = default, Vector3 end = default)
        {
            vertices = new float[6] { start.X, start.Y, start.Z, end.X, end.Y, end.Z };

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            
        }

        public void Move(Vector3 shifts)
        {
            throw new NotImplementedException();
        }

        public void Render(int shaderHandle)
        {
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
        }

        public void Rotate(Vector3 angles)
        {
            throw new NotImplementedException();
        }

        public void Scale(Vector3 scalar)
        {
            throw new NotImplementedException();
        }

        public void TransformClean()
        {
            throw new NotImplementedException();
        }

        public void TransformCombiner()
        {
            throw new NotImplementedException();
        }
    }
}
