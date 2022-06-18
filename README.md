# Unity Ply Export
Export ASCII [.ply files](https://en.wikipedia.org/wiki/PLY_(file_format)) from Unity3d editor or runtime. 

Supports both pointclouds and polygonal meshes.

## Setup
You can install this package using **Package Manager**. Add it by this git URL:
```
https://github.com/Macoron/Unity-Ply-Export.git
```

Alternatively just clone this repo and place files somewhere in your Assets folder.

## Usages
Export mesh model into .ply file:
```csharp
var meshFilter = GetComponent<MeshFilter>();
var ply = PlyExport.ToPly(meshFilter);
File.WriteAllText("mesh.ply", ply);
```

Export pointcloud into .ply file:
```csharp
var points = new List<Vector3>();
for (int i = 0; i < 1000; i++)
    points.Add(Random.insideUnitSphere);

var ply = PlyExport.ToPlyPointcloud(points.ToArray());
File.WriteAllText("pointcloud.ply", ply);
```

You can also export selected meshes in Editor by clicking menu `Assets => Export selected to .ply`.

## Limitations
Keep in mind that not all data is supported for export:
- [x] Colored pointclouds
- [x] Polygonal mesh
- [x] Mesh vertex colors
- [ ] Mesh vertex normals
- [ ] Mesh materials
- [ ] Mesh UV

## License
This project is licensed under the MIT License.
