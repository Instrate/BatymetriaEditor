using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.Graphics.OpenGL4;

using LearnOpenTK.Common;
using System.Diagnostics;
using OpenTK.Mathematics;
using System.Windows.Input;

namespace SceneEditor.editor
{
    public static class ShaderPath
    {
        private static string shaderPath = "../../../shaders/";
        public static string vert { get => shaderPath + "shader.vert"; }
        public static string lightVert { get => shaderPath + "lightShader.vert"; }
        public static string frag { get => shaderPath + "shader.frag"; }
    }

    public static class TexturePath
    {
        private static string resourcesPath = "../../../resources/images/";
        public static string wall { get => resourcesPath + "wall.jpg"; }
        public static string oriental_tiles { get => resourcesPath + "oriental-tiles.png"; }
        public static string morocco { get => resourcesPath + "morocco.png"; }
        public static string morocco_blue { get => resourcesPath + "morocco-blue.png"; }
        public static string pxtile { get => resourcesPath + "px_by_Gre3g.png"; }
        public static string criss_cross { get => resourcesPath + "criss-cross.png"; }
        public static string cork_board { get => resourcesPath + "cork-board.png"; }
    }

    public class Editor
    {
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private float elapsedTime = 0;
        private float timeDelta = 0;

        public Shader shader;

        int[] textureHandlers;

        Square sqr;
        //Square[] test;
        ComplexPlaneTile bottom;
        Cube[] cubes;

        public CameraControl[] cameras;

        public int activeCam;

        Vector3 lightPos;
        Cube lightBubble;

        //public List<TextureUnit> test = TextureLoader.units_all;

        Size selfSize;

        Matrix4 model;
        Matrix4 modelCramble;
        Matrix4 view;

        // add switchable uniform bool useGradient
        public Editor(Size windowSize)
        {
            selfSize = windowSize;

            //textureHandlers = new int[2];
            //textureHandlers[0] = TextureLoader.LoadFromFile(TexturePath.wall);
            //textureHandlers[1] = TextureLoader.LoadFromFile(TexturePath.oriental_tiles);

            //sqr = new Square(textureSet: new string[] { TexturePath.wall, TexturePath.morocco_blue }, pos: new Vector3() { Z = 0f });

            //test = new Square[2] { 
            //    new Square(textureSet: new string[] { TexturePath.wall, TexturePath.pxtile }, pos: new Vector3() { Z = 0f }),
            //    new Square(textureSet: new string[] { TexturePath.wall, TexturePath.pxtile }, pos: new Vector3() { Z = 1f })
            //};

            //foreach(var square in test)
            //{
            //    square.Scale(new Vector3(2f));
            //}

            //sqr.Scale(new Vector3(10f));

            //bottom = new ComplexPlaneTile(textureSet: new string[] { TexturePath.morocco_blue, TexturePath.cork_board });

            cubes = new Cube[1] {
                new Cube(pos: new Vector3() { Z = 2f }, color: new Vector3(1.0f, 0f, 0f)),
                //new Cube(pos: new Vector3() { Z = 2f }, color: new Vector3(1.0f, 0.5f, 0.31f)),
                //new Cube(pos: new Vector3() { Z = 2f }, color: new Vector3(1.0f, 0.5f, 0.31f)),
                //new Cube(pos: new Vector3() { Z = 2f }, color: new Vector3(1.0f, 0.5f, 0.31f))
            };

            lightBubble = new Cube(pos: new Vector3() { Z = 3.3f , X = -1.9f, Y = 0.9f});
            lightBubble.Scale(new Vector3(0.1f));

            //cubes = new Cube[4] { 
            //    new Cube(pos: new Vector3() { Z = 2f }, textureSet: new string[] { TexturePath.pxtile, TexturePath.pxtile }),
            //    new Cube(pos: new Vector3() { Z = 2f }, textureSet: new string[] { TexturePath.pxtile, TexturePath.pxtile }),
            //    new Cube(pos: new Vector3() { Z = 2f }, textureSet: new string[] { TexturePath.pxtile, TexturePath.pxtile }),
            //    new Cube(pos: new Vector3() { Z = 2f }, textureSet: new string[] { TexturePath.pxtile, TexturePath.pxtile })
            //}; 

            //foreach(var cube in cubes)
            //{
            //    cube.Scale(new Vector3(1.5f));
            //}

            //cubes[0].Rotate(new Vector3() { X = MathHelper.DegreesToRadians(45f), Y = MathHelper.DegreesToRadians(90f), Z = 0 });
            //cubes[1].Rotate(new Vector3() { X = MathHelper.DegreesToRadians(45f), Y = 0, Z = MathHelper.DegreesToRadians(90f) });
            //cubes[3].Rotate(new Vector3() { X = 0, Y = MathHelper.DegreesToRadians(45f), Z = MathHelper.DegreesToRadians(90f) });



            shader = new Shader(ShaderPath.lightVert, ShaderPath.frag);
            shader.Use();

            activeCam = 0;

            cameras = new CameraControl[1];
            cameras[activeCam] = new CameraControl(new Vector2i() { X = 800, Y = 600 });
            cameras[activeCam].cam.Fov = 90f;

            //var ortho = Matrix4.CreateOrthographic((float)selfSize.Width, (float)selfSize.Height, 0.1f, 100f);

            model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(0f));
            modelCramble = Matrix4.Transpose(model.Inverted());
            shader.SetMatrix4("model_cramble", modelCramble);
            view = cameras[activeCam].cam.GetViewMatrix();
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", cameras[activeCam].cam.GetProjectionMatrix());
        }

