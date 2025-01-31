using System.Collections.Generic;
using UnityEngine;

public class TrailDrawer : MonoBehaviour
{
    private List<Vector3> _trailPoints = new List<Vector3>();
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private LayerMask _territoryLayer; // Слой территории для проверки

    void Start()
    {
        _lineRenderer.positionCount = 0;
    }

    public void AddPoint(Vector3 position)
    {
        // Если точка слишком близка к предыдущей, не добавляем её
        if (_trailPoints.Count > 0 && Vector3.Distance(_trailPoints[^1], position) < 0.1f) return;

        // Проверяем, находится ли точка на территории
        if (IsPointInTerritory(position))
        {
            return; // Не рисуем след, если точка находится в пределах территории
        }

        // Добавляем точку в след
        _trailPoints.Add(position);
        _lineRenderer.positionCount = _trailPoints.Count;
        _lineRenderer.SetPosition(_trailPoints.Count - 1, position);
    }

    public List<Vector3> GetTrail()
    {
        return new List<Vector3>(_trailPoints); // Возвращаем копию списка
    }

    public void ClearTrail()
    {
        _trailPoints.Clear();
        _lineRenderer.positionCount = 0;
    }

    private bool IsPointInTerritory(Vector3 point)
    {
        // Проверяем, находится ли точка в пределах территории
        // Для этого используем тот же метод, что и в PlayerMovement
        return Physics.CheckSphere(point, 0.5f, _territoryLayer);
    }
}
