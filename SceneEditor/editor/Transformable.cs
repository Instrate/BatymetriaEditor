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
    public class Material
    {
        public Vector3 ambient = new Vector3(1);
        public Vector3 diffuse = new Vector3(1f);
        //public Vector3 specular = new Vector3(1);
        //public float shininess = 32;
        public float opacity = 1;

        public Material(
            Vector3? ambient = null,
            Vector3? diffuse = null,
            //Vector3? specular = null,
            //float? shininess = null,
            float? opacity = null
            )
        {
            if (ambient != null)
                this.ambient = ambient.Value;
            if(diffuse != null)
                this.diffuse = diffuse.Value;
            //if(specular != null)
                //this.specular = specular.Value;
            //if (shininess != null)
                //this.shininess = shininess.Value;
            if(opacity != null)
                this.opacity = opacity.Value;
        }
    }


    public class Transformable : Moveable
    {
        protected int[] textureHandlers;

        private protected Matrix4 rotation = Matrix4.Identity;
        private protected Matrix4 scale = Matrix4.Identity;

        private protected Vector2 _range = new Vector2(0,0);

        private protected Transformable? parent = null;
        private protected Material material = new();

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

        private protected void _applyMaterial(Shader shader)
        {
            Material mat = (parent != null) ? parent.material : new();

            shader.SetVector3("material.ambient", mat.ambient);
            shader.SetVector3("material.diffuse", mat.diffuse);
            //shader.SetVector3("material.specular", mat.specular);
            //shader.SetFloat("material.shininess", mat.shininess);
            shader.SetFloat("material.opacity", mat.opacity);
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
                _applyMaterial(shader);
                _renderObjects(shader, primitive);
            }
        }
    }
}
