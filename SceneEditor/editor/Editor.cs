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
        public static string dark_paths { get => resourcesPath + "dark-paths.png"; }
    }

    public class Axis : IRenderable
    {
        Cube[] axis = new Cube[3];

        public int[]? textureHandlers;

        public Axis(string[]? textureSet = null)
        {
            for(int i = 0; i < 3; i++)
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

    public class Editor
    {
        // renderable objects
        public Lazy<List<ComplexPlaneTile>> tiledBottoms = new Lazy<List<ComplexPlaneTile>>();
        public Lazy<List<Section>> sections = new Lazy<List<Section>>();
        public Lazy<List<ComplexPlaneTriangular>> nonTiledBottoms = new Lazy<List<ComplexPlaneTriangular>>();
        public bool doUpdate = false;

        // rendrabe visual additions
        Mesh mesh;
        Axis axis;

        // to remove
        ComplexPlaneTile bottom;
        Section section;
        
        // fix usage
        //Vector3 lightPos;
        Cube lightBubble;
        

        // for control
        public CameraControl[] cameras;
        public int activeCam;

        // for the settings
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private float elapsedTime = 0;
        private float timeDelta = 0;
        Size selfSize;

        // for the renderer
        public Shader shader;
        Matrix4 model;
        Matrix4 modelCramble;
        Matrix4 view;
        int[] textureHandlers;

        // add switchable uniform bool useGradient
        public Editor(Size windowSize)
        {
            selfSize = windowSize;

            
            
            addNewUnMeshedObject(new ComplexPlaneTriangular(shouldTriangulate:true, textureSet: new string[] { TexturePath.dark_paths, TexturePath.cork_board, TexturePath.oriental_tiles }));

            //bottom = new ComplexPlaneTile(textureSet: new string[] { TexturePath.dark_paths, TexturePath.cork_board, TexturePath.criss_cross });

            //bottom = nonTiledBottoms.Value[0].ConvertToTiledByInterpolation();

            section = new Section(new Vector3(0,0,3), new Vector3(3, 8, 0), textureSet: new string[] { TexturePath.criss_cross, TexturePath.pxtile });

            addNewSection(section);

            //addNewBottom(bottom);
            addNewBottom(new ComplexPlaneTile(textureSet: new string[] { TexturePath.dark_paths, TexturePath.cork_board, TexturePath.criss_cross }));

            _setupCam();
            _setupObjects();
            _setupTextures();
            _setupShader();
        }

        public void addNewBottom(ComplexPlaneTile tiledBottom)
        {
            tiledBottoms.Value.Add(tiledBottom);
            doUpdate = true;
        }

        public void addNewSection(Section section)
        {
            sections.Value.Add(section);
            doUpdate = true;
        }

        public void addNewUnMeshedObject(ComplexPlaneTriangular item)
        {
            nonTiledBottoms.Value.Add(item);
            doUpdate = true;
        }

        private void _setupObjects()
        {
            axis = new Axis();
            mesh = new Mesh(size: 10, step:15, width: 1);

            lightBubble = new Cube(pos: new Vector3() { Z = 7f, X = 0, Y = 0 });
            lightBubble.Scale(new Vector3(0.1f));
        }

        private void _setupTextures()
        {
            textureHandlers = new int[3];
            textureHandlers[0] = TextureLoader.LoadFromFile(TexturePath.wall);
            textureHandlers[1] = TextureLoader.LoadFromFile(TexturePath.oriental_tiles);
            textureHandlers[2] = TextureLoader.LoadFromFile(TexturePath.criss_cross);
        }

        private void _setupCam()
        {
            cameras = new CameraControl[1];
            cameras[activeCam] = new CameraControl(new Vector2i() { X = 800, Y = 600 }, false);
            cameras[activeCam].cam.Fov = 90f;
            activeCam = 0;
        }

        private void _setupShader()
        {
            shader = new Shader(ShaderPath.lightVert, ShaderPath.frag);
            shader.Use();

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
                
                activeCam = cameras.Length;
                cameras = camsBuffer;
                cameras[activeCam].cam.Fov = 90f;

                view = cameras[activeCam].cam.GetViewMatrix();
                shader.SetMatrix4("view", view);
            }
        }

        public void cameraChange(int shift = 0)
        {
            activeCam += shift;
            if(activeCam == cameras.Length)
            {
                activeCam = 0;
            }
            if(activeCam < 0)
            {
                activeCam = cameras.Length - 1;
            }
            view = cameras[activeCam].cam.GetViewMatrix();
            shader.SetMatrix4("view", cameras[activeCam].cam.GetViewMatrix());
        }

        [MTAThread]
        public void OnResize(Size windowSizeNew)
        {
            selfSize = windowSizeNew;

            //Console.WriteLine("size: ");
            //Console.WriteLine(selfSize);

            cameras[activeCam].cam.AspectRatio = (float)(selfSize.Width / selfSize.Height);

            shader.SetMatrix4("projection", cameras[activeCam].cam.GetProjectionMatrix());

            GL.Viewport(0, 0, (int)selfSize.Width, (int)selfSize.Height);
        }

        public void OnMouseButtonPressed(MouseButtonEventArgs mouse)
        {

            if (mouse.RightButton == MouseButtonState.Pressed)
            {
                cameras[activeCam].grabedMouse = !cameras[activeCam].grabedMouse;
            }
            if (cameras[activeCam].grabedMouse)
            {
                view = cameras[activeCam].cam.GetViewMatrix();
                shader.SetMatrix4("view", cameras[activeCam].cam.GetViewMatrix());
            }
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

        private void _applyTextureUnits()
        {
            for (int i = 0; i < textureHandlers.Length && i < 32; i++)
            {
                TextureLoader.Use(TextureLoader.units_all[i], textureHandlers[i]);
            }
        }

        private void _applyShaderSettings()
        {
            shader.Use();
            shader.SetMatrix4("model", model);
            //shader.SetVector3("lightColor", new Vector3(c.R, c.G, c.B));
            shader.SetVector3("lightColor", new Vector3(1));
            shader.SetVector3("lightPos", lightBubble.position - new Vector3(0, 0, -0.2f));
            //shader.SetVector3("lightPos", cameras[activeCam].cam.Position);
            shader.SetVector3("viewPos", cameras[activeCam].cam.Position);

            for (int i = 0; i < textureHandlers.Length && i < 32; i++)
            {
                shader.SetInt("texture" + (i + 1).ToString(), i);
            }
        }

        private void _applyRenderQueue()
        {
            RenderObject(mesh);
            RenderObject(axis);
            RenderObject(lightBubble);

            //tiledBottoms.Value.ForEach(bottom => RenderObject(bottom));
            sections.Value.ForEach(section => RenderObject(section));
            nonTiledBottoms.Value.ForEach(item => RenderObject(item));
        }

        // rewrite shader usage for elements
        [MTAThread]
        public void Render()
        {
            var elapsed = (float)_stopwatch.Elapsed.TotalSeconds;
            //var hue = elapsed * 0.15f % 1;
            timeDelta = elapsed - elapsedTime;
            elapsedTime = elapsed;

            //background cleaning
            //var c = Color4.FromHsv(new Vector4(hue, 0.75f, 0.75f, 1));
            GL.ClearColor(0.4f, 0.4f, 0.4f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _applyTextureUnits();
            _applyShaderSettings();
            _applyRenderQueue();
            GL.Finish();
        }

        [STAThread]
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

        [STAThread]
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

        [MTAThread]
        public void OnKeyDown(KeyEventArgs e)
        {
            var key = e.Key;

            //var shift = 0.125f;
            //float rotation = (MathF.Pow(2, -4) * MathF.PI);
            //float scale = 1.0625f;
           
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
            else
            {
                if (key == Key.OemPlus)
                {
                    addCamera();
                }
                if (key == Key.NumPad9)
                {
                    cameraChange(1);
                }
                if (key == Key.NumPad3)
                {
                    cameraChange(-1);
                }
                if (key == Key.I)
                {
                    bottom.Interp(0.9f, 0);
                }
                if (key == Key.PageUp)
                {
                    bottom.MoveVisibleMesh(0, 0, 0, 0, 1, 1);
                }
                if (key == Key.PageDown)
                {
                    bottom.MoveVisibleMesh(0, 0, 0, 0, -1, -1);
                }
                if (key == Key.Up)
                {
                    bottom.MoveVisibleMesh(0, 1, 0, 0, 0, 0);
                }
                if (key == Key.Down)
                {
                    bottom.MoveVisibleMesh(0, -1, 0, 0, 0, 0);
                }
                if (key == Key.Left)
                {
                    bottom.MoveVisibleMesh(-1, 0, 0, 0, 0, 0);
                }
                if (key == Key.Right)
                {
                    bottom.MoveVisibleMesh(1, 0, 0, 0, 0, 0);
                }
                if (key == Key.J)
                {
                    // test intersection
                    section.IntersectTiled(bottom.Xmesh, bottom.Ymesh, bottom.DataBuffer);
                    section.CountPolarFunction();
                }
                if (key == Key.P)
                {
                    bottom.SwitchDotsVisibility();
                }
                if (key == Key.O)
                {
                    bottom.SwitchDrawStyle();
                }
                if (key == Key.L)
                {
                    section.SwitchPlaneDisplaying();
                }
                if (key == Key.T)
                {
                    //section.SwitchPlaneDisplaying();
                }
            }
        }

        [MTAThread]
        public void OnMouseMove(MouseEventArgs mouse, Point pointGL, Point pointScreen)
        {
            
            if (cameras[activeCam].grabedMouse)
            {
                cameras[activeCam].OnMouseMove(mouse, selfSize, pointGL, pointScreen);

                view = cameras[activeCam].cam.GetViewMatrix();
                shader.SetMatrix4("view", view);

                if (mouse.LeftButton == MouseButtonState.Pressed)
                {
                    //axis.RotateAlongWithCamera(cameras[activeCam]);
                }
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
