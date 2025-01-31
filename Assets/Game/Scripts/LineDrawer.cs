using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public float lineWidth = 0.2f; // Толщина линии
    public Material lineMaterial; // Материал линии

    private List<Vector3> points = new List<Vector3>(); // Храним точки пути
    private Mesh lineMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        // Создаём объект для линии
        GameObject lineObject = new GameObject("PlayerLine");
        lineObject.transform.SetParent(transform); // Привязываем к игроку

        // Добавляем компоненты Mesh
        meshFilter = lineObject.AddComponent<MeshFilter>();
        meshRenderer = lineObject.AddComponent<MeshRenderer>();
        meshRenderer.material = lineMaterial;

        lineMesh = new Mesh();
        meshFilter.mesh = lineMesh;
    }

    void Update()
    {
        Vector3 playerPos = transform.position; // Получаем позицию игрока
        playerPos.y = 0.01f; // Поднимаем линию, чтобы избежать Z-файтинга

        // Добавляем новую точку, если игрок двигается
        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], playerPos) > 0.1f)
        {
            points.Add(playerPos);
            UpdateMesh();
        }
    }

    void UpdateMesh()
    {
        if (points.Count < 2) return;

        Vector3[] vertices = new Vector3[points.Count * 2];
        int[] triangles = new int[(points.Count - 1) * 6];

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 left = points[i] + Vector3.left * lineWidth;
            Vector3 right = points[i] + Vector3.right * lineWidth;

            vertices[i * 2] = left;
            vertices[i * 2 + 1] = right;

            if (i < points.Count - 1)
            {
                int start = i * 2;
                triangles[i * 6] = start;
                triangles[i * 6 + 1] = start + 2;
                triangles[i * 6 + 2] = start + 1;

                triangles[i * 6 + 3] = start + 1;
                triangles[i * 6 + 4] = start + 2;
                triangles[i * 6 + 5] = start + 3;
            }
        }

        lineMesh.Clear();
        lineMesh.vertices = vertices;
        lineMesh.triangles = triangles;
        lineMesh.RecalculateNormals();
    }
}
