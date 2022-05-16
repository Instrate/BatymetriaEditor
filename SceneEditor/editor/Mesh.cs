using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;

namespace SceneEditor.editor
{
    public class Mesh : IRenderable, IDrawable
    {
        public int size;
        public int length;

        private Line[] lines;

        public Mesh(int size = 18, float step = 2.0f)
        {
            this.size = size;
            length = size * 2;
            int amount = 2 * length + 2;


            lines = new Line[amount];

            float x = -size * step;
            float y = x;
            float z = 0f;

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


            //for (int i = 0; i < length; i++)
            //{
            //    for (int j = 0; j < length; j++)
            //    {
            //        lines[i * length * 2 + j * 2] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: x, y: y + step, z: z));
            //        lines[i * length * 2 + j * 2 + 1] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: x + step, y: y, z: z));
            //        y += step;
            //    }
            //    x += step;
            //    y -= length * step;
            //}
            //lines[amount * 2] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: x, y: y + length * step, z: z));
            //lines[amount * 2 + 1] = new Line(new Vector3(x: -size * step, y: y + length * step, z: z), new Vector3(x: x, y: y + length * step, z: z));\

        }

        public void Move(Vector3 shifts)
        {

        }

        public void Rotate(Vector3 angles)
        {

        }

        public void Scale(Vector3 scalar)
        {

        }

        public void TransformClean()
        {

        }

        public void TransformCombiner()
        {

        }

        void IRenderable.Render(int shaderHandle)
        {
            int amount = lines.Length;
            for (int i = 0; i < amount; i++)
            {
                lines[i].Render(shaderHandle);
            }

        }
    }
}
