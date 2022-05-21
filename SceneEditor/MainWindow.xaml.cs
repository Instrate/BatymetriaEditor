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
        private EditorSettings currentEditor;
        private int currentEditorNum;

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
                EventInform(listViewProcess, "Press \"F\" or RMB over GL window to switch mouse being grabbed");
                EventInform(listViewProcess, "Keep pressing LMB to move camera while mouse grabbed");
                EventInform(listViewProcess, "Use WASD, Ctrl, Shift to move");
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
            editors[currentEditorNum].OnMouseWheel(e);
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
            editors[currentEditorNum].OnResize(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight });
            textKey.Text = "Pressed: no key is pressed yet";

            
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            textKey.Text = "Pressed: " + key.ToString();

            if (editors[currentEditorNum].cameras[editors[currentEditorNum].activeCam].grabedMouse)
            {
                editors[currentEditorNum].OnKeyDown(e);
            }
            else
            {
                currentEditor.OnKeyDown(e); 
            }

            switchVisibleGLStatus();

            if (currentEditor.cameras[currentEditor.activeCam].isMoved)
            {
                currentEditor.CamUpdatedPosition();
                currentEditor.cameras[currentEditor.activeCam].isMoved = false;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs mouse)
        {
            Point pos = mouse.GetPosition(this);

            String msg = "MousePosition: " + pos.ToString();

            textMouse.Text = msg;

            editors[currentEditorNum].OnMouseMove(mouse, pos, glMain.PointToScreen(pos));

            //// posible event placer
            //if (currentEditor.doInvalidate)
            //{
                
            //    currentEditor.doInvalidate = false;
            //}


            switchVisibleGLStatus();
        }

        private void OnPreviewMouseKeyPressed(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(glMain.IsMouseDirectlyOver == true)
            {
                editors[currentEditorNum].OnMouseButtonPressed(e);
            }
            
            switchVisibleGLStatus();
        }

        private void switchVisibleGLStatus()
        {
            if (editors[currentEditorNum].cameras[editors[currentEditorNum].activeCam].grabedMouse)
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
            editorStructure.SelectionChanged += EditorStructureEditorChanged;
            editorSceneSettings.Loaded += EditorSceneSettingsLoaded;


            switchVisibleGLStatus();

            //glMain.RenderContinuously = true;
        }

        private void EditorStructureEditorChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tab = (TabItem) editorStructure.SelectedItem;
            if(tab != null)
            {
                if(tab.Header.ToString().CompareTo("+") == 0)
                {
                    editorAddNew();
                    tab.Header = "Editor " + editors.Count;
                    //editorStructure.Items.Add(Generate.TabItem(ButtonEventDeleteCurrentEditor));
                }
                structureUpdate();
            }
        }

        private void EditorSceneSettingsLoaded(object sender, RoutedEventArgs e)
        {
            currentEditor.EditorChanged();
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
                if (editors[currentEditorNum].doUpdate)
                {
                    structureUpdate();
                    editors[currentEditorNum].doUpdate = false;
                }
            }

            tickerFrames += (float)delta.TotalSeconds;
            if (tickerFrames < 1f / 30)
            {
                editors[currentEditorNum].Render();
            }
            else
            {
                tickerFrames = 0;
            }
        }

        private void structureOnReady(object sender, RoutedEventArgs e)
        {
            editorStructure.SelectionChanged += structureSelectionChanged;
            editorStructure.SelectedIndex = currentEditorNum;
            structureUpdate();

            //currentEditor = editors[currentEditorNum];
            //currentEditor.addDependencyBoxes(editorElementSettings, editorSceneSettings);
        }

        private void structureSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            editorChangeCurrent(editorStructure.SelectedIndex);
            structureUpdate();
        }

        private void structureUpdate()
        {
            if (editors[currentEditorNum].isLoaded && editorStructure.IsLoaded && editorStructure.SelectedItem != null)
            {
                EditorSettings editor = editors[currentEditorNum];
                TabItem tab = (TabItem)editorStructure.SelectedItem;
                DockPanel dock = (DockPanel) tab.Content;
                TreeView treeView = (TreeView)dock.Children[0];
                editor.UpdateTreeView(treeView);
            }     
        }

        private void onResize(object sender, SizeChangedEventArgs e)
        {
            if (sender is GLWpfControl)
            {
                editors[currentEditorNum].OnResize(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight });
            }
        }

        private void editorAddNew()
        {
            editors.Add(new Editor(new Size { Width = glMain.ActualWidth, Height = glMain.ActualHeight }));
            editorChangeCurrent(editors.Count - 1);

            editorStructure.Items.Add(Generate.TabItem(ButtonEventDeleteCurrentEditor));

            structureUpdate();
        }

        public void ButtonEventDeleteCurrentEditor(object sender, RoutedEventArgs e)
        {
            editorStructure.SelectedItem = editorStructure.Items[editorStructure.SelectedIndex - 1];

            DockPanel panel = (DockPanel) ((Button)sender).Parent;
            TabItem tab = (TabItem)panel.Parent;
            editorStructure.Items.Remove(tab);
        }

        private void editorChangeCurrent(int editorIndex)
        {
            int amount = editors.Count;
            if(editorIndex < 0 || editorIndex >= amount)
            {
                return;
            }
            currentEditorNum = editorIndex;

            currentEditor = editors[currentEditorNum];
            currentEditor.addDependencyBoxes(editorElementSettings, editorSceneSettings);

            if (editorSceneSettings.IsLoaded)
            {
                currentEditor.EditorChanged();
            }
            
        }

    }
}
