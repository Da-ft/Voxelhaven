using UnityEngine;

[CreateAssetMenu(fileName = "EnemyThiefProfile", menuName = "ScriptableObjects/Enemy Thief Profile")]
public class EnemyThiefProfile : EnemyProfileSO
{
    [Header("Thief Specific Settings")]
    public int maxScrapCapacity = 5;
    public float stealDuration = 1.5f;

    public override void ExecuteAttack(EnemyBrain enemy)
    {
        // Cannot attack player
    }

    public override void ExecuteDeath(EnemyBrain enemy)
    {
        // Drop scrap on death for recollection
        if (dropPrefab != null && enemy.CarriedScrap > 0)
        {
            for (int i = 0; i < enemy.CarriedScrap; i++)
            {
                // fizzle while dropping shit
                Vector3 offset = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f));
                Vector3 spawnPos = new Vector3(enemy.transform.position.x, 0.5f, enemy.transform.position.z) + offset;

                ObjectPoolManager.SpawnObject(dropPrefab, spawnPos, Quaternion.identity, ObjectPoolManager.PoolType.Collectibles);
            }
        }

        if (deathVfxPrefab != null)
        {
            ObjectPoolManager.SpawnObject(deathVfxPrefab, enemy.transform.position, Quaternion.identity, ObjectPoolManager.PoolType.ParticleSystems);
        }

        ObjectPoolManager.ReturnObjectToPool(enemy.gameObject, ObjectPoolManager.PoolType.GameObjects);
    }
}