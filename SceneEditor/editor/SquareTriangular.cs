using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    //public class SquareTriangular : IDrawable
    //{
    //    public Vector3 position = Vector3.Zero;

    //    private Matrix4 transform = Matrix4.Identity;
    //    private Matrix4 rotation = Matrix4.Identity;
    //    private Matrix4 scale = Matrix4.Identity;
    //    private Matrix4 originShift = Matrix4.Identity;

    //    bool is_rotated = false;

    //    public int[] textureHandlers = null;

    //    Triangle[] trgs;

    //    public SquareTriangular(string[]? textureSet = null, Vector2[]? builder = default, Vector4? heights = default, Vector3 pos = default, Vector3? color = default)
    //    {
    //        trgs = new Triangle[2]
    //        {
    //            new Triangle(),
    //            new Triangle(secondTriangle: true)
    //        };
    //        //trgs[0].Rotate(new Vector3(MathHelper.DegreesToRadians(180F), MathHelper.DegreesToRadians(90F), 0));

    //        if (builder != default)
    //        {
                
    //        }
    //        else
    //        {

    //        }


    //        if (pos != default)
    //        {
    //            position = pos;
    //            Move(position);
    //        }

    //        if (textureSet != null)
    //        {
    //            textureHandlers = new int[textureSet.Length];
    //            for (int i = 0; i < textureSet.Length; i++)
    //            {
    //                textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
    //            }
    //        }

    //        RotateIn(MathHelper.DegreesToRadians(90f));
    //    }

    //    public void Move(Vector3 shifts)
    //    {
    //        foreach (Triangle triangle in trgs)
    //        {
    //            triangle.Move(shifts);
    //        }
    //    }

    //    public void Rotate(Vector3 angles)
    //    {
    //        foreach (Triangle triangle in trgs)
    //        {
    //            triangle.Rotate(angles);
    //        }
    //    }

    //    public void RotateIn(float angle)
    //    {
    //        if (MathF.Abs(angle) > MathHelper.PiOver2)
    //        {
    //            angle %= MathHelper.PiOver2;
    //        }

    //        //is_rotated = !is_rotated;
    //        //if (is_rotated)
    //        //{
    //        //    trgs[0].Rotate(new Vector3(0, 0, MathHelper.DegreesToRadians(-180f)));
    //        //}
            
    //        //trgs[0].Rotate(new Vector3(angle, -angle, angle));
    //        //trgs[1].Rotate(new Vector3(-angle / 2, -angle / 2, 0));
    //    }

    //    public void Scale(Vector3 scalar)
    //    {
    //        foreach(Triangle triangle in trgs)
    //        {
    //            triangle.Scale(scalar);
    //        }
    //    }

    //    public void TransformCombiner()
    //    {
    //        transform = scale * rotation * originShift;

    //    }

    //    public void Render(int shaderHandle, PrimitiveType primitiveType = 0)
    //    {

    //        //texture loading
    //        if (textureHandlers != null && textureHandlers.Length > 0)
    //        {

    //            for (int i = 0; i < textureHandlers.Length && i < 32; i++)
    //            {
    //                TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
    //            }
    //        }

    //        //geometry
    //        var id = GL.GetUniformLocation(shaderHandle, "transform");
    //        GL.UniformMatrix4(id, false, ref transform);

    //        // drawing processed geometry
    //        trgs[0].Render(shaderHandle);
    //        trgs[1].Render(shaderHandle);

    //        // for safe drawing
    //        //GL.BindVertexArray(0);

    //    }

    //    public void TransformClean()
    //    {
    //        scale = originShift = rotation = Matrix4.Identity;
    //    }
    //}
}
