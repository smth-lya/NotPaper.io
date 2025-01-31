using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    [SerializeField] private LayerMask _trailLayer; // Слой для следов
    [SerializeField] private LayerMask _territoryLayer; // Слой для территории

    public bool CheckTrailCollision(Vector3 position)
    {
        // Проверяем столкновение с любым следом
        Collider[] colliders = Physics.OverlapSphere(position, 0.5f, _trailLayer);
        return colliders.Length > 0;
    }

    public bool CheckTerritoryCollision(Vector3 position)
    {
        // Проверяем столкновение с территорией
        Collider[] colliders = Physics.OverlapSphere(position, 0.5f, _territoryLayer);
        Debug.Log(colliders.Length);
        foreach (Collider collider in colliders)
            Debug.Log(collider.name);

        return colliders.Length > 0;
    }
}
