using UnityEngine;

public class ComboSpinTrait : IWeaponTrait
{
    private int hitsRequired;
    private float spinRadius;
    private float comboDamageMultiplier;
    private GameObject vfxPrefab;
    private float vfxScale;

    public ComboSpinTrait(int hitsRequired = 3, float spinRadius = 4f, float comboDamageMultiplier = 1.5f, GameObject vfxPrefab = null, float vfxScale = 1f)
    {
        this.hitsRequired = hitsRequired;
        this.spinRadius = spinRadius;
        this.comboDamageMultiplier = comboDamageMultiplier;
        this.vfxPrefab = vfxPrefab;
        this.vfxScale = vfxScale;
    }

    public void OnPreAttack(PlayerController player, WeaponInstance instance, Transform currentTarget)
    {
        // emptyness
    }

    public void OnPostAttack(PlayerController player, WeaponInstance instance, Transform currentTarget)
    {
        if (instance.AttackCount % hitsRequired == 0)
        {
            WeaponStats stats = instance.GetCurrentStats();

            float spinDamage = stats.damage * comboDamageMultiplier;
            bool isCrit = Random.value <= stats.critRate;
            float finalDamage = isCrit ? (spinDamage * stats.critDamage) : spinDamage;

            Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position, spinRadius, player.enemyLayer);

            foreach (Collider hit in hitEnemies)
            {
                if (hit.TryGetComponent(out EnemyBrain enemy))
                {
                    enemy.TakeDamage(finalDamage);
                }
            }

            // VFX
            if (vfxPrefab != null)
            {
                Transform spawnPoint = player.weaponSpawnPoint != null ? player.weaponSpawnPoint : player.transform;
                Vector3 spawnPos = new Vector3(player.transform.position.x, spawnPoint.position.y, player.transform.position.z);

                GameObject vfx = ObjectPoolManager.SpawnObject(vfxPrefab, spawnPos, Quaternion.identity, ObjectPoolManager.PoolType.ParticleSystems);

                vfx.transform.localScale = vfxPrefab.transform.localScale * vfxScale;
            }

            Debug.Log($"<color=orange>[TRAIT] WIRBELWIND-FINISHER! ({finalDamage} Dmg auf {hitEnemies.Length} Gegner!)</color>");
        }
    }
}