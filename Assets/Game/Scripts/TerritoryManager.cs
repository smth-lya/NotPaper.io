using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerritoryManager : MonoBehaviour
{
    [SerializeField] private ComputeShader _mergeShader;

    private List<Vector3> _territoryPoints = new();

    public void ExpandTerritory(List<Vector3> trailPoints)
    {
        if (trailPoints.Count < 3) return;

        var polygonBuffer = new ComputeBuffer(_territoryPoints.Count, sizeof(float) * 2);
        var trailBuffer = new ComputeBuffer(trailPoints.Count, sizeof(float) * 2);

        polygonBuffer.SetData(_territoryPoints);
        trailBuffer.SetData(trailPoints);

        _mergeShader.SetBuffer(0, "Polygon", polygonBuffer);
        _mergeShader.SetBuffer(0, "Trail", trailBuffer);
        _mergeShader.Dispatch(0, 1, 1, 1);

        Vector3[] updatedTerritory = new Vector3[_territoryPoints.Count];
        polygonBuffer.GetData(updatedTerritory);
        _territoryPoints = updatedTerritory.ToList();

        polygonBuffer.Release();
        trailBuffer.Release();
    }
}