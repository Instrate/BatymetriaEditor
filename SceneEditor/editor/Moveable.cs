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
    // add virtual event on object selected
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

        private protected virtual void _renderObjects(Shader shader, PrimitiveType? primitive)
        {
            return;
        }

        private protected virtual void _prepareRendering(Shader shader)
        {
            var id = GL.GetUniformLocation(shader.Handle, "transform");
            GL.UniformMatrix4(id, false, ref transform);
        }

        public virtual void Render(Shader shader, PrimitiveType? primitive = null)
        {
            if (isEnabled)
            {
                _prepareRendering(shader);
                _renderObjects(shader, primitive);
            }
        }


    }
}
