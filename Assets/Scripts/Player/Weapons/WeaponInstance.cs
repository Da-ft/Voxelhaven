using System.Collections.Generic;
using UnityEngine;


public struct WeaponStats
{
    public float damage;
    public float range;
    public float attackSpeed;
    public float critRate;
    public float critDamage;
    public float knockback;
    public float projectileCount;

    // Helpermethod, translate attackSpeed to CD-Timer for PlayerController
    public float GetCooldown() => 1f / Mathf.Max(0.1f, attackSpeed);
}

public class WeaponInstance
{
    public PlayerWeaponSO BaseWeapon { get; private set; }
    public int AttackCount { get; private set; }
    public List<IWeaponTrait> ActiveTraits { get; private set; } = new List<IWeaponTrait>();

    public WeaponInstance(PlayerWeaponSO baseWeapon)
    {
        BaseWeapon = baseWeapon;
        AttackCount = 0;
    }

    public void AddTrait(IWeaponTrait trait)
    {
        // Stopps to add one trait multiple times
        if (!ActiveTraits.Contains(trait))
        {
            ActiveTraits.Add(trait);
        }
    }

    public WeaponStats GetCurrentStats()
    {
        Player player = Player.Instance;
        return new WeaponStats
        {
            damage = BaseWeapon.baseDamage * player.DamageModifier,
            range = BaseWeapon.baseRange * player.RangeModifier,
            attackSpeed = BaseWeapon.baseAttackSpeed * player.AttackSpeedModifier,
            critRate = BaseWeapon.baseCritRate + player.CritRateModifier,
            critDamage = BaseWeapon.baseCritDamage * player.CritDamageModifier,
            knockback = BaseWeapon.baseKnockback * player.KnockbackModifier,
            projectileCount = BaseWeapon.baseProjectileCount + Mathf.FloorToInt(player.ProjectileCountModifier - 1f)
        };
    }

    public void ApplyUnlockedTraits()
    {
        if (UpgradeManager.Instance == null) return;

        List<IWeaponTrait> traits = UpgradeManager.Instance.GetUnlockedTraits();
        foreach (var trait in traits)
        {
            AddTrait(trait);
        }
    }

    public void ExecuteAttack(PlayerController player, Transform currentTarget)
    {
        AttackCount++;

        // Traits before Attack
        foreach (var trait in ActiveTraits)
            trait.OnPreAttack(player, this, currentTarget);

        // Weapon Attack
        BaseWeapon.ExecuteAttack(player, this, currentTarget);

        // Traits after Attack
        foreach (var trait in ActiveTraits)
            trait.OnPostAttack(player, this, currentTarget);
    }
}