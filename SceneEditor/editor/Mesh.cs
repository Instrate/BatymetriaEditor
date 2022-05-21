using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;

namespace SceneEditor.editor
{
    public class Mesh : Moveable
    {
        public int size;
        public float step;

        private Line[] lines;

        public float width;

        public float height;

        public Mesh(int size = 20, float step = 10.0f, float height = 0, float width = 0.5f)
        {
            size = size % 2 == 1  ? size : size + 1;
            int sizeAwayFromCenter = size / 2;
            this.size = size;
            int amount = 2 * size;

            this.width = width;
            

            lines = new Line[amount];

            this.step = step;
            float x = -sizeAwayFromCenter * step;
            float y = x;
            float z = height;
            this.height = height;

            float yy = sizeAwayFromCenter * step;
            for(int i = 0; i < size; i++)
            {
                lines[i] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: x, y: yy, z: z));
                x += step;
            }
            x = y;
            float xx = sizeAwayFromCenter * step;
            for (int i = 0; i < size; i++)
            {
                lines[size + i] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: xx, y: y, z: z));
                y += step;
            }

            isEnabled = true;
        }

        private protected override void _renderObjects(int shaderHandle, PrimitiveType? primitive)
        {
            GL.LineWidth(width);

            int amount = lines.Length;
            for (int i = 0; i < amount; i++)
            {
                lines[i].Render(shaderHandle, primitive);
            }
        }
    }
}
