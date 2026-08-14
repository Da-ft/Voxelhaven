using UnityEngine;

[CreateAssetMenu(fileName = "New Stat Upgrade", menuName = "Skill Tree/Effects/ Stat Upgrade")]
public class SkillUpgradeEffectSO : SkillEffectSO
{
    public enum StatType
    {
        MaxHealth,
        HealthRegen,
        DamageModifier,
        RangeModifier,
        AttackSpeedModifier,
        CritRateModifier,
        CritDamageModifier,
        KnockbackModifier,
        ProjectileCountModifier,
    }

    [Header("Upgrade Settings")]
    public StatType statToUpgrade;
    public float upgradeValue;

    public override void Execute(PlayerController player)
    {
        Player stats = Player.Instance;

        switch (statToUpgrade)
        {
            case StatType.MaxHealth:
                stats.MaxHealth += upgradeValue;
                break;
            case StatType.HealthRegen:
                stats.HealthRegen += upgradeValue;
                break;
            case StatType.DamageModifier:
                stats.DamageModifier += upgradeValue;
                break;
            case StatType.RangeModifier:
                stats.RangeModifier += upgradeValue;
                break;
            case StatType.AttackSpeedModifier:
                stats.AttackSpeedModifier += upgradeValue;
                break;
            case StatType.CritRateModifier:
                stats.CritRateModifier += upgradeValue;
                break;
            case StatType.CritDamageModifier:
                stats.CritDamageModifier += upgradeValue;
                break;
            case StatType.KnockbackModifier:
                stats.KnockbackModifier += upgradeValue;
                break;
            case StatType.ProjectileCountModifier:
                stats.ProjectileCountModifier += (int)upgradeValue;
                break;
            default:
                break;
        }
    }
}
