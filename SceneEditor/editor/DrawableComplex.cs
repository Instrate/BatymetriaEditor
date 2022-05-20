using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class DrawableComplex
    {
        protected PrimitiveType[] stylesSwitcher = new PrimitiveType[]
            {
                PrimitiveType.Triangles,
                PrimitiveType.Lines,
                PrimitiveType.LineStrip,
                PrimitiveType.LinesAdjacency,
                PrimitiveType.Points
            };


    }
}
