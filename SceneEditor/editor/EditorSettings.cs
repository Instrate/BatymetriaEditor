using LearnOpenTK.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;

/*
    things to add
    highlighting selected point
    mesh size to size(m)
    water plane with opacity and maybe simulation
 */

namespace SceneEditor.editor
{
    public static class ObjectNames
    {
        // length is Enum.GetNames(typeof(StructuresNames)).Length
        public static string[] objectsNames = new string[]
        {
            "Mesh tiled",
            "Sections",
            "Mesh uneven"
        };
    }

    public class EditorSettings
    {
        //public Task? renderTask;

        protected Size selfSize;

        //make it private later
        //public Lazy<List<TreeViewItem>> objectsWithProperties = new Lazy<List<TreeViewItem>>();

        public ListBox elementProperties;
        public ListBox editorProperties;
        private object? currentProperty;
        public ListView listViewProcess;

        protected bool isEnabled = false;
        public bool doUpdate = false;
        //public bool doInvalidate = false;

        public Lazy<List<ComplexPlaneTile>> meshTiled;
        public Lazy<List<ComplexPlaneTriangular>> meshUneven;
        public Lazy<List<Section>> sections;

        public Mesh mesh;
        public Axis axis;
        public ComplexPlaneTile water;

        public CameraControl[] cameras;
        public int activeCam;

        public int blendingStyle = 0;
        public Shader shader;
        protected Matrix4 view;

        public void ChangeBlending()
        {
            blendingStyle++;
            if (blendingStyle == 5)
            {
                blendingStyle = 0;
            }

            switch (blendingStyle)
            {
                case 1:
                    {
                        GL.BlendFunc(BlendingFactor.OneMinusDstColor, BlendingFactor.SrcAlpha);
                    }; break;
                case 2:
                    {
                        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcColor);
                    }; break;
                case 3:
                    {
                        GL.BlendFunc(BlendingFactor.SrcColor, BlendingFactor.OneMinusSrcAlpha);
                    }; break;
                default:
                    {
                        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
                    }; break;
            }
        }

        public void addDependencyBoxes(ListBox? elementProperties, ListBox? editorProperties)
        {
            this.elementProperties = elementProperties;
            this.editorProperties = editorProperties;

            EventInform("Editor has been started successfuly");
            EventInform("Keep pressing LMB to move camera while mouse grabbed");
            EventInform("Use WASD, Ctrl, Shift to move");
            //EditorChanged();
        }

        public bool CurrentPropertyGetState()
        {
            return currentProperty != null;
        }

        public void EventInform(object? content, ListView? console = null)
        {
            if (console == null){
                console = listViewProcess;
            }
            var amount = console.Items.Count;

            if (amount > 100)
            {
                console.Items.Clear();
            }

            if(content is string)
            {
                console.Items.Add(content);
            }
            else
            {
                //complete it later
                console.Items.Add(content);
            }
            
            console.SelectedIndex = console.Items.Count;
            console.ScrollIntoView(console.SelectedItem);
        }

        private void _putExistingInTree(TreeView treeView, int amount, int indexName)
        {
            if (amount != 0)
            {
                string header = ObjectNames.objectsNames[indexName];
                treeView.Items.Add(_addNewTreeMember(header));
                TreeViewItem view = (TreeViewItem)treeView.Items.GetItemAt(treeView.Items.Count - 1);
                for (int i = 0; i < amount; i++)
                {
                    TreeViewItem item = _addNewTreeMember(header.ToLower() + " " + (i + 1));
                    view.Items.Add(item);
                    item.Selected += ObjectSelected;
                    //objectsWithProperties.Value.Add(item);
                }
                view.IsExpanded = true;
            }
        }

        // can be void
        public TreeView UpdateTreeView(TreeView treeView)
        {
            treeView.Items.Clear();
            treeView.Items.Add(_addNewTreeMember("Mesh"));
            treeView.Items.Add(_addNewTreeMember("Axis"));

            _putExistingInTree(treeView, meshTiled.Value.Count, 0);
            _putExistingInTree(treeView, sections.Value.Count, 1);
            _putExistingInTree(treeView, meshUneven.Value.Count, 2);

            return treeView;
        }

        private TreeViewItem _addNewTreeMember(string Header)
        {
            TreeViewItem item = new TreeViewItem() { Header = Header };
            return item;
        }

