using UnityEngine;
using System.Collections.Generic;

public class TerritoryManager : MonoBehaviour
{
    private Dictionary<int, List<Vector3>> playerTerritories = new();

    public void ExpandTerritory(List<Vector3> newTrail, int playerID)
    {
        if (newTrail.Count < 3) return;
        if (!playerTerritories.ContainsKey(playerID))
            playerTerritories[playerID] = new List<Vector3>();

        playerTerritories[playerID].AddRange(newTrail);
    }

    public bool IsPointInsideTerritory(Vector3 point, int playerID)
    {
        if (!playerTerritories.ContainsKey(playerID)) return false;
        int crossings = 0;
        List<Vector3> territoryPoints = playerTerritories[playerID];

        for (int i = 0; i < territoryPoints.Count; i++)
        {
            Vector3 a = territoryPoints[i];
            Vector3 b = territoryPoints[(i + 1) % territoryPoints.Count];
            if (((a.z > point.z) != (b.z > point.z)) &&
                (point.x < (b.x - a.x) * (point.z - a.z) / (b.z - a.z) + a.x))
            {
                crossings++;
            }
        }
        return (crossings % 2) != 0;
    }
}
