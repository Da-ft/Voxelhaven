using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradeData", menuName = "Shop Upgrade Data")]
public class UpgradeDataSO : ScriptableObject
{
    [Header("Allgemeine Infos")]
    public string upgradeID;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Shop & Drafting")]
    public NodeType nodeType = NodeType.InRunUpgrade;
    public UpgradeRarity rarity = UpgradeRarity.Common;
    public int dropWeight = 100;
    public bool isUnlockedForShop = true;

    [Header("Kauf Regeln")]
    public bool isRepeatable = true;
    [Tooltip("Maximale Anzahl an Käufen. Setze auf 0 für unendlich oft kaufbar.")]
    public int maxPurchases = 0;
    public List<UpgradeDataSO> parentNodes;

    [Header("In-Run Kosten (Scrap & Mana)")]
    public int baseScrapCost = 10;
    public int baseManaCost = 0;
    public AnimationCurve costScalingCurve = AnimationCurve.Linear(1, 1, 20, 5);

    [Header("Meta-Progression Kosten")]
    public int metaResourceCost = 50;

    [Header("Payload 1: Reine Stat Upgrades")]
    public List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("Payload 2: Spezielle Effekte (Zukunft)")]
    public List<UpgradeEffectSO> specialEffects = new List<UpgradeEffectSO>();

    public (int scrapCost, int manaCost) GetScaledCosts(int currentCycle)
    {
        float multiplier = costScalingCurve.Evaluate(currentCycle);
        int scaledScrap = Mathf.RoundToInt(baseScrapCost * multiplier);
        int scaledMana = Mathf.RoundToInt(baseManaCost * multiplier);
        return (scaledScrap, scaledMana);
    }

    /// <summary>
    /// Führt die Effekte direkt auf der Player-Klasse aus
    /// </summary>
    public void ApplyEffects(Player player)
    {
        if (player == null) return;

        // 1. Array an direkten Stats auslesen
        foreach (var modifier in statModifiers)
        {
            ApplyStatModifier(player, modifier);
        }

        // 2. Komplexe Zusatzeffekte ausführen
        foreach (var effect in specialEffects)
        {
            if (effect != null)
            {
                effect.Execute(player);
            }
        }
    }

    private void ApplyStatModifier(Player player, StatModifier mod)
    {
        switch (mod.statType)
        {
            case StatType.Damage:
                player.DamageModifier += mod.isPercentage ? (player.DamageModifier * mod.amount) : mod.amount;
                break;
            case StatType.AttackSpeed:
                player.AttackSpeedModifier += mod.amount;
                break;
            case StatType.CritRate:
                player.CritRateModifier += mod.amount;
                break;
            case StatType.CritDamage:
                player.CritDamageModifier += mod.amount;
                break;
            case StatType.Range:
                player.RangeModifier += mod.amount;
                break;
            case StatType.Knockback:
                player.KnockbackModifier += mod.amount;
                break;
            case StatType.ProjectileCount:
                player.ProjectileCountModifier += Mathf.FloorToInt(mod.amount);
                break;
            case StatType.MaxHealth:
                float healthIncrease = mod.isPercentage ? (player.MaxHealth * mod.amount) : mod.amount;
                player.AddMaxHealth(healthIncrease, true);
                break;
            
        }
    }
}