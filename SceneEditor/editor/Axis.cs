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
    public class Axis : Transformable
    {
        private Cube[] axis = new Cube[3];

        public Axis(string[]? textureSet = null)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 color = Vector3.Zero;
                color[i] = 1;
                axis[i] = new Cube(color: color);
            }

            float scaling = 0.05f;
            float sizeScale = 4f;

            axis[0].Scale(new Vector3(sizeScale, scaling, scaling));
            axis[1].Scale(new Vector3(scaling, sizeScale, scaling));
            axis[2].Scale(new Vector3(scaling, scaling, sizeScale));

            axis[0].Move(new Vector3() { X = 0.5f * sizeScale - scaling / 2 });
            axis[1].Move(new Vector3() { Y = 0.5f * sizeScale - scaling / 2 });
            axis[2].Move(new Vector3() { Z = 0.5f * sizeScale - scaling / 2 });

            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for (int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                }
            }

            isEnabled = true;
        }

        private protected override void _renderObjects(Shader shader, PrimitiveType? primitive)
        {
            foreach (var c in axis)
            {
                c.Render(shader, primitive);
            }
        }
    }
}
