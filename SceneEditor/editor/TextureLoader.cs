using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

using OpenTK.Mathematics;
using LearnOpenTK.Common;

namespace SceneEditor.editor
{
    public class TextureLoader
    {
        public static List<TextureUnit> units_all = Enum.GetValues(typeof(TextureUnit)).Cast<TextureUnit>().ToList();

        public static int LoadFromFile(string path, bool hasBorder = false, int borderWidth = 0,
                                        Color4 borderColor = default,
                                        TextureWrapMode style = TextureWrapMode.Repeat, 
                                        TextureMinFilter LODbiasMIN = TextureMinFilter.LinearMipmapLinear,
                                        TextureMagFilter LODbiasMAG = TextureMagFilter.Linear)
        {
            int handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, handle);
            using (var image = new Bitmap(path))
            {

                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)LODbiasMIN);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)LODbiasMAG);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)style);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)style);

            if (hasBorder && style == TextureWrapMode.ClampToBorder)
            {
                float[] color = new float[4]; 
                ((Vector4)borderColor).Deconstruct(out color[0], out color[1], out color[2], out color[3]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, color);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return handle;
        }

        public static void Use(TextureUnit unit, int handle)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, handle);
           
        }

        public static void UseMany(Shader shader, int[]? textureHandlers)
        {
            if(textureHandlers == null)
            {
                return;
            }

            shader.Use();
            int attrLoc = GL.GetUniformLocation(shader.Handle, "textures[0]");

            GL.Uniform1(attrLoc, textureHandlers.Length, textureHandlers);
            for (int i = 0; i < textureHandlers.Length; i++)
            {
                GL.ActiveTexture(units_all[i]);
            }
            GL.BindTextures(textureHandlers[0], textureHandlers.Length, textureHandlers);
        }
        
    }
}
