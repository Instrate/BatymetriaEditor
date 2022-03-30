﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnOpenTK.Common
{
    //class Text
    //{
    //    private const string Characters = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789µ§½!""#¤%&/()=?^*@£€${[]}\~¨'-_.:,;<>|°©®±¥";

    //    private readonly Vector4 _color;

    //    private static readonly Dictionary<char, int> Lookup;
    //    public static readonly float CharacterWidthNormalized;
    //    //21*48 per char
    //    public readonly List<RenderChar> Text;
        
    //    public Bitmap GenerateCharacters(int fontSize, string fontName, out Size charSize)
    //    {
    //        var characters = new List<Bitmap>();
    //        using (var font = new Font(fontName, fontSize))
    //        {
    //            for (int i = 0; i < Characters.Length; i++)
    //            {
    //                var charBmp = GenerateCharacter(font, Characters[i]);
    //                characters.Add(charBmp);
    //            }
    //            charSize = new Size(characters.Max(x => x.Width), characters.Max(x => x.Height));
    //            var charMap = new Bitmap(charSize.Width * characters.Count, charSize.Height);
    //            using (var gfx = Graphics.FromImage(charMap))
    //            {
    //                gfx.FillRectangle(Brushes.Black, 0, 0, charMap.Width, charMap.Height);
    //                for (int i = 0; i < characters.Count; i++)
    //                {
    //                    var c = characters[i];
    //                    gfx.DrawImageUnscaled(c, i * charSize.Width, 0);

    //                    c.Dispose();
    //                }
    //            }
    //            return charMap;
    //        }
    //    }

    //    private Bitmap GenerateCharacter(Font font, char c)
    //    {
    //        var size = GetSize(font, c);
    //        var bmp = new Bitmap((int)size.Width, (int)size.Height);
    //        using (var gfx = Graphics.FromImage(bmp))
    //        {
    //            gfx.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);
    //            gfx.DrawString(c.ToString(), font, Brushes.White, 0, 0);
    //        }
    //        return bmp;
    //    }
    //    private SizeF GetSize(Font font, char c)
    //    {
    //        using (var bmp = new Bitmap(512, 512))
    //        {
    //            using (var gfx = Graphics.FromImage(bmp))
    //            {
    //                return gfx.MeasureString(c.ToString(), font);
    //            }
    //        }
    //    }
    //}

    //public class RenderChar
    //{
    //    private float _offset;

    //    public RenderChar(ARenderable model, Vector4 position, float charOffset) : base(model, position, Vector4.Zero, 0) {
    //        _offset = charOffset;
    //        _scale = new Vector3(0.2f);
    //    }

    //    public void SetChar(float charOffset)
    //    {
    //        _offset = charOffset;
    //    }

    //    public void Render(Camera camera)
    //    {

    //    }
    //}
}