        private void ObjectSelected(object sender, RoutedEventArgs e)
        {

            TreeViewItem item = (TreeViewItem)sender;
            TreeViewItem parent = (TreeViewItem)item.Parent;

            string[] data = item.Header.ToString().Split(' ');
            int num = int.Parse(data[data.Length - 1]) - 1;

            int index = 0;
            for (;
                index < ObjectNames.objectsNames.Length &&
                ObjectNames.objectsNames[index].CompareTo(parent.Header.ToString()) != 0;
                index++) ;

            switch (index)
            {
                case 0:
                    {
                        LookUpProperties(meshTiled.Value.ElementAt(num), elementProperties);
                    }; break;
                case 1:
                    {
                        LookUpProperties(sections.Value.ElementAt(num), elementProperties);
                    }; break;
                case 2:
                    {
                        LookUpProperties(meshUneven.Value.ElementAt(num), elementProperties);
                    }; break;
            }

            // add properties

            elementProperties.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        // object settings 
        // TODO: make @enabled@ checkbox smaller 
        // var dataItem = currentProperty is ComplexPlaneTile ? (ComplexPlaneTile) currentProperty : (ComplexPlaneTriangular) currentProperty;
        private object? LookUpProperties(object itemWithProperties, ListBox list)
        {
            int index = -1;
            list.Items.Clear();

            bool isEnabled = false;
            if (itemWithProperties is ComplexPlaneTile)
            {
                index = 0;
                currentProperty = (ComplexPlaneTile)itemWithProperties;
                isEnabled = (currentProperty as ComplexPlaneTile).isEnabled;
            }
            else if (itemWithProperties is Section)
            {
                index = 1;
                currentProperty = (Section)itemWithProperties;
                isEnabled = (currentProperty as Section).isEnabled;
            }
            else if (itemWithProperties is ComplexPlaneTriangular)
            {
                index = 2;
                currentProperty = (ComplexPlaneTriangular)itemWithProperties;
                isEnabled = (currentProperty as ComplexPlaneTriangular).isEnabled;
            }

            switch (index)
            {
                case 0:
                    {
                        CheckBox vertices = CreateCheckBoxSetting(
                                                    "Show vertices",
                                                    (currentProperty as ComplexPlaneTile).showDots,
                                                    SwitchVerticesDisplayingForTiles
                                                    );
                        list.Items.Add(vertices);

                        CheckBox geometry = CreateCheckBoxSetting(
                                                    "Show geometry",
                                                    (currentProperty as ComplexPlaneTile).showGeometry,
                                                    SwitchDisplayMainGeometry
                                                    );
                        list.Items.Add(geometry);

                        Button buttonStyle = Generate.Button(
                            "Switch primitive type",
                            SwitchDrawStyle
                            );
                        list.Items.Add(buttonStyle);

                        
                        Expander expTranform = Generate.Expander(
                            "Transform",
                            CreateListTransformForTiles()
                            );
                        expTranform.IsExpanded = false;
                        list.Items.Add(expTranform);

                    }; break;
                case 1:
                    {
                        //vertices
                        ListBox listSet = new ListBox();
                        Expander expVert = Generate.Expander("Vertices", listSet);
                        list.Items.Add(expVert);

                        if ((currentProperty as Section).intersected)
                        {
                            CheckBox showAreaMesh = CreateCheckBoxSetting(
                                                   "Show intersected area",
                                                   (currentProperty as Section).showAreaMesh,
                                                   SectionSwitchAreaDisplay
                                                   );
                            list.Items.Add(showAreaMesh);

                            CheckBox showPolar = CreateCheckBoxSetting(
                                                   "Show polar function",
                                                   (currentProperty as Section).showPolar,
                                                   SectionSwitchPolarDisplay
                                                   );
                            list.Items.Add(showPolar);
                        }
                        if (meshTiled.Value.Count != 0)
                        {
                            ComboBox comboChoose = new ComboBox();
                            Button buttonIntersect = Generate.Button(
                            "Intersect",
                            ButtonIntersect
                            );
                            ListBox listOpt = new ListBox();
                            listOpt.Items.Add(Generate.ListBoxItem(comboChoose));
                            listOpt.Items.Add(Generate.ListBoxItem(buttonIntersect));

                            for (int i = 0; i < meshTiled.Value.Count; i++)
                            {
                                comboChoose.Items.Add(Generate.ComboBoxItem("mesh " + (i + 1)));
                            }
                            comboChoose.SelectedItem = comboChoose.Items[0];

                            list.Items.Add(listOpt);
                        }

                        listSet.Items.Add(CreateSettingForVector3Point(
                            "Start point",
                            (currentProperty as Section).chars[0],
                            SectionVertChanged
                            ));
                        listSet.Items.Add(CreateSettingForVector3Point(
                            "End point",
                            (currentProperty as Section).chars[1],
                            SectionVertChanged
                            ));




                    }; break;
                default:
                    {
                        CheckBox vertices = CreateCheckBoxSetting(
                                                    "Show vertices",
                                                    (currentProperty as ComplexPlaneTriangular).showDots,
                                                    SwitchVerticesDisplayingForTiles
                                                    );
                        list.Items.Add(vertices);

                        CheckBox geometry = CreateCheckBoxSetting(
                                                    "Show geometry",
                                                    (currentProperty as ComplexPlaneTriangular).showGeometry,
                                                    SwitchDisplayMainGeometry
                                                    );
                        list.Items.Add(geometry);

                        Button buttonStyle = Generate.Button(
                            "Switch primitive type",
                            SwitchDrawStyle
                            );
                        list.Items.Add(buttonStyle);

                        Button buttonToMesh = Generate.Button(
                            "Tranform to mesh",
                            ButtonTransformTriangulatedToMesh
                            );
                        list.Items.Add(buttonToMesh);
                                                

                        Expander pointsListExp = CreateRangedListVector3Points(
                            "Points dataset",
                            (currentProperty as ComplexPlaneTriangular).dataSetAdjustable,
                            TextChangedVector3PointValueTriangular,
                            SliderPointsListSlide,
                            ButtonPointAdd,
                            ButtonPointDeleteSelected
                            );
                        pointsListExp.IsExpanded = false;

                        list.Items.Add(pointsListExp);

                        Expander expTriangulate = CreateTriangulationExpander();
                        expTriangulate.IsExpanded = false;

                        list.Items.Add(expTriangulate);
                    }; break;
            }

            Button buttonExport = Generate.Button(
                            "Export data",
                            ExportData
                            );
            list.Items.Add(buttonExport);

            CheckBox enabled = CreateCheckBoxSetting(
                            "Enable",
                            isEnabled,
                            SwitchObjectDrawingEnabled
                            );
            list.Items.Add(enabled);

            Button buttonRemove = Generate.Button(
                            "Remove me",
                            ButtonDelete
                            );
            list.Items.Add(buttonRemove);
            return null;
        }

        private ListBox CreateListTransformForTiles()
        {
            ListBox list = new();

            Button buttonShowLess = Generate.Button("Shrink", ShowLessCells);
            Button buttonMoveLeft = Generate.Button("Shift to the left", ButtonMoveVisibleTilesLeft);
            Button buttonMoveRight = Generate.Button("Shift to the right", ButtonMoveVisibleTilesRight);
            Button buttonMoveForward = Generate.Button("Shift forward", ButtonMoveVisibleTilesForward);
            Button buttonMoveBack = Generate.Button("Shift back", ButtonMoveVisibleTilesBack);
            Button buttonShowMore = Generate.Button("Expand", ShowMoreCells);

            list.Items.Add(buttonShowLess);
            list.Items.Add(buttonMoveLeft);
            list.Items.Add(buttonMoveRight);
            list.Items.Add(buttonMoveForward);
            list.Items.Add(buttonMoveBack);
            list.Items.Add(buttonShowMore);

            DockPanel panel = new DockPanel();

            ListBoxItem interpItem = Generate.ListBoxItem(panel);
            list.Items.Add(interpItem);

            TextBlock textInterp = Generate.TextBlock("Interpolation");
            TextBlock textLabelScale = Generate.TextBlock("Scale:");
            TextBox boxScale = Generate.TextBox("1");
            Button buttonInterp = Generate.Button("Interpolate", ButtonInterpolateTiles);

            textInterp.VerticalAlignment = VerticalAlignment.Top;
            textInterp.HorizontalAlignment = HorizontalAlignment.Center;

            textLabelScale.HorizontalAlignment = HorizontalAlignment.Left;
            textLabelScale.VerticalAlignment = VerticalAlignment.Center;

            boxScale.HorizontalAlignment = HorizontalAlignment.Stretch;
            boxScale.VerticalAlignment = VerticalAlignment.Center;

            buttonInterp.VerticalAlignment = VerticalAlignment.Bottom;

            DockPanel.SetDock(textInterp, Dock.Top);
            DockPanel.SetDock(textLabelScale, Dock.Top);
            DockPanel.SetDock(boxScale, Dock.Top);
            DockPanel.SetDock(buttonInterp, Dock.Bottom);

            panel.Children.Add(textInterp);
            panel.Children.Add(textLabelScale);
            panel.Children.Add(boxScale);
            panel.Children.Add(buttonInterp);

            list.HorizontalAlignment = HorizontalAlignment.Stretch;
            list.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            return list;
        }

        private void ButtonInterpolateTiles(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            try
            {
                TextBox box = (TextBox)((DockPanel)button.Parent).Children[2];
                float scale = float.Parse(box.Text);
                var data = (ComplexPlaneTile)currentProperty;
                data.Interp(scale);
            }
            catch(Exception ex) { }
        }

        private void ShowLessCells(object sender, RoutedEventArgs e)
        {
            ((ComplexPlaneTile)currentProperty).MoveVisibleMesh(shrinkx: 1, shrinky: 1);
            //EventInform("Mesh: amount of cells to be displayed is reduced");
        }

        private void ShowMoreCells(object sender, RoutedEventArgs e)
        {
            ((ComplexPlaneTile)currentProperty).MoveVisibleMesh(shrinkx: -1, shrinky: -1);
            //EventInform("Mesh: amount of cells to be displayed is iscreased");
        }

        private void ButtonMoveVisibleTilesLeft(object sender, RoutedEventArgs e)
        {
            ((ComplexPlaneTile)currentProperty).MoveVisibleMesh(x: -1);
        }

        private void ButtonMoveVisibleTilesRight(object sender, RoutedEventArgs e)
        {
            ((ComplexPlaneTile)currentProperty).MoveVisibleMesh(x: 1);
        }

        private void ButtonMoveVisibleTilesForward(object sender, RoutedEventArgs e)
        {
            ((ComplexPlaneTile)currentProperty).MoveVisibleMesh(y: 1);
        }

        private void ButtonMoveVisibleTilesBack(object sender, RoutedEventArgs e)
        {
            ((ComplexPlaneTile)currentProperty).MoveVisibleMesh(y: -1);
        }

        private Expander CreateTriangulationExpander()
        {
            ListBox list = new();
            list.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            Expander expander = Generate.Expander("Triangulation options", list);

            CheckBox check1 = Generate.CheckBox("Mesh should be convex");
            CheckBox check2 = Generate.CheckBox("Use stricted area for triangles");
            CheckBox check3 = Generate.CheckBox("Confirm Delaunay statement");
            Button button = Generate.Button("Triangulate\n(may take a while)", ButtonTriangulate);
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            list.Items.Add(check1);
            list.Items.Add(check2);
            list.Items.Add(check3);
            list.Items.Add(button);
            return expander;
        }

        private void ButtonTriangulate(object sender, RoutedEventArgs e)
        {
            ComplexPlaneTriangular data = (ComplexPlaneTriangular)currentProperty;
            ListBox list = (ListBox)((Button)sender).Parent;
            bool[] options = new bool[3];

           
            
            for(int i = 0; i < 3; i++)
            {
                CheckBox check = (CheckBox) list.Items[i];
                options[i] = check.IsChecked.Value;
            }
            var clock = new Stopwatch();
            clock.Start();
            bool state = data.generateDelaunayTriangulationFromData(options[0], options[1], options[2]);
            EventInform("Trigangulatio: operation state " + (state ? "\'done successfuly\'" : "\'error occured\'"));
            EventInform("Triangulation: completed and took " + clock.Elapsed.TotalSeconds + " seconds");
            clock.Stop();
        }

        private Expander CreateRangedListVector3Points(
            string name,
            List<Vector3> listValues,
            TextChangedEventHandler textHandlerEvent,
            RoutedPropertyChangedEventHandler<double> slideEvent,
            RoutedEventHandler buttonEventAdd,
            RoutedEventHandler buttonEventRemove,
            int lowerInd = 0,
            int maxAmount = 5)
        {
            
            DockPanel panel = new();
            Expander exp = Generate.Expander(name, panel);
            Slider slider = new();
            slider.Orientation = Orientation.Vertical;
            slider.VerticalAlignment = VerticalAlignment.Stretch;
            slider.Minimum = 0;
            slider.Maximum = listValues.Count - 1;
            slider.Width = 30;
            slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
            ListBox listHandler = new();
            ScrollViewer scroll = Generate.ScrollViewer(listHandler);
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            scroll.HorizontalAlignment = HorizontalAlignment.Stretch;

            // TODO: make a passing parameter 
            listHandler.SelectionChanged += EventListPointSelectedTriangularSet;

            Button buttonCreatePoint = Generate.Button("Add point", buttonEventAdd);
            Button buttonRemovePoint = Generate.Button("Remove selected point", buttonEventRemove);

            DockPanel.SetDock(slider, Dock.Left);
            DockPanel.SetDock(scroll, Dock.Top);
            DockPanel.SetDock(buttonCreatePoint, Dock.Bottom);
            DockPanel.SetDock(buttonRemovePoint, Dock.Bottom);
            buttonCreatePoint.VerticalAlignment = VerticalAlignment.Bottom;
            buttonRemovePoint.VerticalAlignment = buttonCreatePoint.VerticalAlignment;
            panel.Children.Add(slider);
            panel.Children.Add(scroll);
            panel.Children.Add(buttonCreatePoint);
            panel.Children.Add(buttonRemovePoint);

            if (lowerInd + maxAmount >= listValues.Count)
            {
                lowerInd = listValues.Count - maxAmount - 1;
            }
            if (lowerInd < 0)
            {
                lowerInd = 0;
            }

            slider.Value = listValues.Count - lowerInd - 1;
            slider.IsSnapToTickEnabled = true;
            slider.TickFrequency = 1;
            slider.ValueChanged += slideEvent;
            slider.MouseWheel += SliderMouseWheel;

            var selectedItems = listValues.Where(el => {
                int temp = listValues.IndexOf(el) - lowerInd;
                return temp >= 0 && temp < maxAmount;
            }).ToArray();

            if (selectedItems.Length >= maxAmount)
            {
                for (int i = 0; i < maxAmount; i++)
                {
                        var point = selectedItems[i];
                        listHandler.Items.Add(CreateSettingForVector3Point(
                            "Point " + (lowerInd + i + 1),
                            point,
                            textHandlerEvent
                            ));
                }
            }
            return exp;
        }

        private void EventListPointSelectedTriangularSet(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = (ListBox)sender;
            if(list.SelectedIndex != -1)
            {
                Slider slider = (Slider)((DockPanel)((ScrollViewer)list.Parent).Parent).Children[0];
                var data = (ComplexPlaneTriangular)currentProperty;
                int index = data.dataSetAdjustable.Count - (int)slider.Value + list.SelectedIndex;
                data.HighlightDot(index);
            }
        }

        private void RecreateListOfPoints(ListBox list, Expander exp, int value)
        {
            int ind = list.Items.IndexOf(exp);
            list.Items[ind] = CreateRangedListVector3Points(
                exp.Header.ToString(),
                (currentProperty as ComplexPlaneTriangular).dataSetAdjustable,
                TextChangedVector3PointValueTriangular,
                SliderPointsListSlide,
                ButtonPointAdd,
                ButtonPointDeleteSelected,
                value
                );
        }

        private void SliderMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider slider = (Slider)sender;
            Expander exp = ((Expander)((DockPanel)slider.Parent).Parent);
            ListBox? list = ((ListBox) exp.Parent);
            if (list != null && e.Delta != 0)
            {
                ComplexPlaneTriangular data = (ComplexPlaneTriangular)currentProperty;
                int abs = e.Delta / Math.Abs(e.Delta);
                int value = data.dataSetAdjustable.Count - (int)slider.Value - abs * list.Items.Count - 1;
                RecreateListOfPoints(list, exp, value);
            }
        }

