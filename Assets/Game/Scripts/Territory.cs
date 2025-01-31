using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Territory : MonoBehaviour
{
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;

    [SerializeField] private float _baseRadius = 2.5f; // Радиус стартовой территории
    [SerializeField] private int _baseSegments = 16; // Количество сегментов круга
    [SerializeField] private Material _territoryMaterial; // Материал территории

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshCollider = GetComponent<MeshCollider>();

        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;

        if (_territoryMaterial != null)
        {
            _meshRenderer.material = _territoryMaterial;
        }

        GenerateBaseTerritory();
    }

    private void GenerateBaseTerritory()
    {
        List<Vector3> baseVertices = new List<Vector3> { Vector3.zero }; // Центр круга
        List<int> triangles = new List<int>();

        // Создаём точки по окружности
        for (int i = 0; i < _baseSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / _baseSegments;
            float x = Mathf.Cos(angle) * _baseRadius;
            float z = Mathf.Sin(angle) * _baseRadius;
            baseVertices.Add(new Vector3(x, 0, z));
        }

        // Создаём треугольники
        for (int i = 1; i < _baseSegments; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }
        triangles.Add(0);
        triangles.Add(_baseSegments - 1);
        triangles.Add(1); // Замыкающий треугольник

        // Применяем данные к мешу
        _mesh.Clear();
        _mesh.vertices = baseVertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.RecalculateNormals();

        // Обновляем коллайдер с новой сеткой
        _meshCollider.sharedMesh = _mesh;
    }

    public void CaptureArea(List<Vector3> trailPoints)
    {
        List<Vector3> territoryPoints = new List<Vector3>(trailPoints);
        int[] triangles = Triangulator3D.Triangulate(trailPoints);

        // Применяем новые вершины и треугольники
        _mesh.Clear();
        _mesh.vertices = territoryPoints.ToArray();
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();

        // Обновляем коллайдер с новой сеткой
        _meshCollider.sharedMesh = _mesh;
    }
}
