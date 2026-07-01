using UnityEngine;

public enum DamageType
{
    Physical,
    Fire,
    Fall,
    True
}

public struct DamageInfo
{
    public float Amount;
    public GameObject Source; // Who or what caused the damage (for kill credit, quests)
    public DamageType Type;
    public bool IsCritical;
    public Vector3 HitPoint; // World-space impact point (for VFX placement)
    public Vector3 HitDirection; // Normalized direction of hit (for knockback)

    public DamageInfo(
        float amount,
        GameObject source,
        DamageType type = DamageType.Physical,
        bool isCritical = false,
        Vector3 hitPoint = default,
        Vector3 hitDirection = default)
    {
        Amount = amount;
        Source = source;
        Type = type;
        IsCritical = isCritical;
        HitPoint = hitPoint;
        HitDirection = hitDirection;
    }
}
