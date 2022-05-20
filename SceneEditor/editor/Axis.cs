using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class Axis : IRenderable
    {
        Cube[] axis = new Cube[3];

        public int[]? textureHandlers;

        public Axis(string[]? textureSet = null)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 color = Vector3.Zero;
                color[i] = 1;
                axis[i] = new Cube(color: color);
            }

            float scaling = 0.05f;
            float sizeScale = 200f;

            axis[0].Scale(new Vector3(sizeScale, scaling, scaling));
            axis[1].Scale(new Vector3(scaling, sizeScale, scaling));
            axis[2].Scale(new Vector3(scaling, scaling, sizeScale));

            axis[0].Move(new Vector3() { X = 0.5f * sizeScale - scaling / 2 });
            axis[1].Move(new Vector3() { Y = 0.5f * sizeScale - scaling / 2 });
            axis[2].Move(new Vector3() { Z = 0.5f * sizeScale - scaling / 2 });

            //foreach (var ax in axis)
            //{
            //    ax.Move(new Vector3(0, 0, 3f));
            //}

            if (textureSet != null)
            {
                textureHandlers = new int[textureSet.Length];
                for (int i = 0; i < textureSet.Length; i++)
                {
                    textureHandlers[i] = TextureLoader.LoadFromFile(textureSet[i]);
                }
            }
        }

        public void Render(int shaderHandle, PrimitiveType primitiveType = 0)
        {
            if (textureHandlers != null && textureHandlers.Length > 0)
            {

                for (int i = 0; i < textureHandlers.Length && i < 32; i++)
                {
                    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
                }
            }

            foreach (var c in axis)
            {
                c.Render(shaderHandle);
            }
        }

        public void MoveAlongWithCamera(CameraControl camera)
        {

            //Vector3 fix = new Vector3(-0.2f, -0.2f, 0);
            //Vector3 shifts = camera.cam.Position - axis[0].position;

            //foreach (var ax in axis)
            //{
            //    ax.Move(shifts);
            //}


        }

        public void RotateAlongWithCamera(CameraControl camera)
        {
            //float pitch = camera.cam.Pitch;
            //float yaw = camera.cam.Yaw;

            //Vector3 shifts = camera.cam.Position - axis[0].position;

            //Vector3 YAW = new Vector3(-1, 0, 0);

            //float Yaw = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(shifts, YAW));

            //var x = shifts.X;
            //var y = shifts.Y;
            //var Yawdif = yaw - Yaw;

            //Vector3 posH = new Vector3(MathF.Cos(Yawdif) * x - MathF.Sin(Yawdif) * y, MathF.Sin(Yawdif) * x + MathF.Cos(Yawdif) * y, 0);

            //Console.WriteLine("Yawdif: " + Yawdif);
            //Console.WriteLine("A: " + shifts);
            //Console.WriteLine("pos: " + axis[0].position);
            //Console.WriteLine("Shift: " + posH + "\n");

            //foreach (var ax in axis)
            //{
            //    ax.Move(shifts - posH);
            //}
        }
    }
}
