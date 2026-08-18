using System;

public enum UpgradeRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum NodeType
{
    InRunUpgrade,      // Setzt sich nach dem Run zurück
    MetaProgression    // Bleibt für immer erhalten
}

public enum StatType
{
    MaxHealth,
    HealthRegen,
    Damage,
    Range,
    AttackSpeed,
    CritRate,
    CritDamage,
    Knockback,
    ProjectileCount,
    Luck,
}

[Serializable]
public struct StatModifier
{
    public StatType statType;
    public float amount;
    public bool isPercentage;
}