using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    // Ein separater Pool pro Prefab-Referenz, da unterschiedliche Enemy-Typen unterschiedliche Projektile (Optik/Geschwindigkeit) verwenden
    private readonly Dictionary<GameObject, Queue<Projectile>> pools = new Dictionary<GameObject, Queue<Projectile>>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Projectile Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        Queue<Projectile> queue = GetOrCreateQueue(prefab);

        Projectile projectile;
        if (queue.Count > 0)
        {
            projectile = queue.Dequeue();
            projectile.transform.SetPositionAndRotation(position, rotation);
            projectile.gameObject.SetActive(true);
        }
        else
        {
            GameObject instance = Instantiate(prefab, position, rotation, transform);
            projectile = instance.GetComponent<Projectile>();

            if (projectile == null)
                Debug.LogError($"[ProjectilePool] Prefab '{prefab.name}' hat keine Projectile-Komponente.");
        }

        projectile.SetOriginatingPrefab(prefab);
        return projectile;
    }

    public void Return(GameObject prefab, Projectile projectile)
    {
        if (prefab == null || projectile == null) return;

        projectile.gameObject.SetActive(false);
        GetOrCreateQueue(prefab).Enqueue(projectile);
    }

    private Queue<Projectile> GetOrCreateQueue(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Queue<Projectile> queue))
        {
            queue = new Queue<Projectile>();
            pools[prefab] = queue;
        }

        return queue;
    }
}
