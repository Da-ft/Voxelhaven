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
            Vector3 spawnPos = enemy.transform.position + Vector3.up * 0.5f;
            Instantiate(dropPrefab, spawnPos, Quaternion.identity);
            // TODO: Pooling
        }

        if (deathVfxPrefab != null)
        {
            Instantiate(deathVfxPrefab, enemy.transform.position, Quaternion.identity);
            // TODO: Pooling
        }
        // TODO: Pooling
        Destroy(enemy.gameObject);  
    }
}