        private Vector2i SizeToVector2i(Size size)
        {
            return new Vector2i { X = (int)size.Width, Y = (int)size.Height };
        }

        public void addCamera(bool isOrthogonalPerspective = false)
        {
            if (cameras.Length != 0 && cameras.Length < 5)
            {
                CameraControl[] camsBuffer = new CameraControl[cameras.Length + 1];
                for (int i = 0; i < cameras.Length; i++)
                {
                    camsBuffer[i] = cameras[i];
                }
                camsBuffer[cameras.Length] = new CameraControl(SizeToVector2i(selfSize));
            }
        }

        public void OnResize(Size windowSizeNew)
        {
            selfSize = windowSizeNew;

            //Console.WriteLine("size: ");
            //Console.WriteLine(selfSize);

            cameras[activeCam].cam.AspectRatio = (float)(selfSize.Width / selfSize.Height);

            shader.SetMatrix4("projection", cameras[activeCam].cam.GetProjectionMatrix());

            GL.Viewport(0, 0, (int)selfSize.Width, (int)selfSize.Height);
        }

        public void OnMouseWheel(MouseWheelEventArgs e)
        {
            //cameras[activeCam].cam.Fov = 90f - e.Delta / 10;
            //shader.SetMatrix4("projection", cameras[activeCam].cam.GetProjectionMatrix());
        }

        public int[] getAttribLocations(int programHandle, string[] names)
        {
            int[] layouts = new int[names.Length]; 
            for (int i = 0; i < names.Length; i++)
            {
                layouts[i] = GL.GetAttribLocation(programHandle, names[i]);
            }
            return layouts;
        }

