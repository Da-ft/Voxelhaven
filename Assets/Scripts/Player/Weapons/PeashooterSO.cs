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

        // 1. Projektil spawnen. 
        // Da der Spieler sich zur Maus/Gegner dreht, zeigt die Rotation des SpawnPoints exakt in Schussrichtung.
        GameObject proj = Instantiate(projectilePrefab, player.weaponSpawnPoint.position, player.weaponSpawnPoint.rotation);

        // 2. Initialisieren
        if (proj.TryGetComponent(out PeashooterProjectile projectileScript))
        {
            // Rechnet den Basis-Schaden mit dem globalen Multiplikator aus Player.cs zusammen
            float finalDamage = damage * Player.Instance.GlobalDamage;

            // Lebensdauer des Projektils berechnen (Reichweite / Geschwindigkeit)
            // Verhindert, dass Projektile unendlich weit fliegen und den RAM vollmüllen.
            float lifeTime = range / projectileSpeed;

            projectileScript.Initialize(finalDamage, projectileSpeed, lifeTime);
        }
    }
}