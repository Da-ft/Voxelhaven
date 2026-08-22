using UnityEngine;

public class ComboSpinTrait : IWeaponTrait
{
    private int hitsRequired;
    private float spinRadius;
    private float comboDamageMultiplier;

    public ComboSpinTrait(int hitsRequired = 3, float spinRadius = 4f, float comboDamageMultiplier = 1.5f)
    {
        this.hitsRequired = hitsRequired;
        this.spinRadius = spinRadius;
        this.comboDamageMultiplier = comboDamageMultiplier;
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

            // Spin Damage
            float spinDamage = stats.damage * comboDamageMultiplier;
            bool isCrit = Random.value <= stats.critRate;
            float finalDamage = isCrit ? (spinDamage * stats.critDamage) : spinDamage;

            // Calc Spin Attack (Player Center)
            Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position, spinRadius, player.enemyLayer);

            foreach (Collider hit in hitEnemies)
            {
                if (hit.TryGetComponent(out EnemyBrain enemy))
                {
                    enemy.TakeDamage(finalDamage);
                }
            }

            Debug.Log($"<color=orange>[TRAIT] WIRBELWIND-FINISHER! ({finalDamage} Dmg auf {hitEnemies.Length} Gegner!)</color>");
        }
    }
}