        // rewrite shader usage for elements
        public void Render()
        {
            var elapsed = (float)_stopwatch.Elapsed.TotalSeconds;
            var hue = elapsed * 0.15f % 1;
            timeDelta = elapsed - elapsedTime;
            elapsedTime = elapsed;

            //background cleaning
            var c = Color4.FromHsv(new Vector4(hue, 0.75f, 0.75f, 1));
            //GL.ClearColor(c);
            //GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //texture buffer loading
            //for(int i = 0; i < textureHandlers.Length && i < 32; i++)
            //{
            //    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
            //}

            //model = Matrix4.CreateRotationX(0.5f) * Matrix4.CreateRotationY(1f) * timeDelta;

            foreach (var cube in cubes)
            {
                cube.Rotate(new Vector3() { X = 0.0f, Y = 0f, Z = 0.0f } * timeDelta);
            }

            //foreach(var square in test)
            //{
            //    //square.Rotate(new Vector3() { X = 0.5f, Y = 0, Z = 0 } * timeDelta);
            //    //square.Move()
            //}

            //if(testTime > 1)
            //{
            //    test[0].Move(new Vector3() { Y = (timeDelta * 1.01f - 0.5f) });
            //    testTime -= timeDelta;
            //    if (testTime < 1)
            //    {
            //        testTime = 0;
            //    }
            //}
            //else
            //{
            //    test[0].Move(new Vector3() { Y = -(timeDelta * 1.01f - 0.5f) });
            //    testTime += timeDelta;
            //    if(testTime > 1)
            //    {
            //        testTime = 2;
            //    }
            //}

            //foreach(Cube cube in cubes)
            //{
            //    cube.Rotate(new Vector3() { Z = -timeDelta });
            //}

            //lightPos = new Vector3() { X = 1 + MathF.Sin(elapsedTime) * 2, Y = MathF.Sin(elapsedTime / 2) , Z = 1};
            lightBubble.Move(new Vector3() { X = MathF.Sin(elapsedTime) / 20, Y = MathF.Cos(elapsedTime) / 20 });
            

            shader.Use();
            shader.SetMatrix4("model", model);
            shader.SetVector3("lightColor", new Vector3(c.R, c.G, c.B));
            shader.SetVector3("lightPos", lightBubble.position - new Vector3(0, 0, 0.2f));
            shader.SetVector3("viewPos", cameras[activeCam].cam.Position);

            RenderObject(lightBubble);
            
            //for (int i = 0; i < textureHandlers.Length && i < 32; i++)
            //{
            //    shader.SetInt("texture" + (i+1).ToString(), i);
            //}

            //RenderObject(bottom);
            //RenderObject(test);
            RenderObject(cubes);
            

            GL.Finish();
        }

        private void RenderObject(IRenderable renderable)
        {
            renderable.Render(shader.Handle);

            if (textureHandlers != null)
            {
                for (int i = 0; i < textureHandlers.Length && i < 32; i++)
                {
                    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
                }
            }
        }

        private void RenderObject(IRenderable[] renderable)
        {
            for (int i = 0; i < renderable.Length; i++)
            {
                renderable[i].Render(shader.Handle);
            }

            if (textureHandlers != null)
            {
                for (int i = 0; i < textureHandlers.Length && i < 32; i++)
                {
                    TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
                }
            }
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            var key = e.Key;

            var shift = 0.125f;
            float rotation = (MathF.Pow(2, -4) * MathF.PI);
            float scale = 1.0625f;
           
            if(key == Key.F)
            {
                cameras[activeCam].grabedMouse = !cameras[activeCam].grabedMouse;
            }
            if (cameras[activeCam].grabedMouse)
            {
                cameras[activeCam].OnKeyDown(e, timeDelta);
                view = cameras[activeCam].cam.GetViewMatrix();
                shader.SetMatrix4("view", cameras[activeCam].cam.GetViewMatrix());

            }
            
        }

        public void OnMouseMove(MouseEventArgs mouse, Point pointGL, Point pointScreen)
        {
            if (cameras[activeCam].grabedMouse)
            {
                cameras[activeCam].OnMouseMove(mouse, selfSize, pointGL, pointScreen);

                view = cameras[activeCam].cam.GetViewMatrix();
                shader.SetMatrix4("view", view);
            }
            else
            {
                cameras[activeCam].OnMouseLeave();
            }
            
        }

        ~Editor()
        {
            //GL.DeleteShader(shader.Handle);
        }
    }
}
