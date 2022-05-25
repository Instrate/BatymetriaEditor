using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
//using OpenTK.Graphics.OpenGL;
//using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class Transformable : Moveable
    {
        
        protected int[] textureHandlers;

        public new Vector3 position = Vector3.Zero;

        private protected new Matrix4 transform = Matrix4.Identity;
        private protected new Matrix4 originShift = Matrix4.Identity;
        private protected Matrix4 rotation = Matrix4.Identity;
        private protected Matrix4 scale = Matrix4.Identity;

        private protected Vector2 _range = new Vector2(0,0);
        private protected bool _rangeAssigned = false;

        private protected Transformable? parent = null;

        public virtual new void Move(Vector3 shifts)
        {
            position += shifts;
            originShift = originShift * Matrix4.CreateTranslation(shifts);
            TransformCombiner();
        }

        public virtual void Rotate(Vector3 angles)
        {
            Matrix4 X = Matrix4.CreateRotationX(angles.X);
            Matrix4 Y = Matrix4.CreateRotationY(angles.Y);
            Matrix4 Z = Matrix4.CreateRotationZ(angles.Z);
            rotation = rotation * X * Y * Z;
            TransformCombiner();
        }

        public virtual void Scale(Vector3 scalar)
        {
            scale = scale * Matrix4.CreateScale(scalar);
            TransformCombiner();
        }

        public virtual new void TransformCombiner()
        {
            transform = scale * rotation * originShift;
        }

        public virtual new void TransformClean()
        {
            transform = scale = originShift = rotation = Matrix4.Identity;
        }

        private protected void _applyTextures(Shader shader)
        {
            TextureLoader.UseMany(shader, textureHandlers);
        }

        private protected override void _prepareRendering(Shader shader)
        {
            var id = GL.GetUniformLocation(shader.Handle, "transform");
            GL.UniformMatrix4(id, false, ref transform);

            int attr = GL.GetUniformLocation(shader.Handle, "textureGradRange");
            if(parent == null)
            {
                GL.Uniform2(attr, ref _range);
            }
            else
            {
                GL.Uniform2(attr, parent._range);
            }
            
        }

        public override void Render(Shader shader, PrimitiveType? primitive = null)
        {
            if (isEnabled)
            {
                _applyTextures(shader);
                _prepareRendering(shader);
                _renderObjects(shader, primitive);
            }
        }
    }
}
