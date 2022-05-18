using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public interface IRenderable
    {
        void Render(int shaderHandle, PrimitiveType primitiveType = 0);
    }
}
