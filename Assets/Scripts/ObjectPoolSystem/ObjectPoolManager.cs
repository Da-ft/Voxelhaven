using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private bool addToDontDestroyOnLoad = false;

    private GameObject emptyHolder;

    private static GameObject particleSystemsEmpty;
    private static GameObject gameObjectsEmpty;
    private static GameObject soundFXEmpty;
    private static GameObject projectilesEmpty;
    private static GameObject collectiblesEmpty;

    private static Dictionary<GameObject, ObjectPool<GameObject>> objectPools;
    private static Dictionary<GameObject, GameObject> cloneToPrefabMap;

    public enum PoolType
    {
        ParticleSystems,
        GameObjects,
        SoundFX,
        Projectiles,
        Collectibles
    }

    private void Awake()
    {
        if (addToDontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

        SetupEmpties();
    }

    private void SetupEmpties()
    {
        emptyHolder = new GameObject("Object Pools");
        if (addToDontDestroyOnLoad) DontDestroyOnLoad(emptyHolder);

        particleSystemsEmpty = new GameObject("Particle Effects");
        particleSystemsEmpty.transform.SetParent(emptyHolder.transform);

        soundFXEmpty = new GameObject("Sound Effects");
        soundFXEmpty.transform.SetParent(emptyHolder.transform);

        gameObjectsEmpty = new GameObject("Enemy Gameobjects");
        gameObjectsEmpty.transform.SetParent(emptyHolder.transform);

        projectilesEmpty = new GameObject("Projectiles");
        projectilesEmpty.transform.SetParent(emptyHolder.transform);

        collectiblesEmpty = new GameObject("Collectibles");
        collectiblesEmpty.transform.SetParent(emptyHolder.transform);
    }

    private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, pos, rot, poolType),
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject
        );

        // WICHTIGER FIX: Den Pool auch im Dictionary speichern!
        objectPools.Add(prefab, pool);
    }

    private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType pooltype = PoolType.GameObjects)
    {
        bool wasActive = prefab.activeSelf;
        prefab.SetActive(false);
        GameObject obj = Instantiate(prefab, pos, rot);
        prefab.SetActive(wasActive);

        GameObject parentObject = SetParentObject(pooltype);
        obj.transform.SetParent(parentObject.transform);

        return obj;
    }

    private static void OnGetObject(GameObject obj)
    {
        // Wird beim Herausholen aus dem Pool aufgerufen
    }

    private static void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    private static void OnDestroyObject(GameObject obj)
    {
        if (cloneToPrefabMap.ContainsKey(obj))
        {
            cloneToPrefabMap.Remove(obj);
        }
        Destroy(obj);
    }

    private static GameObject SetParentObject(PoolType poolType)
    {
        return poolType switch
        {
            PoolType.ParticleSystems => particleSystemsEmpty,
            PoolType.GameObjects => gameObjectsEmpty,
            PoolType.SoundFX => soundFXEmpty,
            PoolType.Projectiles => projectilesEmpty,
            PoolType.Collectibles => collectiblesEmpty,
            _ => null,
        };
    }

    private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType pooltype = PoolType.GameObjects) where T : Object
    {
        if (!objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, spawnPos, spawnRotation, pooltype);
        }

        GameObject obj = objectPools[objectToSpawn].Get();

        if (obj != null)
        {
            if (!cloneToPrefabMap.ContainsKey(obj))
            {
                cloneToPrefabMap.Add(obj, objectToSpawn);
            }

            obj.transform.position = spawnPos;
            obj.transform.rotation = spawnRotation;
            obj.SetActive(true);

            if (typeof(T) == typeof(GameObject)) return obj as T;

            T component = obj.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"Object {objectToSpawn.name} doesnt have component of type {typeof(T)}");
                return null;
            }
            return component;
        }
        return null;
    }

    public static T SpawnObjects<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
    }

    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if (cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObject = SetParentObject(poolType);

            if (obj.transform.parent != parentObject.transform)
            {
                obj.transform.SetParent(parentObject.transform);
            }

            if (objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning($"Trying to return an object that is not pooled: {obj.name}. Destroying instead.");
            Destroy(obj); // Fallback!
        }
    }
}