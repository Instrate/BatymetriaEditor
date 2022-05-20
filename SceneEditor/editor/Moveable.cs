using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class Moveable : IRenderable
    {
        public bool isEnabled = false;

        public Vector3 position = Vector3.Zero;

        private protected Matrix4 transform = Matrix4.Identity;
        private protected Matrix4 originShift = Matrix4.Identity;

        public virtual void Move(Vector3 shifts)
        {
            position += shifts;
            originShift = originShift * Matrix4.CreateTranslation(shifts);

            TransformCombiner();
        }

        public virtual void TransformCombiner()
        {
            transform *= originShift;
        }

        public virtual void TransformClean()
        {
            transform = originShift = Matrix4.Identity;
        }

        private protected virtual void _renderObjects(int shaderHandle, PrimitiveType? primitive)
        {
            return;
        }

        private protected virtual void _prepareRendering(int shaderHandle)
        {
            var id = GL.GetUniformLocation(shaderHandle, "transform");
            GL.UniformMatrix4(id, false, ref transform);
        }

        public virtual void Render(int shaderHandle, PrimitiveType? primitive = null)
        {
            if (isEnabled)
            {
                _prepareRendering(shaderHandle);
                _renderObjects(shaderHandle, primitive);
            }
        }


    }
}
