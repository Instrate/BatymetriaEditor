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
        public void Move(Vector3 shifts)
        {
            throw new NotImplementedException();
        }

        public void Rotate(Vector3 angles)
        {
            throw new NotImplementedException();
        }

        public void Scale(Vector3 scalar)
        {
            throw new NotImplementedException();
        }

        public void TransformClean()
        {
            throw new NotImplementedException();
        }

        public void TransformCombiner()
        {
            throw new NotImplementedException();
        }

        void IRenderable.Render(int shaderHandle)
        {
            
        }
    }
}
