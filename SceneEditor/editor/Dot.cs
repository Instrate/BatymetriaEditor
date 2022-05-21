using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class Dot : Moveable
    {
        private float[]? vertices = null;

        private int VBO = -1;
        private int VAO = -1;

        public float size;

        public Dot(Vector3 pos, Vector3 color = default, float size = 3f)
        {
            vertices = new float[6]
            {
                0, 0, 0, color.X, color.Y, color.Z,
            };

            Move(pos);

            this.size = size;

            BindObject();
            isEnabled = true;
        }

        private void BindObject()
        {
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 3 * sizeof(float));
        }

        private protected override void _renderObjects(int shaderHandle, PrimitiveType? primitive)
        {
            GL.PointSize(size);

            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
        }
    }
}
