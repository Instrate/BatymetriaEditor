using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class ComplexPlaneTile : Transformable
    {
        public Square[]? tiles;
        public bool showGeometry = false;

        public Vector3[] data = new Vector3[0];

        public float[] Xmesh;
        public float[] Ymesh;

        public float[][] DataStock;

        public float[][] DataBuffer;

        public int[] Range = new int[4];

        private Dot[] dots;
        public bool showDots = false;
                
        private int primitiveCurrent = 0;
        private float lineWidth;
        private PrimitiveType[] stylesSwitcher = new PrimitiveType[]
            {
                PrimitiveType.Triangles,
                PrimitiveType.Lines,
                PrimitiveType.LineStrip,
                PrimitiveType.Points
            };
        public PrimitiveType drawStyle;



        public ComplexPlaneTile(Vector3[]? inputData = null,
                                float[]? X = null,
                                float[]? Y = null,
                                float[][]? Z = null,
                                string[]? textureSet = null,
                                int[]? textureHandlersCopy = null, 
                                float lineWidth = 2f,
                                Material? material = null
                                )
        {
            if (inputData == null)
            {
                if (X == null || Y == null || Z == null)
                {
                    Functions.GenerateMesh(out X, out Y, out Z, start: -20, end: 20, divider: 50);
                }
                LoadData(X, Y, Z);
                this.lineWidth = lineWidth;
            }
            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for (int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                }
            }
            else
            {
                if (textureHandlersCopy != null)
                {
                    textureHandlers = textureHandlersCopy;
                }
            }
            if(material != null)
            {
                this.material = material;
            }

            isEnabled = true;
            showGeometry = true;
            drawStyle = stylesSwitcher[primitiveCurrent];
        }

        public void LoadData(float[] X, float[] Y, float[][] Data)
        {
            Xmesh = X;
            Ymesh = Y;
            DataStock = Data;
            DataBuffer = Functions.MatrixCopy(DataStock);

            RebuildRelief();
        }

        public void Interp(float scale = 1, float shift = 0)
        {
            alglib.spline2dinterpolant interp = Functions.Interpolate(Xmesh, Ymesh, DataBuffer);


            float dx = (Xmesh[1] - Xmesh[0]) * scale;
            float dy = (Ymesh[1] - Ymesh[0]) * scale;

            float[] X = Functions.Arrange(Xmesh[0] - shift, Xmesh[Xmesh.Length - 1] + shift, dx);
            float[] Y = Functions.Arrange(Ymesh[0] - shift, Ymesh[Ymesh.Length - 1] + shift, dy);

            Xmesh = X;
            Ymesh = Y;

            DataBuffer = Functions.recalculateBySpline(interp, X, Y);

            RebuildRelief();
        }

        public void MeshCompatibleRange()
        {
            int limit = 30;
            int xfactor = 0;
            int yfactor = 0;
            if (Xmesh.Length > limit)
            {
                xfactor = Xmesh.Length - limit;
            }
            if (Ymesh.Length > limit)
            {
                yfactor = Ymesh.Length - limit;
            }
            MoveVisibleMesh(0, 0, 0, 0, xfactor / 2, yfactor / 2);
        }

        public void MoveVisibleMesh(int x = 0, int y = 0, int scalex = 1, int scaley = 1, int shrinkx = 0, int shrinky = 0)
        {
            int rows = Xmesh.Length - 1;
            int cols = Ymesh.Length - 1;

            if (shrinkx != 0 && Range[1] + shrinkx * 2 < Range[3])
            {
                if (Range[1] + shrinkx >= 0)
                {
                    Range[1] += shrinkx;
                }
                if (Range[3] - shrinkx < rows)
                {
                    Range[3] -= shrinkx;
                }
            }
            if (shrinky != 0 && Range[0] + shrinky * 2 < Range[2])
            {
                if (Range[0] + shrinky >= 0)
                {
                    Range[0] += shrinky;
                }
                if (Range[2] - shrinky < cols)
                {
                    Range[2] -= shrinky;
                }
            }

            int temp = Range[1] + x;
            Range[1] = temp >= 0 && temp < Range[3] ? temp : Range[1];

            temp = Range[3] + x;
            Range[3] = temp > Range[1] && temp < rows ? temp : Range[3];

            temp = Range[0] + y;
            Range[0] = temp >= 0 && temp < Range[2] ? temp : Range[0];

            temp = Range[2] + y;
            Range[2] = temp > Range[0] && temp < cols ? temp : Range[2];

            int dx = Range[3] - Range[1];
            int dy = Range[2] - Range[0];

            int cx = Range[0] + dx / 2;
            int cy = Range[1] + dy / 2;

            Range[0] = Range[0] < 0 ? 0 : Range[0];
            Range[1] = Range[1] < 0 ? 0 : Range[1];
            Range[2] = Range[2] >= cols ? cols - 1 : Range[2];
            Range[3] = Range[3] >= rows ? rows - 1 : Range[3];
        }

        public void ResetMeshVisibility()
        {
            var rows = Xmesh.Length - 1;
            var cols = Ymesh.Length - 1;
            Range[0] = Range[1] = 0;
            Range[2] = rows - 1;
            Range[3] = cols - 1;
        }

        public void RebuildRelief()
        {
            var rows = Xmesh.Length - 1;
            var cols = Ymesh.Length - 1;
            tiles = new Square[rows * cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Vector2 top_left = new Vector2(Xmesh[i], Ymesh[j]);
                    Vector2 bottom_right = new Vector2(Xmesh[i + 1], Ymesh[j + 1]);
                    Vector4 heights = new Vector4(
                        DataBuffer[i + 1][j + 1],
                        DataBuffer[i][j + 1],
                        DataBuffer[i][j],
                        DataBuffer[i + 1][j]);
                    tiles[i * cols + j] = new Square(
                        builder: new Vector2[] { top_left, bottom_right },
                        heights: heights,
                        fixHeight: false,
                        parent: this
                        );
                }
            }

            dots = new Dot[Xmesh.Length * Ymesh.Length];

            for (int i = 0; i <= rows; i++)
            {
                for(int j = 0; j <= cols; j++)
                {
                    dots[i * (cols + 1) + j] = new Dot(pos: new Vector3(Xmesh[i], Ymesh[j], DataBuffer[i][j]), size: 3);
                }
            }

            _calcHeightRange();
            ResetMeshVisibility();
            MeshCompatibleRange();
        }

        public void SwitchDrawStyle()
        {
            primitiveCurrent++;
            if(stylesSwitcher.Length == primitiveCurrent)
            {
                primitiveCurrent = 0;
            }
            drawStyle = stylesSwitcher[primitiveCurrent];
        }

        private void _calcHeightRange()
        {
            float min = DataBuffer[0][0];
            float max = DataBuffer[0][0];
            for (int i = 0; i < DataBuffer.Length; i++)
            {
                for(int j = 0; j < DataBuffer[i].Length; j++)
                {
                    float v = DataBuffer[i][j];
                    if (v < min)
                    {
                        min = v;
                    }
                    else
                    {
                        if(v > max)
                        {
                            max = v;
                        }
                    }
                }
            }

            position = new Vector3(0, 0, min);
            _range = new Vector2(min, max);
        }

        public override void Move(Vector3 shifts)
        {
            position += shifts;
            for(int i = 0; i < Xmesh.Length; i++)
            {
                Xmesh[i] += shifts.X;
                for(int j = 0; j < Ymesh.Length; j++)
                {
                    DataBuffer[i][j] += shifts.Z;
                }
            }

            for (int i = 0; i < Ymesh.Length; i++)
            {
                Ymesh[i] += shifts.Y;
            }

            RebuildRelief();
        }

        private protected override void _renderObjects(Shader shader, PrimitiveType? primitive)
        {
            if (showDots)
            {
                for (int i = 0; i < dots.Length; i++)
                {
                    dots[i].Render(shader);
                }
            }

            if (tiles != null && showGeometry)
            {
                var rows = Xmesh.Length - 1;
                var cols = Ymesh.Length - 1;
                GL.LineWidth(lineWidth);
                GL.PointSize(lineWidth * 5);
                                
                for (int i = Range[0]; i <= Range[2]; i++)
                {
                    int ii = i * rows;
                    for (int j = Range[1]; j <= Range[3]; j++)
                    {
                        tiles[ii + j].Render(shader, drawStyle);
                    }
                }
            }
        }
    }
}
