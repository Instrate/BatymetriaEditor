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

        private Line[][] lines = new Line[3][];
        public bool[] showMesh = new bool[3];
        //private Line[] linesAdditional;

        private bool _isRotated = false;

        public float width;

        public float height;

        public Mesh(int size = 20, float step = 10.0f, float height = 0, float width = 0.5f)
        {
            size = size % 2 == 1  ? size : size + 1;
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

        private Line[] _generateMesh(int size, float step, float height, float width, int rotate = 0)
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
            } else 
            if (rotate == 1){
                float x = -sizeAwayFromCenter * step;
                float y = x;
                float z = height;

                float yy = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[i] = new Line(new Vector3(x: z, y: y, z: x), new Vector3(x: z, y: yy, z: x));
                    x += step;
                }
                x = y;
                float xx = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[size + i] = new Line(new Vector3(x: z, y: y, z: x), new Vector3(x: z, y: y, z: xx));
                    y += step;
                }
            }
            else
            {
                float x = -sizeAwayFromCenter * step;
                float y = x;
                float z = height;

                float yy = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[i] = new Line(new Vector3(x: x, y: z, z: y), new Vector3(x: x, y: z, z: yy));
                    x += step;
                }
                x = y;
                float xx = sizeAwayFromCenter * step;
                for (int i = 0; i < size; i++)
                {
                    lines[size + i] = new Line(new Vector3(x: x, y: z, z: y), new Vector3(x: xx, y: z, z: y));
                    y += step;
                }
            }
            
            return lines;
        }

        private protected override void _renderObjects(int shaderHandle, PrimitiveType? primitive)
        {
            GL.LineWidth(width);
            for (int i = 0; i < lines.Length; i++)
            {
                for(int j = 0; j < lines[i].Length && showMesh[i]; j++)
                {
                    lines[i][j].Render(shaderHandle, primitive);
                }
            }
        }
    }
}