        private void SliderPointsListSlide(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Expander exp = ((Expander)((DockPanel)((Slider)sender).Parent).Parent);
            ListBox? list = ((ListBox) exp.Parent);
            if(list != null)
            {
                int value = (currentProperty as ComplexPlaneTriangular).dataSetAdjustable.Count - (int)e.NewValue;
                RecreateListOfPoints(list, exp, value);
            }
        }

        //fix wrong index
        private void TextChangedVector3PointValueTriangular(object sender, RoutedEventArgs e)
        {
            ComplexPlaneTriangular data = (ComplexPlaneTriangular)currentProperty;
            ListBox handler = GetListWithVector3Settings(sender);
            Slider slider = ((Slider) ((DockPanel) ((ScrollViewer) handler.Parent).Parent).Children[0]);

            int amount = handler.Items.Count;
            //int amountSet = (currentProperty as ComplexPlaneTriangular).dataSetAdjustable.Count;

            int index = handler.Items.IndexOf(GetListItemWithVector3Settings(sender)) + data.dataSetAdjustable.Count - ((int) slider.Value) - 1;
            data.dataSetAdjustable[index] = GetVectorFromListBoxOfVector3Settings(handler, index);
            data.UpdateVertices();
            EventInform("Dot: new value applied");
        }

        private void ButtonPointDeleteSelected(object sender, RoutedEventArgs e)
        {
            ListBox list = ((ListBox)((ScrollViewer)((DockPanel)((Button)sender).Parent).Children[1]).Content);
            if (list.SelectedIndex != -1)
            {
                ComplexPlaneTriangular data = (ComplexPlaneTriangular)currentProperty;
                Slider slider = (Slider)((DockPanel)((Button)sender).Parent).Children[0];
                Expander exp = ((Expander)((DockPanel)slider.Parent).Parent);
                ListBox? listHandler = ((ListBox)exp.Parent);
                int value = data.dataSetAdjustable.Count - (int)slider.Value + list.SelectedIndex - 1;
                data.dataSetAdjustable.Remove(data.dataSetAdjustable[value]);
                data.UpdateVertices();
                RecreateListOfPoints(listHandler, exp, value);
            }
        }

