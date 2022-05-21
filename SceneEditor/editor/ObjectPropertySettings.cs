﻿using LearnOpenTK.Common;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SceneEditor.editor
{
    public enum StructuresNames
    {
        TiledMeshes,
        Sections,
        UnevenMesh
    }

    public static class ObjectPropertySettings
    {
        // length is Enum.GetNames(typeof(StructuresNames)).Length
        public static string[] objectsNames = new string[]
        {
            "Mesh tiled",
            "Sections",
            "Mesh uneven"
        };

        // change to the right type

    }

    public class EditorSettings
    {
        protected Size selfSize;

        //make it private later
        public Lazy<List<TreeViewItem>> objectsWithProperties = new Lazy<List<TreeViewItem>>();

        public ListBox elementProperties;
        public ListBox editorProperties;
        private object currentProperty;

        protected bool isEnabled = false;
        public bool doUpdate = false;
        public bool doInvalidate = false;

        public Lazy<List<ComplexPlaneTile>> meshTiled;
        public Lazy<List<ComplexPlaneTriangular>> meshUneven;
        public Lazy<List<Section>> sections;

        public Mesh mesh;
        public Axis axis;

        public CameraControl[] cameras;
        public int activeCam;

        public Shader shader;
        protected Matrix4 view;

        public void addDependencyBoxes(ListBox? elementProperties, ListBox? editorProperties)
        {
            this.elementProperties = elementProperties;
            this.editorProperties = editorProperties;


            //EditorChanged();
        }

        private void _putExistingInTree(TreeView treeView, int amount, int indexName)
        {
            if (amount != 0)
            {
                string header = ObjectPropertySettings.objectsNames[indexName];
                treeView.Items.Add(_addNewTreeMember(header));
                TreeViewItem view = (TreeViewItem)treeView.Items.GetItemAt(treeView.Items.Count - 1);
                for (int i = 0; i < amount; i++)
                {
                    TreeViewItem item = _addNewTreeMember(header.ToLower() + " " + (i + 1));
                    view.Items.Add(item);
                    item.Selected += ObjectSelected;
                    objectsWithProperties.Value.Add(item);
                }
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
                index < ObjectPropertySettings.objectsNames.Length &&
                ObjectPropertySettings.objectsNames[index].CompareTo(parent.Header.ToString()) != 0;
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

                    }; break;
            }

            CheckBox enabled = CreateCheckBoxSetting(
                            "Enable",
                            isEnabled,
                            SwitchObjectDrawingEnabled
                            );
            list.Items.Add(enabled);
            return null;
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
        }

        private void SectionSwitchAreaDisplay(object sender, RoutedEventArgs e)
        {
            bool state = (bool)(sender as CheckBox).IsChecked;
            (currentProperty as Section).showAreaMesh = state;
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

        }

        private void SectionSwitchPolarDisplay(object sender, RoutedEventArgs e)
        {
            bool state = (bool)(sender as CheckBox).IsChecked;
            (currentProperty as Section).showPolar = state;
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
        }

        // scene settings
        public void EditorChanged()
        {
            editorProperties.Items.Clear();
            // cameras
            Expander exp = Generate.Expander("Cameras");
            exp.HorizontalContentAlignment = HorizontalAlignment.Stretch;
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
            exp.Content = list;
            editorProperties.Items.Add(exp);

            exp = Generate.Expander("Mesh");
            exp.Content = CreateMeshSettings();
            editorProperties.Items.Add(exp);

            editorProperties.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        // isNeeded
        public void addCamera()
        {
            _addCamera();
            EditorChanged();
        }

        public void _addCamera(bool isOrthogonalPerspective = false)
        {
            if (cameras.Length < 10)
            {
                List<CameraControl> buffer = cameras.ToList();
                buffer.Add(new CameraControl(SizeToVector2i(selfSize)));

                activeCam = cameras.Length;
                cameras = buffer.ToArray();
                cameras[activeCam].cam.Fov = 90f;

                view = cameras[activeCam].cam.GetViewMatrix();
                shader.SetMatrix4("view", view);
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
            view = cameras[activeCam].cam.GetViewMatrix();
            shader.SetMatrix4("view", cameras[activeCam].cam.GetViewMatrix());
        }

        private protected Vector2i SizeToVector2i(Size size)
        {
            return new Vector2i { X = (int)size.Width, Y = (int)size.Height };
        }

        private ListBox CreateMeshSettings()
        {
            ListBox listSet = new ListBox();

            listSet.Items.Add(CreateItemWithValueChangeable(
                "Size:",
                mesh.size,
                MeshParamChanged
                ));
            listSet.Items.Add(CreateItemWithValueChangeable(
                "Step:",
                mesh.step,
                MeshParamChanged
                ));
            listSet.Items.Add(CreateItemWithValueChangeable(
                "Line width:",
                mesh.width,
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

            listSet.Items.Add(Generate.ListBoxItem(CreateCheckBoxSetting(
                "Enable",
                mesh.isEnabled,
                CheckBoxSwitched
                )));

            listSet.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            return listSet;
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

        private ListBox GetListWithVector3Settings(object sender)
        {
            return (ListBox)((ListBoxItem)((DockPanel)((ListBox)((ListBoxItem)((DockPanel)((TextBox)sender).Parent).Parent).Parent).Parent).Parent).Parent;
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
                mesh = new Mesh(size: (int)data[0], step: data[1], width: data[2]);
                mesh.showMesh = showMesh;
                mesh.isEnabled = isEnabled;
            }
            catch (Exception ex) { }
        }

        private void CheckBoxSwitched(object sender, RoutedEventArgs e)
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

        public void OnKeyDown(KeyEventArgs e)
        {
            var key = e.Key;

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
            if (key == Key.F)
            {
                cameras[activeCam].grabedMouse = !cameras[activeCam].grabedMouse;
            }
            if (key == Key.I)
            {
                meshTiled.Value[0].Interp(0.9f, 1);
            }
            if (key == Key.K)
            {
                meshTiled.Value[0].Interp(1.1f, 1);
            }
            if (key == Key.PageUp)
            {
                meshTiled.Value[0].MoveVisibleMesh(0, 0, 0, 0, 1, 1);
            }
            if (key == Key.PageDown)
            {
                meshTiled.Value[0].MoveVisibleMesh(0, 0, 0, 0, -1, -1);
            }
            if (key == Key.Up)
            {
                meshTiled.Value[0].MoveVisibleMesh(0, 1, 0, 0, 0, 0);
            }
            if (key == Key.Down)
            {
                meshTiled.Value[0].MoveVisibleMesh(0, -1, 0, 0, 0, 0);
            }
            if (key == Key.Left)
            {
                meshTiled.Value[0].MoveVisibleMesh(-1, 0, 0, 0, 0, 0);
            }
            if (key == Key.Right)
            {
                meshTiled.Value[0].MoveVisibleMesh(1, 0, 0, 0, 0, 0);
            }
            if (key == Key.J)
            {
                // test intersection
                sections.Value[0].IntersectTiled(
                    meshTiled.Value[0].Xmesh,
                    meshTiled.Value[0].Ymesh,
                    meshTiled.Value[0].DataBuffer);
                sections.Value[0].CountPolarFunction();
            }
            if (key == Key.O)
            {
                meshTiled.Value[0].SwitchDrawStyle();
            }
        }
    }
}
