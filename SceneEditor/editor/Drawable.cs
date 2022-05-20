using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class Drawable
    {
        protected int[] textureHandlers;



        public virtual void Render(int shaderHandle, PrimitiveType? primitive)
        {

        }
    }
}
