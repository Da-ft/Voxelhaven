using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProfile", menuName = "ScriptableObjects/Enemy Melee Profile")]
public class EnemyMeleeProfile : EnemyProfileSO
{
    public override void ExecuteAttack(EnemyBrain enemy)
    {
        Player.Instance.TakeDamage(damage);

        Debug.Log($"{enemy.gameObject.name} macht {damage} Nahkampf-Schaden!");

        // TODO: Partikel-Effekte oder Sounds für den Melee-Hit
    }
}