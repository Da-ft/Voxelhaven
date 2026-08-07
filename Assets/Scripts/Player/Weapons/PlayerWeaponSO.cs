using UnityEngine;

public abstract class PlayerWeaponSO : ScriptableObject
{
    [Header("Weapon Stats")]
    public float damage = 10f;
    public float attackCooldown = 0.5f;
    public float range = 10f;

    public abstract void ExecuteAttack(PlayerController player, Transform currentTarget = null);
}
