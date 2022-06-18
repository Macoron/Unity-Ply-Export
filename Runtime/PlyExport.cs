using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityPly
{
    public static class PlyExport
    {
        #region Public Functions
        
        /// <summary>
        /// Serialize pointcloud data in .ply format.
        /// </summary>
        /// <param name="points">Points world coordinates.</param>
        /// <param name="colors">
        /// Optional color for each point. Should be same length as <see cref="points"/> array.
        /// </param>
        /// <returns>String representing pointcloud data in .ply format. Null if error.</returns>
        public static string ToPlyPointcloud(Vector3[] points, Color32[] colors = null)
        {
            // sanity checks
            if (points == null)
            {
                Debug.LogError("Ply export error: Points array can't be null!");
                return null;
            }
            if (colors != null && points.Length != colors.Length)
            {
                Debug.LogError("Colors array should be same length as points array!");
                return null;
            }

            return ToPlyPointcloudInternal(points, colors);
        }

        /// <summary>
        /// Serialize and combine <see cref="MeshFilter"/> meshes into one .ply model.
        /// </summary>
        /// <remarks>All information about UV, materials and vertex normals will be lost.</remarks>
        /// <param name="meshFilters">
        /// Array of <see cref="MeshFilter"/> to serialize. All null values or filters with invalid mesh will be ignored.
        /// </param>
        /// <param name="useWorldPosition">If true will use world position of 3d models.</param>
        /// <returns>String representing combined mesh collection in .ply format. Null if error.</returns>
        public static string ToPly(MeshFilter[] meshFilters, bool useWorldPosition = true)
        {
            // sanity checks
            if (meshFilters == null)
            {
                Debug.LogError("Ply export error: MeshFilters array can't be null!");
                return null;
            }

            var meshes = meshFilters.Select(mf => mf.sharedMesh).ToArray();
            var transforms = useWorldPosition ? meshFilters.Select(mf => mf.transform).ToArray() : null;
            return ToPly(meshes, transforms);
        }

        /// <summary>
        /// Serialize <see cref="MeshFilter"/> mesh into .ply model.
        /// </summary>
        /// <remarks>All information about UV, materials and vertex normals will be lost.</remarks>
        /// <param name="meshFilter">Mesh filter to serialize.</param>
        /// <param name="useWorldPosition">If true will use world position of 3d models.</param>
        /// <returns>String representing mesh in .ply format. Null if error.</returns>
        public static string ToPly(MeshFilter meshFilter, bool useWorldPosition = true)
        {
            // sanity checks
            if (meshFilter == null)
            {
                Debug.LogError("Ply export error: MeshFilter can't be null!");
                return null;
            }
            if (!meshFilter.sharedMesh)
            {
                Debug.LogError($"Ply export error: MeshFilter {meshFilter.name} mesh can't be null!");
                return null;
            }

            var t = useWorldPosition ? meshFilter.transform : null;
            return ToPly(meshFilter.sharedMesh, t);
        }

        /// <summary>
        /// Serialize single mesh into .ply model.
        /// </summary>
        /// <remarks>All information about UV, materials and vertex normals will be lost.</remarks>
        /// <param name="mesh">Mesh to serialize in .ply.</param>
        /// <param name="transform">Optional global transform for mesh.</param>
        /// <returns>String representing mesh in .ply format. Null if error.</returns>
        public static string ToPly(Mesh mesh, Transform transform = null)
        {
            // sanity checks
            if (!mesh)
            {
                Debug.LogError("Ply export error: Mesh can't be null!");
                return null;
            }

            var meshes = new[] { mesh };
            var transforms = transform ? new[] { transform } : null;
            return ToPly(meshes, transforms);
        }
        
        /// <summary>
        /// Serialize and combine meshes into one .ply model.
        /// </summary>
        /// <remarks>All information about UV, materials and vertex normals will be lost.</remarks>
        /// <param name="meshes">
        /// Array of meshes to serialize. All null or invalid meshes will be ignored;
        /// </param>
        /// <param name="transforms">
        /// Optional array of global transforms for each mesh. Should be same length as <see cref="meshes"/>.
        /// </param>
        /// <returns>String representing combined mesh collection in .ply format. Null if error.</returns>
        public static string ToPly(Mesh[] meshes, Transform[] transforms = null)
        {
            // sanity checks
            if (meshes == null)
            {
                Debug.LogError("Ply export error: Meshes array can't be null!");
                return null;
            }
            if (transforms != null && meshes.Length != transforms.Length)
            {
                Debug.LogError("Ply export error: Transforms array should be same length as meshes array!");
                return null;
            }

            return ToPlyInternal(meshes, transforms);
        }
        
        #endregion // Ply Export

        #region Internal
        private static string ToPlyPointcloudInternal(Vector3[] points, Color32[] colors)
        {
            // prepare all data
            var data = new StringBuilder();
            AddVertexes(data, points, colors);
            
            // make a file
            var dataStr = data.ToString();
            return CompilePlyFile(dataStr, points.Length, 0);
        }
        
        private static string ToPlyInternal(Mesh[] meshes, Transform[] transforms)
        {
            // prepare all data
            int totalVertCount = 0, totalFacesCount = 0;
            var verts = new StringBuilder();
            var faces = new StringBuilder(); 
            for (int i = 0; i < meshes.Length; i++)
            {
                // here we gently skip invalid or null meshes
                if (!meshes[i])
                    continue;
                
                totalFacesCount += AddTriangles(faces, meshes[i], totalVertCount);
                totalVertCount += AddVertexes(verts, meshes[i], transforms?[i]);
            }

            var dataStr = verts.ToString() + faces.ToString();
            return CompilePlyFile(dataStr, totalVertCount, totalFacesCount);
        }
        
        // Get all vertexes from mesh and append them to string builder
        // Returns count of written vertexes
        private static int AddVertexes(StringBuilder str, Mesh m, Transform t = null)
        {
            var vertexes = m.vertices;
            var colors = m.colors32.Length == vertexes.Length ? m.colors32 : null;
            return AddVertexes(str, vertexes, colors, t);
        }

        // Append all vertexes in array to string builder
        // Moves to right handed coordinates system by inverting Z axis
        // Returns count of written vertexes
        private static int AddVertexes(StringBuilder str, Vector3[] vert, Color32[] colors = null, Transform t = null)
        {
            for (var i = 0; i < vert.Length; i++)
            {
                var pos = t ? t.TransformPoint(vert[i]) : vert[i];
                var posStr = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", pos.x, pos.y, -pos.z);
                
                var color = colors != null ? colors[i] : (Color32) Color.white;
                var colorStr = $" {color.r} {color.g} {color.b}";
                
                str.AppendLine(posStr + colorStr);
            }

            return vert.Length;
        }

        // Append all triangles in mesh to string builder as faces
        // Invert face normal by writing it in reverse order
        // Returns count of written faces
        private static int AddTriangles(StringBuilder str, Mesh m, int vertIndexMargin = 0)
        {
            for (var i = 0; i < m.triangles.Length; i+=3)
            {
                str.AppendLine($"3 {m.triangles[i+2] + vertIndexMargin}" +
                           $" {m.triangles[i+1] + vertIndexMargin}" +
                           $" {m.triangles[i] + vertIndexMargin}");
            }

            return m.triangles.Length / 3;
        }
        
        private const string PlyHeader =
            "ply\n" +
            "format ascii 1.0\n" +
            "element vertex {0}\n" +
            "property float32 x\n" +
            "property float32 y\n" +
            "property float32 z\n" +
            "property uchar red\n" +
            "property uchar green\n" +
            "property uchar blue\n" +
            "element face {1}\n" +
            "property list uint8 int32 vertex_index\n" +
            "end_header\n";

        private static string GeneratePlyHeader(int vertCount, int facesCount)
        {
            return string.Format(PlyHeader, vertCount, facesCount);
        }
        
        private static string CompilePlyFile(string data, int vertCount, int facesCount)
        {
            var fileBuilder = new StringBuilder();
            var header = GeneratePlyHeader(vertCount, facesCount);
            fileBuilder.Append(header);
            fileBuilder.Append(data);
            return fileBuilder.ToString();
        }
        
        #endregion // Internal
    }
}

