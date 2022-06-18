using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityPly.Editor
{
    public static class PlyExportEditor
    {
        [MenuItem("Assets/Export selected to .ply", priority = 10000)]
        private static void ExportPly()
        {
            var selection = Selection.transforms;
            if (selection.Length == 0)
                return;
            
            var meshFilters = selection.Select(t => t.GetComponent<MeshFilter>())
                .Where(mf => mf != null)
                .ToArray();
            if (meshFilters.Length == 0)
                return;
            
            var path = EditorUtility.SaveFilePanel("Save as PLY", "",
                Selection.activeGameObject.name + ".ply", "ply");
            if (string.IsNullOrEmpty(path))
                return;
            
            var ply = PlyExport.ToPly(meshFilters);
            File.WriteAllText(path, ply);
            
        }
        
        [MenuItem("Assets/Export selected to .ply", true)]
        private static bool ValidateExportPly()
        {
            return Selection.activeGameObject;
        }
    }
}