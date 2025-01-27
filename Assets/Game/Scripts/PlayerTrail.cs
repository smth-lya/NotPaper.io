using System.Collections.Generic;
using UnityEngine;
public class PlayerTrail : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;

    private List<Vector3> _points = new();
    private bool _isDrawing;

    public void StartTrail(Vector3 startPosition)
    {
        _points.Clear();
        _points.Add(startPosition);

        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPosition(0, startPosition);

        _isDrawing = true;
    }

    public void AddPoint(Vector3 point)
    {
        if (!_isDrawing) return;

        _points.Add(point);
        
        _lineRenderer.positionCount = _points.Count;
        _lineRenderer.SetPosition(_points.Count - 1, point);
    }

    public void StopTrail()
    {
        _isDrawing = false;
    }

    public IReadOnlyList<Vector3> GetPoints() => _points;
}