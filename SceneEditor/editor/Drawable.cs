using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public abstract class Drawable
    {
        private protected abstract void _renderObjects(int shaderHandle);

    }
}
