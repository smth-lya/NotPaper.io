using System.Collections.Generic;
using UnityEngine;

public static class Triangulator
{
    public static int[] Triangulate(List<Vector2> points)
    {
        List<int> indices = new List<int>();
        int n = points.Count;
        if (n < 3) return indices.ToArray();

        int[] V = new int[n];
        for (int i = 0; i < n; i++) V[i] = i;

        int count = 2 * n;
        int m = 0;
        for (int v = n - 1; n > 2;)
        {
            if ((count--) <= 0) break; // Защита от бесконечного цикла

            int u = v; if (n <= u) u = 0;
            v = u + 1; if (n <= v) v = 0;
            int w = v + 1; if (n <= w) w = 0;

            if (IsEar(u, v, w, points, V))
            {
                indices.Add(V[u]);
                indices.Add(V[v]);
                indices.Add(V[w]);

                m++;
                for (int s = v, t = v + 1; t < n; s++, t++) V[s] = V[t];
                n--;
                count = 2 * n;
            }
        }
        return indices.ToArray();
    }

    private static bool IsEar(int u, int v, int w, List<Vector2> points, int[] V)
    {
        Vector2 A = points[V[u]], B = points[V[v]], C = points[V[w]];
        if (Vector2.Dot(C - A, B - A) <= 0) return false; // Проверка на выпуклость

        for (int p = 0; p < points.Count; p++)
        {
            if (p == V[u] || p == V[v] || p == V[w]) continue;
            if (PointInTriangle(points[p], A, B, C)) return false;
        }
        return true;
    }

    private static bool PointInTriangle(Vector2 P, Vector2 A, Vector2 B, Vector2 C)
    {
        float dX = P.x - C.x;
        float dY = P.y - C.y;
        float dX21 = C.x - B.x;
        float dY12 = B.y - C.y;
        float D = dY12 * (A.x - C.x) + dX21 * (A.y - C.y);
        float s = dY12 * dX + dX21 * dY;
        float t = (C.y - A.y) * dX + (A.x - C.x) * dY;
        if (D < 0) return s <= 0 && t <= 0 && s + t >= D;
        return s >= 0 && t >= 0 && s + t <= D;
    }
}


public static class Triangulator3D
{
    public static int[] Triangulate(List<Vector3> points)
    {
        List<int> indices = new List<int>();
        int n = points.Count;
        if (n < 3) return indices.ToArray();

        int[] V = new int[n];
        for (int i = 0; i < n; i++) V[i] = i;

        int count = 2 * n;
        int m = 0;
        for (int v = n - 1; n > 2;)
        {
            if ((count--) <= 0) break; // Защита от бесконечного цикла

            int u = v; if (n <= u) u = 0;
            v = u + 1; if (n <= v) v = 0;
            int w = v + 1; if (n <= w) w = 0;

            if (IsEar(u, v, w, points, V))
            {
                indices.Add(V[u]);
                indices.Add(V[v]);
                indices.Add(V[w]);

                m++;
                for (int s = v, t = v + 1; t < n; s++, t++) V[s] = V[t];
                n--;
                count = 2 * n;
            }
        }
        return indices.ToArray();
    }

    private static bool IsEar(int u, int v, int w, List<Vector3> points, int[] V)
    {
        Vector3 A = points[V[u]];
        Vector3 B = points[V[v]];
        Vector3 C = points[V[w]];

        // Используем XZ-координаты
        Vector2 A2D = new Vector2(A.x, A.z);
        Vector2 B2D = new Vector2(B.x, B.z);
        Vector2 C2D = new Vector2(C.x, C.z);

        if (Vector2.Dot(C2D - A2D, B2D - A2D) <= 0) return false; // Проверка на выпуклость

        for (int p = 0; p < points.Count; p++)
        {
            if (p == V[u] || p == V[v] || p == V[w]) continue;

            Vector2 P2D = new Vector2(points[p].x, points[p].z);
            if (PointInTriangle(P2D, A2D, B2D, C2D)) return false;
        }
        return true;
    }

    private static bool PointInTriangle(Vector2 P, Vector2 A, Vector2 B, Vector2 C)
    {
        float dX = P.x - C.x;
        float dY = P.y - C.y;
        float dX21 = C.x - B.x;
        float dY12 = B.y - C.y;
        float D = dY12 * (A.x - C.x) + dX21 * (A.y - C.y);
        float s = dY12 * dX + dX21 * dY;
        float t = (C.y - A.y) * dX + (A.x - C.x) * dY;
        if (D < 0) return s <= 0 && t <= 0 && s + t >= D;
        return s >= 0 && t >= 0 && s + t <= D;
    }
}
