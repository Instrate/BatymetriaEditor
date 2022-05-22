using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SceneEditor.editor
{
    public class Editor : EditorSettings
    {
        // to remove
        ComplexPlaneTile bottom;
        Section section;

        // fix usage
        //Vector3 lightPos;
        Cube lightBubble;

        // for the settings
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private float elapsedTime = 0;
        private float timeDelta = 0;


        // for the renderer

        Matrix4 model;
        Matrix4 modelCramble;

        int[] textureHandlers;

        public bool isLoaded = false;

        public bool isRendered = false;

        // add switchable uniform bool useGradient
        public Editor(Size windowSize)
        {
            selfSize = windowSize;

            Initialize();


            addNewPointsDataset(
                new ComplexPlaneTriangular(
                    shouldTriangulate: true,
                    textureSet: new string[] { TexturePath.dark_paths,
                        TexturePath.dark_paths,
                        TexturePath.dark_paths
                    }));

            //bottom = new ComplexPlaneTile(textureSet: new string[] { TexturePath.dark_paths, TexturePath.cork_board, TexturePath.criss_cross });

            ////bottom = meshUneven.Value[0].ConvertToTiledByInterpolation();

            //section = new Section(new Vector3(-4, -7, 4), new Vector3(3, 6, 0), textureSet: new string[] { TexturePath.criss_cross, TexturePath.pxtile });

            //addNewSection(section);

            //addNewBottom(bottom);

            _setupCam();
            _setupObjects();
            _setupTextures();
            _setupShader();

            isLoaded = true;
        }

        public void Initialize()
        {
            meshTiled = new Lazy<List<ComplexPlaneTile>>();
            meshUneven = new Lazy<List<ComplexPlaneTriangular>>();
            sections = new Lazy<List<Section>>();

            isEnabled = true;
        }


        private void _setupObjects()
        {
            axis = new Axis();
            mesh = new Mesh(size: 20, step: 5, width: 0.5f);
            mesh.showMesh[1] = mesh.showMesh[2] = false;

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
                UpdateView();
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
            //RenderObject(lightBubble);

            meshTiled.Value.ForEach(bottom => RenderObject(bottom));
            sections.Value.ForEach(section => RenderObject(section));
            meshUneven.Value.ForEach(item => RenderObject(item));
        }


        // only sync
        public void Render()
        {
            var elapsed = (float)_stopwatch.Elapsed.TotalSeconds;
            //var hue = elapsed * 0.15f % 1;
            timeDelta = elapsed - elapsedTime;
            elapsedTime = elapsed;

            if (isEnabled)
            {
                //background cleaning
                //var c = Color4.FromHsv(new Vector4(hue, 0.75f, 0.75f, 1));

                GL.ClearColor(0.4f, 0.4f, 0.4f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                _applyTextureUnits();
                _applyShaderSettings();
                _applyRenderQueue();
                GL.Finish();

                isRendered = true;
            }
        }

        [MTAThread]
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

        [MTAThread]
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

            if (key == Key.F)
            {
                cameras[activeCam].grabedMouse = !cameras[activeCam].grabedMouse;
            }
            if (cameras[activeCam].grabedMouse)
            {
                cameras[activeCam].OnKeyDown(e, timeDelta);
                UpdateView();
            }
            else
            {
                
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
