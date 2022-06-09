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
    public class Mesh : Moveable
    {
        public int size;
        public float step;

        private Line[][] lines = new Line[3][];
        public bool[] showMesh = new bool[3];
        //private Line[] linesAdditional;

        //private bool _isRotated = false;

        public float width;

        public float height;

        public Mesh(
            int size = 1000,
            float step = 5.0f,
            float height = 0,
            float width = 0.5f
            )
        {
            size = (int)((float)size / step / 2);
            size = size % 2 == 1 ? size : size + 1;
            this.step = step;
            this.width = width;
            this.size = size;
            this.height = height;

            for(int i = 0; i < 3; i++)
            {
                lines[i] = _generateMesh(size, step, height, width, i);
                showMesh[i] = true;
            }

            isEnabled = true;
        }

        private Line[] _generateMesh(
            int size,
            float step,
            float height,
            float width,
            int rotate = 0)
        {
            int sizeAwayFromCenter = size / 2;
            int amount = 2 * size;
            Line[] lines = new Line[amount];

            if(rotate == 0)
            {
                float x = -sizeAwayFromCenter * step;
                float y = x;
                float z = height;

                float yy = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[i] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: x, y: yy, z: z), width: width);
                    x += step;
                }
                x = y;
                float xx = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[size + i] = new Line(new Vector3(x: x, y: y, z: z), new Vector3(x: xx, y: y, z: z), width: width);
                    y += step;
                }
            } else 
            if (rotate == 1){
                float x = -sizeAwayFromCenter * step;
                float y = x;
                float z = 0;

                float yy = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[i] = new Line(new Vector3(x: z, y: y, z: x + height), new Vector3(x: z, y: yy, z: x + height), width: width);
                    x += step;
                }
                x = y;
                float xx = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[size + i] = new Line(new Vector3(x: z, y: y, z: x + height), new Vector3(x: z, y: y, z: xx + height), width: width);
                    y += step;
                }
            }
            else
            {
                float x = -sizeAwayFromCenter * step;
                float y = x;
                float z = 0;

                float yy = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[i] = new Line(new Vector3(x: x, y: z, z: y + height), new Vector3(x: x, y: z, z: yy + height), width: width);
                    x += step;
                }
                x = y;
                float xx = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[size + i] = new Line(new Vector3(x: x, y: z, z: y + height), new Vector3(x: xx, y: z, z: y + height), width: width);
                    y += step;
                }
            }
            
            return lines;
        }

        private protected override void _renderObjects(Shader shader, PrimitiveType? primitive)
        {
            GL.LineWidth(width);
            for (int i = 0; i < lines.Length; i++)
            {
                for(int j = 0; j < lines[i].Length && showMesh[i]; j++)
                {
                    lines[i][j].Render(shader, primitive);
                }
            }
        }
    }
}