        private void ButtonPointAdd(object sender, RoutedEventArgs e)
        {
            ComplexPlaneTriangular data = (ComplexPlaneTriangular)currentProperty;
            ListBox list = (ListBox)((ScrollViewer)((DockPanel)((Button)sender).Parent).Children[1]).Content;
            Slider slider = (Slider)((DockPanel)((ScrollViewer)list.Parent).Parent).Children[0];
            Expander exp = (Expander)((DockPanel)slider.Parent).Parent;
            ListBox? listHandler = ((ListBox)exp.Parent);
            data.dataSetAdjustable.Add(Vector3.Zero);
            data.UpdateVertices();
            int value = data.dataSetAdjustable.Count - (int)slider.Value + list.SelectedIndex - 1;
            RecreateListOfPoints(listHandler, exp, value);
        }

        private void ButtonDelete(object sender, RoutedEventArgs e)
        {
            removeCurrentItem();
            EventInform("Item: current has been removed");
        }

        private void ButtonTransformTriangulatedToMesh(object sender, RoutedEventArgs e)
        {
            addNewBottom(((ComplexPlaneTriangular)currentProperty).ConvertToTiledByInterpolation());
            EventInform("Mesh: new mesh has been created based on points dataset interpolation");
        }

        private void SectionVertChanged(object sender, TextChangedEventArgs e)
        {
            Vector3[] data = new Vector3[2];
            ListBox list = GetListWithVector3Settings(sender);

            for (int i = 0; i < 2; i++)
            {
                ListBoxItem item = list.Items[i] as ListBoxItem;
                ListBox listVert = ((item.Content as DockPanel).Children[1] as ListBox);
                for (int j = 0; j < 3; j++)
                {
                    TextBox box = (((listVert.Items[j] as ListBoxItem).Content as DockPanel).Children[1] as TextBox);
                    float value;
                    try
                    {
                        value = float.Parse(box.Text.ToString());
                    }
                    catch (Exception ex)
                    {
                        value = (currentProperty as Section).chars[i][j];
                    }

                    data[i][j] = value;
                }
            }
            (currentProperty as Section).Replace(data[0], data[1]);

            EventInform("Section: position of current section has been changed");
        }

        private void SectionSwitchAreaDisplay(object sender, RoutedEventArgs e)
        {
            bool state = (bool)(sender as CheckBox).IsChecked;
            (currentProperty as Section).showAreaMesh = state;

            EventInform("Section: switched display status for intersected area mesh");
        }

        private void ButtonIntersect(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ListBoxItem item = (ListBoxItem)button.Parent;
            ListBox list = (ListBox)item.Parent;
            ListBoxItem itemCombo = (ListBoxItem)list.Items[0];
            ComboBox comboBox = (ComboBox)itemCombo.Content;
            ComboBoxItem comboItem = (ComboBoxItem)comboBox.SelectedItem;
            TextBlock block = (TextBlock)comboItem.Content;
            string text = block.Text;
            string[] opts = text.Split(' ');
            int ind = int.Parse(opts[opts.Length - 1]);

            if (opts[0].CompareTo("mesh") == 0)
            {
                ComplexPlaneTile tiled = meshTiled.Value[ind - 1];
                ((Section)currentProperty).IntersectTiled(tiled.Xmesh, tiled.Ymesh, tiled.DataBuffer);
            }

            EventInform("Section: intersection is done");
        }

        private void SectionSwitchPolarDisplay(object sender, RoutedEventArgs e)
        {
            bool state = (bool)(sender as CheckBox).IsChecked;
            (currentProperty as Section).showPolar = state;
            EventInform("Section: switched display status for polar function on Oyz");
        }

        private void SwitchDrawStyle(object sender, RoutedEventArgs e)
        {
            if (currentProperty is ComplexPlaneTile)
            {
                (currentProperty as ComplexPlaneTile).SwitchDrawStyle();
            }
            else
            if (currentProperty is ComplexPlaneTriangular)
            {
                (currentProperty as ComplexPlaneTriangular).SwitchDrawStyle();
            }
            EventInform("Item: primitive type has been switched");
        }
                
