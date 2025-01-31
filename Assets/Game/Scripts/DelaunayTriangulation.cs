//using System.Collections.Generic;
//using TriangleNet.Geometry;
//using TriangleNet.Meshing;
//using UnityEngine.UIElements;

//public static class DelaunayTriangulation
//{
//    public static int[] Triangulate(List<Vector2> points)
//    {
//        Polygon polygon = new Polygon();
//        foreach (var p in points) polygon.Add(new Vertex(p.x, p.y));

//        var mesh = polygon.Triangulate();
//        List<int> indices = new List<int>();
//        foreach (var tri in mesh.Triangles)
//        {
//            indices.Add(tri.GetVertexID(0));
//            indices.Add(tri.GetVertexID(1));
//            indices.Add(tri.GetVertexID(2));
//        }
//        return indices.ToArray();
//    }
//}
