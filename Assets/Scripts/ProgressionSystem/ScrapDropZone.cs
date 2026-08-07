using System.Collections.Generic;
using UnityEngine;

public class ScrapDropZone : MonoBehaviour
{
    public static List<ScrapDropZone> AllZones { get; private set; } = new List<ScrapDropZone>();

    private void OnEnable()
    {
        if (!AllZones.Contains(this))
        {
            AllZones.Add(this);
        }
    }

    private void OnDisable()
    {
        AllZones.Remove(this);
    }

    public static ScrapDropZone GetClosestZone(Vector3 position)
    {
        if (AllZones.Count == 0) return null;

        ScrapDropZone closest = null;
        float minSqrDistance = float.MaxValue;

        foreach (var zone in AllZones)
        {
            float sqrDist = (zone.transform.position - position).sqrMagnitude;
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                closest = zone;
            }
        }

        return closest;
    }
}