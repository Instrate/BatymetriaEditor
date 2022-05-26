using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using LearnOpenTK.Common;

namespace SceneEditor.editor
{
    public static class LineDefaultBuilder
    {
        public static float[] color = { 0f, 0f, 0f };
    }

    public class Line : Transformable
    {
        private float[] vertices;

        private int VBO = -1;
        private int VAO = -1;

        public float width;

        public Vector3 start;
        public Vector3 end;

        public Line(
            Vector3 start = default,
            Vector3 end = default,
            Vector3? color = null,
            float width = 1f
            )
        {
            float[] c = new float[] {0, 0, 0};
            if (color != null)
            {
                c = new float[3] { color.Value.X, color.Value.Y, color.Value.Z };
            }

            this.width = width;

            Move(start);

            this.start = start;
            this.end = end;
            Vector3 a = end - start;

            vertices = new float[12] 
            { 
                0, 0, 0, c[0], c[1], c[2],
                a.X, a.Y, a.Z, c[0], c[1], c[2]
            };

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
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        }

        //public override void Move(Vector3 shifts)
        //{
        //    position += shifts;
        //    originShift = originShift * Matrix4.CreateTranslation(shifts);

        //    TransformCombiner();
        //}

        private protected override void _renderObjects(Shader shader, PrimitiveType? primitive)
        {
            GL.LineWidth(width);
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
        }
    }
}
