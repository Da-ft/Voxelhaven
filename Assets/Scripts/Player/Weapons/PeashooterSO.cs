using UnityEngine;

[CreateAssetMenu(fileName = "Peashooter", menuName = "ScriptableObjects/Weapons/Peashooter")]
public class PeashooterSO : PlayerWeaponSO
{
    [Header("Peashooter Settings")]
    [Tooltip("Das Prefab, das abgefeuert werden soll")]
    public GameObject projectilePrefab;

    [Tooltip("Wie schnell fliegt die Kugel?")]
    public float projectileSpeed = 20f;

    public override void ExecuteAttack(PlayerController player, Transform currentTarget = null)
    {
        // Sicherheitsabfrage
        if (projectilePrefab == null || player.weaponSpawnPoint == null) return;

        // spawn projectile in avatar rotation -> lookDir = mousePos
        GameObject proj = Instantiate(projectilePrefab, player.weaponSpawnPoint.position, player.weaponSpawnPoint.rotation);

        if (proj.TryGetComponent(out PeashooterProjectile projectileScript))
        {
            // TODO: Finalise Dmg Equation from Player Weapons
            float finalDamage = damage * Player.Instance.GlobalDamage;

            // Calc Lifetime
            // TODO: Rework Lifetime Projectile segment, maybe with vfx?
            float lifeTime = range / projectileSpeed;

            projectileScript.Initialize(finalDamage, projectileSpeed, lifeTime);
        }
    }
}