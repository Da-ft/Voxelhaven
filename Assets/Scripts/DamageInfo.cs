using UnityEngine;

public struct DamageInfo
{
    public float amount;
    public bool isCritical;
    public float knockback;
    public GameObject source;
}

public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
}