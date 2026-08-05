using UnityEngine;

public abstract class EnemyProfileSO : ScriptableObject
{
    public string enemyName;

    [Header("Base Stats")]
    [Tooltip("Defines the starting Maximum HP, scales with the 'level' modifier.")]
    public float maxHealth;
    [Tooltip("This should be a decimal Value: 1.25 = 1.25 HP per Second.")]
    public float healthRegen;
    [Tooltip("Defines the Movespeed of the selected Unit.")]
    public float moveSpeed;

    [Header("Combat Stats")]
    [Tooltip("Defines the starting Raw Damage without Armor Calc, scales with the 'level' modifier.")]
    public float damage;
    [Tooltip("Defines the Range of this Unit.")]
    public float attackRange;
    [Tooltip("This should be a decimal Value: 1.25 = 1.25 Attacks per Second.")]
    public float attackSpeed;

    [Header("Loot und FX.")]
    [Tooltip("Defines the item to be dropped.")]
    public GameObject dropPrefab;
    [Tooltip("Insert death VFX.")]
    public GameObject deathVfxPrefab;

    public abstract void ExecuteAttack(EnemyBrain enemy);

    public virtual void ExecuteDeath(EnemyBrain enemy)
    {
        if (dropPrefab != null)
        {
            //Vector3 spawnPos = enemy.transform.position + Vector3.up * 0.5f;#
            Vector3 spawnPos = enemy.transform.position.magnitude, ;

            if (Physics.Raycast(enemy.transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 2f))
            {
                // Wenn der Boden getroffen wird, spawnen wir exakt 0.2 Einheiten über dem Boden
                spawnPos = hit.point + Vector3.up * 0.2f;
            }

            ObjectPoolManager.SpawnObject(dropPrefab, spawnPos, Quaternion.identity, ObjectPoolManager.PoolType.Collectibles);
        }

        if (deathVfxPrefab != null)
        {
            ObjectPoolManager.SpawnObject(deathVfxPrefab, enemy.transform.position, Quaternion.identity, ObjectPoolManager.PoolType.ParticleSystems);
        }

        ObjectPoolManager.ReturnObjectToPool(enemy.gameObject, ObjectPoolManager.PoolType.GameObjects);
    }
}
