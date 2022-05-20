using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;

namespace SceneEditor.editor
{
    public class Mesh : Moveable, IDrawable
    {
        public int size;
        public int length;

        private Line[] lines;

        float width;

        public Mesh(int size = 20, float step = 10.0f, float height = 0, float width = 0.5f)
        {
            length = size * 2;
            int amount = 2 * length + 2;

            this.width = width;

            lines = new Line[amount];

            float x = -size * step;
            float y = x;
            float z = height;

            float yy = size * step;
            for(int i = 0; i < length + 1; i++)
            {
                lines[i] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: x, y: yy, z: z));
                x += step;
            }
            x = y;
            float xx = size * step;
            for (int i = 0; i < length + 1; i++)
            {
                lines[length + 1 + i] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: xx, y: y, z: z));
                y += step;
            }

        }

        public void Render(int shaderHandle)
        {
            GL.LineWidth(width);

            int amount = lines.Length;
            for (int i = 0; i < amount; i++)
            {
                lines[i].Render(shaderHandle);
            }
        }
    }
}