        private void SwitchVerticesDisplayingForTiles(object sender, RoutedEventArgs e)
        {
            bool state = (bool)(sender as CheckBox).IsChecked;
            if (currentProperty is ComplexPlaneTile)
            {
                (currentProperty as ComplexPlaneTile).showDots = state;
            }
            else
            if (currentProperty is ComplexPlaneTriangular)
            {
                (currentProperty as ComplexPlaneTriangular).showDots = state;
            }
            EventInform("Mesh: switched display status for mesh vertices");
        }

        private void SwitchDisplayMainGeometry(object sender, RoutedEventArgs e)
        {
            bool state = (bool)(sender as CheckBox).IsChecked;
            if (currentProperty is ComplexPlaneTile)
            {
                (currentProperty as ComplexPlaneTile).showGeometry = state;
            }
            else
            if (currentProperty is ComplexPlaneTriangular)
            {
                (currentProperty as ComplexPlaneTriangular).showGeometry = state;
            }
            EventInform("Mesh: switched display status for mesh");
        }

        private void SwitchObjectDrawingEnabled(object sender, RoutedEventArgs e)
        {
            bool state = (bool)(sender as CheckBox).IsChecked;
            if (currentProperty is ComplexPlaneTile)
            {
                (currentProperty as ComplexPlaneTile).isEnabled = state;
            }
            else
            if (currentProperty is Section)
            {
                (currentProperty as Section).isEnabled = state;
            }
            else
            if (currentProperty is ComplexPlaneTriangular)
            {
                (currentProperty as ComplexPlaneTriangular).isEnabled = state;
            }
            EventInform("Item: enabled status is switched for this object");
        }

        private void ButtonAddSection(object sender, RoutedEventArgs e)
        {
            addNewSection(new Section(
                new Vector3(0),
                new Vector3(1),
                textureSet: new string[] { TexturePath.criss_cross, TexturePath.pxtile }));
            EventInform("Section: new section has been added");
        }

