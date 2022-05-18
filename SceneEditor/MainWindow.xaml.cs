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
using System.Threading;

namespace SceneEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MultiThreader Trender;

        private List<Editor> editors = new List<Editor>();
        private int currentEditor;

        float ticker = 0;

        
        public MainWindow()
        {
            InitializeComponent();

            this.initItems();

            panelRoot.Focus();
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

        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            editors[currentEditor].OnMouseWheel(e);
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
            editors[currentEditor].OnResize(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight });
            textKey.Text = "Pressed: no key is pressed yet";
            
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            textKey.Text = "Pressed: " + key.ToString();

            editors[currentEditor].OnKeyDown(e);

            if (editors[currentEditor].cameras[editors[currentEditor].activeCam].grabedMouse)
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


            editors[currentEditor].OnMouseMove(mouse, pos, glMain.PointToScreen(pos));
        }



        private void OnReady()
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.LineSmooth);
            //GL.Enable(EnableCap.PolygonSmooth);
            //GL.Enable(EnableCap.ScissorTest);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            editors.Add(new Editor(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight }));
            currentEditor = 0;

            Trender = new MultiThreader(8, editors[currentEditor].Render);
        }

        private float tickerFrames = 0;

        [MTAThread]
        private void OnRender(TimeSpan delta)
        {
            ticker += (float)delta.TotalSeconds;
            if(ticker >= 0.5)
            {
                textFps.Text = "FPS: " + (Math.Truncate(1.0f / delta.TotalSeconds)).ToString();
                ticker = 0;
                //editors[currentEditor].Render();
            }

            //Trender.activateFreeThread();

            tickerFrames += (float)delta.TotalSeconds;
            if (tickerFrames < 1f / 30)
            {
                //
                editors[currentEditor].Render();
            }
            else
            {
                tickerFrames = 0;
            }
        }

        private void onResize(object sender, SizeChangedEventArgs e)
        {
            if (sender is GLWpfControl)
            {
                editors[currentEditor].OnResize(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight });
            }
        }

        private void addNewEditor()
        {
             
        }
    }


}
