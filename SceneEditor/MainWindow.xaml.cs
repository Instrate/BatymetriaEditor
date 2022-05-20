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

        //MultiThreader Trender;

        // TODO: make it a resizable array for performance
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

                // OpenGL v4.5.0 if your video card doesn't support it -> :(
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

            switchVisibleGLStatus();
        }

        private void OnMouseMove(object sender, MouseEventArgs mouse)
        {
            Point pos = mouse.GetPosition(this);

            String msg = "MousePosition: " + pos.ToString();

            textMouse.Text = msg;

            editors[currentEditor].OnMouseMove(mouse, pos, glMain.PointToScreen(pos));

            switchVisibleGLStatus();
        }

        private void OnPreviewMouseKeyPressed(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            editors[currentEditor].OnMouseButtonPressed(e);

            switchVisibleGLStatus();
        }

        private void switchVisibleGLStatus()
        {
            if (editors[currentEditor].cameras[editors[currentEditor].activeCam].grabedMouse)
            {
                tabWindows.Background = Brushes.Black;
                glMain.Cursor = Cursors.None;
            }
            else
            {
                tabWindows.Background = Brushes.FloralWhite;
                if (glMain.IsFocused)
                {
                    glMain.Cursor = Cursors.Cross;
                }
                else
                {
                    glMain.Cursor = Cursors.Arrow;
                }
            }
        }

        private void OnReady()
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.LineSmooth);
            //GL.Enable(EnableCap.PolygonSmooth);
            //GL.Enable(EnableCap.ScissorTest);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            editorAddNew();

            //seamless optimization but is it?
            //Trender = new MultiThreader(8, editors[currentEditor].Render);

            editorStructure.Loaded += structureOnReady;

            switchVisibleGLStatus();
        }

        private float tickerFrames = 0;

        [MTAThread]
        private void OnRender(TimeSpan delta)
        {


            ticker += (float)delta.TotalSeconds;
            if (ticker >= 0.5)
            {
                textFps.Text = "FPS: " + (Math.Truncate(1.0f / delta.TotalSeconds)).ToString();
                ticker = 0;
                if (editors[currentEditor].doUpdate)
                {
                    structureUpdate();
                    editors[currentEditor].doUpdate = false;
                }
            }

            tickerFrames += (float)delta.TotalSeconds;
            if (tickerFrames < 1f / 30)
            {
                editors[currentEditor].Render();
            }
            else
            {
                tickerFrames = 0;
            }
        }

        private void structureOnReady(object sender, RoutedEventArgs e)
        {
            editorStructure.SelectionChanged += structureSelectionChanged;
        }

        private void structureSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            editorChangeCurrent(editorStructure.SelectedIndex);
            structureUpdate();
        }

        private TreeViewItem _addNewTreeMember(string Header)
        {
            TreeViewItem item = new TreeViewItem() { Header = Header};
            //item.Header = Header;
            return item;
        }

        private void structureCreate()
        {
            editorStructure.SelectedIndex = currentEditor;
            TabItem item = (TabItem)editorStructure.SelectedItem;
            TreeView treeView = (TreeView)item.Content;      
        }

        private void structureViewAdd()
        {

        }

        private void structureUpdate()
        {

            editorStructure.SelectedIndex = currentEditor;
            TabItem item = (TabItem)editorStructure.SelectedItem;
            TreeView treeView = (TreeView)item.Content;

            treeView.Items.Clear();
            treeView.Items.Add(_addNewTreeMember("Mesh"));
            treeView.Items.Add(_addNewTreeMember("Axis"));
            //treeView.Items.Add(_addNewTreeMember("Bottoms"));
            //treeView.Items.Add(_addNewTreeMember("Section"));

            
            int amount = editors[currentEditor].meshTiled.Value.Count;
            
            if(amount == 0)
            {
                //view.Items.Add(_addNewTreeMember("None"));
            }
            else
            {
                treeView.Items.Add(_addNewTreeMember("Bottoms"));
                TreeViewItem view = (TreeViewItem)treeView.Items.GetItemAt(treeView.Items.Count - 1);
                for (int i = 0; i < amount; i++)
                {
                    view.Items.Add(_addNewTreeMember("bottom " + (i + 1).ToString()));
                }
            }

            amount = editors[currentEditor].sections.Value.Count;
            if (amount == 0)
            {
                //view.Items.Add(_addNewTreeMember("None"));
            }
            else
            {
                treeView.Items.Add(_addNewTreeMember("Section"));
                TreeViewItem view = (TreeViewItem)treeView.Items.GetItemAt(treeView.Items.Count-1);
                for (int i = 0; i < amount; i++)
                {
                    view.Items.Add(_addNewTreeMember("section " + (i + 1).ToString()));
                }
            }

            amount = editors[currentEditor].meshUneven.Value.Count;
            if (amount == 0)
            {
                //view.Items.Add(_addNewTreeMember("None"));
            }
            else
            {
                treeView.Items.Add(_addNewTreeMember("Triangulated plane"));
                TreeViewItem view = (TreeViewItem)treeView.Items.GetItemAt(treeView.Items.Count - 1);
                for (int i = 0; i < amount; i++)
                {
                    view.Items.Add(_addNewTreeMember("mesh " + (i + 1).ToString()));
                }
            }
        }

        private void onResize(object sender, SizeChangedEventArgs e)
        {
            if (sender is GLWpfControl)
            {
                editors[currentEditor].OnResize(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight });
            }
        }

        private void editorAddNew()
        {
            editors.Add(new Editor(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight }));
            currentEditor = editors.Count - 1;

            structureUpdate();
        }

        private void editorChangeCurrent(int editorIndex)
        {
            int amount = editors.Count;
            if(editorIndex < 0 || editorIndex >= amount)
            {
                return;
            }
            currentEditor = editorIndex;
        }
        
        // !!!!!
        private void elementSettingsLoad()
        {

        }

    }


}
