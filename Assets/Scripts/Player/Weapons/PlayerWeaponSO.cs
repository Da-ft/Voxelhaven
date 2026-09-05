using UnityEngine;

public abstract class PlayerWeaponSO : ScriptableObject
{
    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseRange = 3f;
    public float baseAttackSpeed = 1.5f;
    [Range(0f, 1f)] public float baseCritRate = 0.05f;
    public float baseCritDamage = 1.5f;
    public float baseKnockback = 5f;
    public int baseProjectileCount = 1;

    [Header("Visuals")]
    [Tooltip("Das 3D-Modell/Prefab der Waffe, das in der Hand des Spielers gehalten wird")]
    public GameObject weaponMeshPrefab;

    public abstract void ExecuteAttack(PlayerController player, WeaponInstance instance, Transform currentTarget = null);

    public virtual void DrawGizmos(PlayerController player, WeaponStats stats)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.transform.position, stats.range);
    }
}