        // scene settings
        public void EditorChanged()
        {
            editorProperties.Items.Clear();
            // cameras
            ListBox list = new ListBox();
            list.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            for (int i = 0; i < cameras.Length; i++)
            {
                Expander camWithSettings = Generate.Expander("camera " + (i + 1));
                camWithSettings.Content = _getCamSettings(i);
                ListBoxItem item = Generate.ListBoxItem(camWithSettings);
                item.BorderThickness = Generate.Thickness(2);
                list.Items.Add(item);
            }
            Button buttonAddCam = Generate.Button("Add", ButtonCameraAdd);
            Button buttonRemoveCam = Generate.Button("Remove selected", ButtonCameraRemove);

            list.Items.Add(buttonAddCam);
            list.Items.Add(buttonRemoveCam);
            list.SelectionChanged += SelectCamera;

            Expander exp = Generate.Expander("Cameras", list);
            exp.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            editorProperties.Items.Add(exp);

            exp = Generate.Expander("Mesh", CreateMeshSettings());
            editorProperties.Items.Add(exp);

            Button buttonAddSection = Generate.Button(
                "Add section",
                ButtonAddSection);
            editorProperties.Items.Add(buttonAddSection);

            Button buttonImportTileset = Generate.Button(
                "Import data",
                ImportData);
            editorProperties.Items.Add(buttonImportTileset);

            Button buttonBlendStyle = Generate.Button("Switch blending", ButtonSwitchBlending);
            editorProperties.Items.Add(buttonBlendStyle);

            editorProperties.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        private void SelectCamera(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListBox;
            int ind = list.SelectedIndex;
            if (ind >= 0 && ind < cameras.Length)
            {
                activeCam = ind;
            }
        }

        private void ButtonCameraAdd(object sender, RoutedEventArgs e)
        {
            addCamera();
        }
        private void ButtonCameraRemove(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ListBox list = button.Parent as ListBox;
            _cameraRemoveAt(list.SelectedIndex);
        }

        private void ButtonSwitchBlending(object sender, RoutedEventArgs e)
        {
            ChangeBlending();
        }

        // isNeeded
        public void addCamera()
        {
            _addCamera();
            EditorChanged();
            EventInform("Camera: new camera is created and ready for usage");
        }

        private void _addCamera(bool isOrthogonalPerspective = false)
        {
            if (cameras.Length < 10)
            {
                List<CameraControl> buffer = cameras.ToList();
                buffer.Add(new CameraControl(SizeToVector2i(selfSize)));

                activeCam = cameras.Length;
                cameras = buffer.ToArray();
                cameras[activeCam].cam.Fov = 90f;
                UpdateView();
            }
        }

        public void cameraChange(int shift = 0)
        {
            activeCam += shift;
            if (activeCam == cameras.Length)
            {
                activeCam = 0;
            }
            if (activeCam < 0)
            {
                activeCam = cameras.Length - 1;
            }

            UpdateView();
            EventInform("Camera: switched");
        }
        
        private void _cameraRemoveAt(int ind)
        {
            if(ind >= 0 && ind < cameras.Length)
            {
                List<CameraControl> cams = cameras.ToList();
                cams.RemoveAt(ind);
                cameras = cams.ToArray();
            }
        }

        private protected Vector2i SizeToVector2i(Size size)
        {
            return new Vector2i { X = (int)size.Width, Y = (int)size.Height };
        }

        private ListBox CreateMeshSettings()
        {
            ListBox listSet = new ListBox();

            // TODO: make it (m) whole width
            listSet.Items.Add(CreateItemWithValueChangeable(
                "Size(m):",
                mesh.size,
                MeshParamChanged
                ));
            listSet.Items.Add(CreateItemWithValueChangeable(
                "Step(m):",
                mesh.step,
                MeshParamChanged
                ));
            listSet.Items.Add(CreateItemWithValueChangeable(
                "Height:",
                mesh.height,
                MeshParamChanged
                ));

            listSet.Items.Add(Generate.ListBoxItem(CreateCheckBoxSetting(
                "mesh Oxy",
                mesh.showMesh[0],
                MeshTurnOxy
                )));
            listSet.Items.Add(Generate.ListBoxItem(CreateCheckBoxSetting(
                "mesh Oxz",
                mesh.showMesh[1],
                MeshTurnOyz
                )));
            listSet.Items.Add(Generate.ListBoxItem(CreateCheckBoxSetting(
                "mesh Oyz",
                mesh.showMesh[2],
                MeshTurnOxz
                )));

            listSet.Items.Add(Generate.ListBoxItem(CreateWaterLevelSettings()));

            listSet.Items.Add(Generate.ListBoxItem(CreateCheckBoxSetting(
                "Enable",
                mesh.isEnabled,
                CheckBoxMeshSwitched
                )));

            listSet.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            return listSet;
        }

        private ListBoxItem CreateWaterLevelSettings()
        {
            DockPanel panel = new DockPanel();
            TextBlock name = Generate.TextBlock("Water level height: ");
            TextBox box = Generate.TextBox(water.position.Z);
            box.HorizontalAlignment = HorizontalAlignment.Stretch;
            box.TextChanged += TextChangedWaterLevel;
            panel.Children.Add(name);
            panel.Children.Add(box);
            ListBoxItem item = Generate.ListBoxItem(panel);
            item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            return item;
        }

        private void TextChangedWaterLevel(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            try
            {
                float height = float.Parse(box.Text);
                CreateWaterLevel(height);
            }
            catch (Exception ex) { }
        }

        private CheckBox CreateCheckBoxSetting(
            string Name,
            bool initialState,
            RoutedEventHandler Event
            )
        {
            CheckBox checkBox = Generate.CheckBox(Generate.TextBlock(Name), initialState);
            checkBox.Unchecked += Event;
            checkBox.Checked += Event;
            return checkBox;
        }

        private ListBoxItem CreateItemWithValueChangeable(
            string Name,
            float valueIn,
            TextChangedEventHandler textEvent
            )
        {
            DockPanel panel = new DockPanel();
            TextBlock name = Generate.TextBlock(Name);
            TextBox value = Generate.TextBox(valueIn);
            DockPanel.SetDock(name, Dock.Left);
            DockPanel.SetDock(value, Dock.Right);
            value.TextChanged += textEvent;
            panel.Children.Add(name);
            panel.Children.Add(value);
            return Generate.ListBoxItem(panel);
        }

        private ListBoxItem CreateCamSettingWithSlider(
            string Name,
            double min,
            double max,
            float valueIn,
            TextChangedEventHandler textEvent,
            RoutedPropertyChangedEventHandler<double> sliderEvent)
        {
            DockPanel panel = new DockPanel();
            ListBoxItem item = Generate.ListBoxItem(panel);

            TextBlock setting = Generate.TextBlock(Name);
            Slider slider = new Slider();
            float sens = Functions.CutFraction(valueIn, 2);
            slider.Value = sens;
            TextBox value = Generate.TextBox(sens);
            value.Padding = Generate.Thickness(2);
            value.TextChanged += textEvent;
            slider.ValueChanged += sliderEvent;
            slider.Minimum = min;
            slider.Maximum = max;
            slider.TickFrequency = (max - min) / 20;
            DockPanel.SetDock(setting, Dock.Left);
            DockPanel.SetDock(value, Dock.Right);
            DockPanel.SetDock(slider, Dock.Bottom);
            panel.Children.Add(setting);
            panel.Children.Add(value);
            panel.Children.Add(slider);


            item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            return item;
        }

        private Vector3 GetVectorFromListBoxOfVector3Settings(ListBox sender, int index)
        {
            Vector3 result = new Vector3();

            ListBox verts = ((ListBox)((DockPanel)((ListBoxItem)sender.Items[index]).Content).Children[1]);

            for(int i = 0; i < 3; i++)
            {
                TextBox box = ((TextBox)((DockPanel)((ListBoxItem)verts.Items[i]).Content).Children[1]);
                try
                {
                    float value = float.Parse(box.Text.ToString());
                    result[i] = value;
                }catch(Exception ex) { }
            }

            return result;
        }

        private ListBoxItem GetListItemWithVector3Settings(object sender)
        {
            return ((ListBoxItem)((DockPanel)((ListBox)((ListBoxItem)((DockPanel)((TextBox)sender).Parent).Parent).Parent).Parent).Parent);
        }

        private ListBox GetListWithVector3Settings(object sender)
        {
            return (ListBox)GetListItemWithVector3Settings(sender).Parent;
        }

        private ListBoxItem CreateSettingForVector3Point(
            string Name,
            Vector3 valueIn,
            TextChangedEventHandler textEvent
            )
        {
            float[] data = new float[3];
            valueIn.Deconstruct(out data[0], out data[1], out data[2]);
            string[] names = new string[] { "X:", "Y:", "Z:" };

            ListBox list = new ListBox();
            DockPanel panel = new DockPanel();
            ListBoxItem item = Generate.ListBoxItem(panel);
            TextBlock setting = Generate.TextBlock(Name);

            DockPanel.SetDock(setting, Dock.Top);
            DockPanel.SetDock(list, Dock.Bottom);
            panel.Children.Add(setting);
            panel.Children.Add(list);
            list.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            for (int i = 0; i < 3; i++)
            {
                DockPanel panelValue = new DockPanel();
                TextBlock valueName = Generate.TextBlock(names[i]);
                TextBox value = Generate.TextBox(data[i]);
                value.TextChanged += textEvent;
                DockPanel.SetDock(valueName, Dock.Left);
                DockPanel.SetDock(value, Dock.Right);
                panelValue.Children.Add(valueName);
                panelValue.Children.Add(value);
                ListBoxItem itemValue = Generate.ListBoxItem(panelValue);
                itemValue.HorizontalContentAlignment = HorizontalAlignment.Stretch;

                list.Items.Add(itemValue);
            }

            item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            return item;
        }

        private ListBox _getCamSettings(int index)
        {
            ListBox list = new ListBox();

            list.Items.Add(CreateCamSettingWithSlider(
                "Sensitivity",
                0,
                3,
                cameras[index].sensitivity,
                CamSensitivityTextChanged,
                CamSensitivityChanged
                ));
            list.Items.Add(CreateCamSettingWithSlider(
                "Speed",
                0,
                20,
                cameras[index].cameraSpeed,
                CamSpeedTextChanged,
                CamSpeedChanged
                ));
            list.Items.Add(CreateCamSettingWithSlider(
                "Field of view",
                45,
                90,
                cameras[index].cam.Fov,
                CamFovTextChanged,
                CamFovChanged
                ));
            list.Items.Add(CreateSettingForVector3Point(
                "Position",
                cameras[index].cam.Position,
                CamPosTextChanged
                ));

            return list;
        }

        // EVENTS FOR CAM SETTINGS START
        // -----------------------------
        public void CamUpdatedPosition()
        {
            Vector3 newPos = cameras[activeCam].cam.Position;

            if(editorProperties.Items.Count != 0)
            {
                Expander exp = (Expander)editorProperties.Items[0];

                ListBox list = (ListBox)exp.Content;

                ListBoxItem itemActive = (ListBoxItem)list.Items[activeCam];

                Expander expCamI = (Expander)itemActive.Content;

                ListBox listSettings = (ListBox)expCamI.Content;

                // property now
                ListBoxItem itemPos = (ListBoxItem)listSettings.Items[3];

                DockPanel dockPanel = (DockPanel)itemPos.Content;

                ListBox listContent = (ListBox)dockPanel.Children[1];

                for (int i = 0; i < 3; i++)
                {
                    ListBoxItem coord = (ListBoxItem)listContent.Items[i];
                    DockPanel dockSet = (DockPanel)coord.Content;
                    TextBox text = (TextBox)dockSet.Children[1];
                    text.Text = newPos[i].ToString();
                }

                UpdateView();
            }

            
        }

        protected private void UpdateView()
        {
            view = cameras[activeCam].cam.GetViewMatrix();
            shader.SetMatrix4("view", cameras[activeCam].cam.GetViewMatrix());
        }

        private void CamSensitivityTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double newValue = double.Parse(((TextBox)sender).Text);
                newValue = Functions.CutFraction((float)newValue, 2);
                Slider slider = _getSliderOfCamSetting(sender);
                if (newValue > slider.Maximum)
                {
                    newValue = slider.Maximum;
                }
                else
                {
                    if (newValue < slider.Minimum)
                    {
                        newValue = slider.Minimum;
                    }
                }
                slider.Value = newValue;
                //block.Text = newValue.ToString();
                EventInform("Camera: sensitivity has been updated");
            }
            catch (Exception ex) { }
        }

        private void CamSpeedTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double newValue = double.Parse(((TextBox)sender).Text);
                newValue = Functions.CutFraction((float)newValue, 2);
                Slider slider = _getSliderOfCamSetting(sender);
                if (newValue > slider.Maximum)
                {
                    newValue = slider.Maximum;
                }
                else
                {
                    if (newValue < slider.Minimum)
                    {
                        newValue = slider.Minimum;
                    }
                }
                slider.Value = newValue;
                //block.Text = newValue.ToString();
                EventInform("Camera: speed has been updated");
            }
            catch (Exception ex) { }
        }

        private void CamFovTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double newValue = double.Parse(((TextBox)sender).Text);
                newValue = Functions.CutFraction((float)newValue, 2);
                Slider slider = _getSliderOfCamSetting(sender);
                if (newValue > slider.Maximum)
                {
                    newValue = slider.Maximum;
                }
                else
                {
                    if (newValue < slider.Minimum)
                    {
                        newValue = slider.Minimum;
                    }
                }
                slider.Value = newValue;
                //block.Text = newValue.ToString();
                EventInform("Camera: field of view has been updated");
            }
            catch (Exception ex) { }
        }

        private void CamPosTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int camIndex = 0;
                TextBox boxTemp = (TextBox)sender;
                DockPanel panel = (DockPanel)boxTemp.Parent;
                if (panel != null)
                {
                    ListBoxItem itemValue = (ListBoxItem)panel.Parent;
                    ListBox listValue = (ListBox)itemValue.Parent;
                    DockPanel panelValue = (DockPanel)listValue.Parent;

                    ListBoxItem item = (ListBoxItem)panelValue.Parent;
                    ListBox list = (ListBox)item.Parent;
                    Expander exp = (Expander)list.Parent;
                    ListBoxItem itemExp = (ListBoxItem)exp.Parent;
                    ListBox listExp = (ListBox)itemExp.Parent;
                    camIndex = listExp.Items.IndexOf(itemExp);

                    float[] data = new float[3];
                    Vector3 oldPos = cameras[camIndex].cam.Position;

                    for (int i = 0; i < 3; i++)
                    {
                        ListBoxItem itemCoord = (ListBoxItem)listValue.Items[i];
                        DockPanel panelCoord = (DockPanel)itemCoord.Content;
                        try
                        {
                            data[i] = float.Parse(((TextBox)panelCoord.Children[1]).Text.ToString());
                        }
                        catch (Exception ex)
                        {
                            data[i] = oldPos[i];
                        }
                    }
                    Vector3 newPos = new Vector3(data[0], data[1], data[2]);
                    cameras[camIndex].cam.Position = newPos;
                    //doInvalidate = true;
                    //EventInform("Camera: position has been updated");
                }
            }
            catch (Exception ex) { }
        }

        private Slider _getSliderOfCamSetting(object sender)
        {
            TextBlock block = (TextBlock)sender;
            DockPanel panel = (DockPanel)block.Parent;
            return (Slider)panel.Children[2];
        }

        private int _getIndexOfCamFromSetting(object sender)
        {
            try
            {
                Slider slider = (Slider)sender;
                DockPanel panel = (DockPanel)slider.Parent;
                if (panel != null)
                {
                    ListBoxItem item = (ListBoxItem)panel.Parent;
                    ListBox list = (ListBox)item.Parent;
                    Expander exp = (Expander)list.Parent;
                    ListBoxItem itemExp = (ListBoxItem)exp.Parent;
                    ListBox listExp = (ListBox)itemExp.Parent;

                    return listExp.Items.IndexOf(itemExp);
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        private TextBox? _getTextBoxOfCamSetting(object sender)
        {
            Slider slider = (Slider)sender;
            DockPanel panel = (DockPanel)slider.Parent;
            return panel == null ? null : (TextBox)panel.Children[1];
        }

        private void CamSpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                float newValue = Functions.CutFraction((float)e.NewValue, 2);
                cameras[_getIndexOfCamFromSetting(sender)].cameraSpeed = newValue;
                TextBox? box = _getTextBoxOfCamSetting(sender);
                if (box != null)
                {
                    box.Text = newValue.ToString();
                }
            }
            catch (Exception ex) { }
        }

        private void CamFovChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                float newValue = Functions.CutFraction((float)e.NewValue, 2);
                cameras[_getIndexOfCamFromSetting(sender)].cam.Fov = newValue;
                TextBox? box = _getTextBoxOfCamSetting(sender);
                if (box != null)
                {
                    box.Text = newValue.ToString();
                }
            }
            catch (Exception ex) { }
        }

        private void CamSensitivityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                float newValue = Functions.CutFraction((float)e.NewValue, 2);
                cameras[_getIndexOfCamFromSetting(sender)].sensitivity = newValue;
                TextBox? box = _getTextBoxOfCamSetting(sender);
                if (box != null)
                {
                    box.Text = newValue.ToString();
                }
            }
            catch (Exception ex) { }
        }

        // EVENTS FOR CAM SETTINGS END
        // ---------------------------



        // EVENTS FOR MESH SETTINGS START
        // ------------------------------

        private void MeshParamChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                float[] data = new float[3];
                TextBox box = (TextBox)sender;
                ListBoxItem item = (ListBoxItem)((DockPanel)box.Parent).Parent;
                ListBox list = (ListBox)item.Parent;

                bool[] showMesh = mesh.showMesh;
                bool isEnabled = mesh.isEnabled;

                int j = list.Items.IndexOf(item);
                if (j == 0)
                {
                    box.Text = ((int)float.Parse(box.Text.ToString())).ToString();
                }
                for (int i = 0; i < 3; i++)
                {
                    ListBoxItem iter = (ListBoxItem)list.Items[i];
                    TextBox value = (iter.Content as DockPanel).Children[1] as TextBox;
                    data[i] = float.Parse(value.Text.ToString());
                }
                mesh = new Mesh(size: (int)data[0], step: data[1], height: data[2]);
                mesh.showMesh = showMesh;
                mesh.isEnabled = isEnabled;
            }
            catch (Exception ex) { }
        }

        private void CheckBoxMeshSwitched(object sender, RoutedEventArgs e)
        {
            mesh.isEnabled = (bool)(sender as CheckBox).IsChecked;
        }

        private void MeshTurnOxy(object sender, RoutedEventArgs e)
        {
            mesh.showMesh[0] = (bool)(sender as CheckBox).IsChecked;
        }

        private void MeshTurnOyz(object sender, RoutedEventArgs e)
        {
            mesh.showMesh[1] = (bool)(sender as CheckBox).IsChecked;
        }

        private void MeshTurnOxz(object sender, RoutedEventArgs e)
        {
            mesh.showMesh[2] = (bool)(sender as CheckBox).IsChecked;
        }

        // EVENTS FOR MESH SETTINGS END
        // ----------------------------

        // DATA IMPORT-EXPORT START
        // ------------------------

        public void ExportData(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "JSON file (*.json)|*.json";
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            if (dialog.ShowDialog() == true) 
            {
                if (dialog.FileName.Length == 0)
                {
                    return;
                }
                FileStream file = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
                using (StreamWriter stream = new StreamWriter(file))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    if (currentProperty is ComplexPlaneTile)
                    {
                        ComplexPlaneTile export = (ComplexPlaneTile)currentProperty;
                        TileDataSet data = new TileDataSet(export.Xmesh, export.Ymesh, export.DataBuffer);
                        data.WriteStream(writer);

                    }
                    else if (currentProperty is ComplexPlaneTriangular)
                    {
                        ComplexPlaneTriangular export = (ComplexPlaneTriangular)currentProperty;
                        TriangularedDataSet? data = export.ExportPointDataSet();
                        if (data == null)
                        {
                            PointsDataSet pointsData = new(export.ExportPoints());
                            pointsData.WriteStream(writer);
                        }
                        else
                        {
                            data.WriteStream(writer);
                        }
                    }
                    else if(currentProperty is Section)
                    {
                        Section export = (Section)currentProperty;
                        export.ExportWriteStreamPolarArgs(writer);
                    }

                }
            }
            if (dialog.FileName.Length == 0)
            {
                return;
            }
            File.WriteAllText(dialog.FileName, sb.ToString());
            EventInform("Status: successfuly exported data to " + dialog.FileName);
        }

        public void ImportData(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                if(dialog.FileName.Length == 0)
                {
                    return;
                }
                string jsonString = File.ReadAllText(dialog.FileName);

                // add triangulated data supportion
                try
                {
                    TileDataSet? income = JsonConvert.DeserializeObject<TileDataSet>(jsonString);
                    if (income != null && income.X != null && income.Y != null && income.Z != null)
                    {
                        TileDataSet data = income;
                        addNewBottom(new ComplexPlaneTile(
                            textureSet: new string[] {
                                TexturePath.dark_paths,
                                TexturePath.cork_board,
                                TexturePath.criss_cross
                            },
                            X: data.X, Y: data.Y, Z: data.Z
                            ));
                        EventInform("Status: successfuly imported tiled mesh from " + dialog.FileName);
                    }
                    else
                    {
                        PointsDataSet? inpoints = JsonConvert.DeserializeObject<PointsDataSet>(jsonString);
                        if (inpoints != null && inpoints.Points != null)
                        {
                            PointsDataSet data = inpoints;
                            addNewPointsDataset(new ComplexPlaneTriangular(
                                inputData: data.ToVector3Set(),
                                shouldTriangulate: false,
                                textureSet: new string[] {
                                    TexturePath.dark_paths,
                                    TexturePath.criss_cross,
                                    TexturePath.cork_board
                                }));
                            EventInform("Status: successfuly imported points dataset from " + dialog.FileName);
                        }
                        else
                        {
                            TriangularedDataSet? inDataSet = JsonConvert.DeserializeObject<TriangularedDataSet>(jsonString);
                            if (inDataSet != null && inDataSet.Triangles != null)
                            {
                                TriangularedDataSet data = inDataSet;
                                addNewPointsDataset(new ComplexPlaneTriangular(
                                    data,
                                    textureSet: new string[] {
                                        TexturePath.dark_paths,
                                        TexturePath.criss_cross,
                                        TexturePath.cork_board
                                }));
                                EventInform("Status: successfuly imported triangulated dataset from " + dialog.FileName);
                            }
                            else
                            {
                                EventInform("Status: failed to import from " + dialog.FileName + "\n Either file or data format not supported");
                            }
                        }
                    }
                }
                catch (Exception ex) { }
            }
        }

        // DATA IMPORT-EXPORT END
        // ----------------------

        private void removeCurrentItem()
        {
            if (currentProperty is ComplexPlaneTile)
            {
                meshTiled.Value.Remove((ComplexPlaneTile)currentProperty);
            }
            else if (currentProperty is ComplexPlaneTriangular)
            {
                meshUneven.Value.Remove((ComplexPlaneTriangular)currentProperty);
            }
            else if (currentProperty is Section)
            {
                sections.Value.Remove((Section)currentProperty);
            }
            currentProperty = null;
            editorProperties.Items.Clear();

            doUpdate = true;
        }

        public void CreateWaterLevel(float height, float area = 100000)
        {
            float[] X = Functions.Arrange(-area, area, area);
            float[][] Z = Functions.FunctWaterLine(X, X, height);
            water = new ComplexPlaneTile(
                    X:X,
                    Y:X,
                    Z:Z,
                    textureSet: new string[] {
                                        TexturePath.dark_paths,
                                        TexturePath.dark_paths,
                                        TexturePath.dark_paths
                                        },
                    material: new(opacity: 0.01f)
                );
            water.Move(new Vector3(0, 0, 0));
        }

        private void addNewBottom(ComplexPlaneTile tiledBottom)
        {
            meshTiled.Value.Add(tiledBottom);
            doUpdate = true;
        }

        private void addNewSection(Section section)
        {
            sections.Value.Add(section);
            doUpdate = true;
        }

        private void addNewPointsDataset(ComplexPlaneTriangular item)
        {
            meshUneven.Value.Add(item);
            doUpdate = true;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            var key = e.Key;

            //if (key == Key.OemPlus)
            //{
            //    addCamera();
            //}
            //if (key == Key.NumPad9)
            //{
            //    cameraChange(1);
            //}
            //if (key == Key.NumPad3)
            //{
            //    cameraChange(-1);
            //}
            //if (key == Key.I)
            //{
            //    meshTiled.Value[0].Interp(0.9f, 1);
            //}
            //if (key == Key.K)
            //{
            //    meshTiled.Value[0].Interp(1.1f, 1);
            //}
            if (key == Key.Up)
            {
                cameraChange(1);
            }
            if (key == Key.Down)
            {
                cameraChange(-1);
            }
            //if (key == Key.Left)
            //{
            //    meshTiled.Value[0].MoveVisibleMesh(-1, 0, 0, 0, 0, 0);
            //}
            //if (key == Key.Right)
            //{
            //    meshTiled.Value[0].MoveVisibleMesh(1, 0, 0, 0, 0, 0);
            //}
        }
    }
}
