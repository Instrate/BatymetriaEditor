using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using OpenTK.Wpf;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.Diagnostics;
using System.Collections.ObjectModel;

using SceneEditor.editor;

namespace SceneEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        

        private Editor scene;

        float ticker = 0;

        public MainWindow()
        {
            InitializeComponent();

            this.initItems();

            panelRoot.Focus();
            //glMain.Focus();
        }

        public static void EventInform(ListView console, String text)
        {
            var amount = console.Items.Count;

            if (amount > 100)
            {
                console.Items.Clear();
            }

            console.Items.Add(text);
            console.SelectedIndex = console.Items.Count;

        }

        private void initItems()
        {
            try
            {
                

                listViewProcess.Items.Clear();

                glMain.Ready += OnReady;

                glMain.MouseMove += OnMouseMove;
                glMain.MouseEnter += OnMouseEnter;
                glMain.MouseLeave += OnMouseLeave;
                glMain.MouseDown += OnMouseButtonDown;

                glMain.KeyDown += OnKeyDown;
                glMain.KeyUp += OnKeyUp;

                glMain.SizeChanged += onResize;


                EventInform(listViewProcess, "render size: " + glMain.RenderSize.ToString());

                var windowSettings = new GLWpfControlSettings { MajorVersion = 4, MinorVersion = 5, GraphicsProfile = ContextProfile.Compatability, GraphicsContextFlags = ContextFlags.Debug };
                glMain.Start(windowSettings);

                EventInform(listViewProcess, "Editor has been started successfuly");
            }
            catch (Exception ex)
            {
                EventInform(listViewProcess, "An error occured");
                MessageBox.Show(ex.Message);
            }

        }

        private void OnMouseButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if(e.LeftButton == MouseButtonState.Pressed)
            //{
            //    glMain.CaptureMouse();
            //}
            
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            scene.OnMouseWheel(e);
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            panelRoot.Focus();
            String msg = "MousePosition: outside the window";
            textMouse.Text = msg;
            textKey.Text = "Pressed: window isn't focused";
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            glMain.Focus();
            scene.OnResize(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight });
            textKey.Text = "Pressed: no key is pressed yet";
            //glMain.InvalidateVisual();
            
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            textKey.Text = "Pressed: " + key.ToString();
            
            scene.OnKeyDown(e);

            if (scene.cameras[scene.activeCam].grabedMouse)
            {
                tabWindows.Background = Brushes.Green;
            }
            else
            {
                tabWindows.Background = Brushes.Red;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs mouse)
        {
            Point pos = mouse.GetPosition(this);

            String msg = "MousePosition: " + pos.ToString();

            textMouse.Text = msg;

            
            scene.OnMouseMove(mouse, pos, glMain.PointToScreen(pos));
            //glMain.InvalidateVisual();
        }



        private void OnReady()
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.PolygonSmooth);
            //GL.Enable(EnableCap.ScissorTest);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            scene = new Editor(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight });


            //for (int i = 0; i < scene.test.Count; i++)
            //{
            //    EventInform(listViewProcess, "[" + (i + 1).ToString() + "]: " + scene.test[i].ToString()); 
            //}

            //ObservableCollection<object> oList;
            //oList = new ObservableCollection<object>(SettingsApp);
            //SettingsApp.DataContext = oList;

            //Binding binding = new Binding();
            //SettingsApp.SetBinding(ListBox.ItemsSourceProperty, binding);

            //foreach (var cam in scene.cameras)
            //{
            //    (SettingsApp.ItemsSource as ObservableCollection<object>).Add("Lol");
            //}
            
        }

        private float tickerFrames = 0;

        private void OnRender(TimeSpan delta)
        {
            // textFps.Text = "FPS: " + 1.0f / delta.TotalSeconds;/*+ 1.0f / delta.TotalSeconds * 1000;*/
            ticker += (float)delta.TotalSeconds;
            if(ticker >= 0.5)
            {
                textFps.Text = "FPS: " + (Math.Truncate(1.0f / delta.TotalSeconds)).ToString();
                ticker = 0;
                scene.Render();
            }

            tickerFrames += (float)delta.TotalSeconds;
            if(tickerFrames < 1f / 24)
            {
                scene.Render();
            }
            else
            {
                tickerFrames = 0;
            }
        }

        private void onResize(object sender, SizeChangedEventArgs e)
        {
            //MessageBox.Show("resized");
            if (sender is GLWpfControl)
            {
                scene.OnResize(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight });
                //glMain.Width = glMain.Height;
                //GL.Viewport(0, 0, (int)glMain.Width, (int)glMain.Height);
            }
        }


    }


}
