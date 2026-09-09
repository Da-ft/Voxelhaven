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
        Drop(enemy);

        if (deathVfxPrefab != null)
        {
            ObjectPoolManager.SpawnObject(deathVfxPrefab, enemy.transform.position, Quaternion.identity, ObjectPoolManager.PoolType.ParticleSystems);
        }

        ObjectPoolManager.ReturnObjectToPool(enemy.gameObject, ObjectPoolManager.PoolType.GameObjects);
    }

    private void Drop(EnemyBrain enemy)
    {
        if (dropPrefab != null)
        {
            // Wir nehmen X und Z vom Gegner, aber setzen Y fest auf einen Wert (z. B. 0.5f oder 1.0f)
            Vector3 spawnPos = new Vector3(enemy.transform.position.x, 0.5f, enemy.transform.position.z);

            ObjectPoolManager.SpawnObject(dropPrefab, spawnPos, Quaternion.identity, ObjectPoolManager.PoolType.Collectibles);
        }
    }
}
