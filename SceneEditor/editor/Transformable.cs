﻿using OpenTK.Graphics.OpenGL4;
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

        private protected Matrix4 rotation = Matrix4.Identity;
        private protected Matrix4 scale = Matrix4.Identity;

        public virtual new void Move(Vector3 shifts)
        {
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

        private protected virtual void _applyTextures()
        {
            if (textureHandlers != null && textureHandlers.Length > 0)
            {
                for (int i = 0; i < textureHandlers.Length && i < 32; i++)
                {
                    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
                }
            }
        }

        public virtual void Render(int shaderHandle, PrimitiveType primitive)
        {
            _applyTextures();
            var id = GL.GetUniformLocation(shaderHandle, "transform");
            GL.UniformMatrix4(id, false, ref transform);
            _renderObjects(shaderHandle);
        }

    }
}
