using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private static Type[] supportedTypes = new Type[]
        {
            typeof(ComplexPlaneTile),
            typeof(Section),
            typeof(ComplexPlaneTriangular)
        };

        // change to the right type
        public static object? LookUpProperties(object itemWithProperties)
        {
            int index = -1;

            if (itemWithProperties is ComplexPlaneTile)
                index = 0;
            else if (itemWithProperties is Section)
                index = 1;
            else if (itemWithProperties is ComplexPlaneTriangular)
                index = 2;

            switch (index)
            {
                case 0:
                    {

                    }; break;
                case 1:
                    {

                    }; break;
                case 2:
                    {

                    }; break;
            }


            return null;
        }

    }

    public class EditorSettings
    {
        //make it private later
        public Lazy<List<TreeViewItem>> objectsWithProperties = new Lazy<List<TreeViewItem>>();

        protected bool isEnabled = false;
        public bool doUpdate = false;

        public Lazy<List<ComplexPlaneTile>> meshTiled;
        public Lazy<List<ComplexPlaneTriangular>> meshUneven;
        public Lazy<List<Section>> sections;

        public Mesh mesh;
        public Axis axis;

        public CameraControl[] cameras;
        public int activeCam;

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
                        ObjectPropertySettings.LookUpProperties(meshTiled.Value.ElementAt(num));
                    }; break;
                case 1:
                    {
                        ObjectPropertySettings.LookUpProperties(sections.Value.ElementAt(num));
                    }; break;
                case 2:
                    {
                        ObjectPropertySettings.LookUpProperties(meshUneven.Value.ElementAt(num));
                    }; break;
            }



            // add properties

            Console.WriteLine("I\'m selected");
        }
    }
}
