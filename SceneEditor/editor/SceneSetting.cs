using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    internal class SceneSetting
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public event EventHandler ValueChanged;


    }
}
