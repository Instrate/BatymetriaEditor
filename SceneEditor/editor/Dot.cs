using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class Dot : IRenderable, IDrawable
    {
        private float[]? vertices = null;

        private int VBO = -1;
        private int VAO = -1;

        public float size;

        public Vector3 position = Vector3.Zero;

        private Matrix4 transform = Matrix4.Identity;
        private Matrix4 rotation = Matrix4.Identity;
        private Matrix4 scale = Matrix4.Identity;
        private Matrix4 originShift = Matrix4.Identity;

        public Dot(Vector3 pos, Vector3 color = default, float size = 3f)
        {
            vertices = new float[6]
            {
                0, 0, 0, color.X, color.Y, color.Z,
            };

            Move(pos);

            this.size = size;

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

        public void Render(int shaderHandle, PrimitiveType primitiveType = PrimitiveType.Points)
        {
            GL.PointSize(size);

            var id = GL.GetUniformLocation(shaderHandle, "transform");
            GL.UniformMatrix4(id, false, ref transform);

            GL.BindVertexArray(VAO);
            GL.DrawArrays(primitiveType, 0, 1);
        }

        public void Move(Vector3 shifts)
        {
            position += shifts;
            originShift = originShift * Matrix4.CreateTranslation(shifts);

            TransformCombiner();
        }

        public void Rotate(Vector3 angles)
        {

        }

        public void Scale(Vector3 scalar)
        {

        }

        public void TransformCombiner()
        {
            transform = originShift;
        }

        public void TransformClean()
        {
            originShift = Matrix4.Identity;
        }
    }
}
