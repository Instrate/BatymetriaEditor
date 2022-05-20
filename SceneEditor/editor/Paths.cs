using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
