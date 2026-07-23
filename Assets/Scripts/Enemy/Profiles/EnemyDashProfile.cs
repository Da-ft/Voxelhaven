using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProfile", menuName = "ScriptableObjects/Enemy Dash Profile")]
public class EnemyDashProfile : EnemyProfileSO
{
    [Header("Dash Settings")]
    public float chargeTime = 0.5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.3f;

    public override void ExecuteAttack(EnemyBrain enemy)
    {
        enemy.StartCoroutine(DashRoutine(enemy));
    }

    private IEnumerator DashRoutine(EnemyBrain enemy)
    {
        enemy.IsActionLocked = true;

        Debug.Log("CHAAAAAAARGE, oh und hier sollte eine animation oder ein Partikeleffekt sein!");

        yield return new WaitForSeconds(chargeTime);

        if (enemy.PlayerTarget == null) yield break;

        Vector3 dashDirection = (enemy.PlayerTarget.position - enemy.transform.position).normalized;
        dashDirection.y = 0f;

        float timer = 0f;
        bool hasDealtDamage = false;

        Debug.Log($"{enemy.gameObject.name} DASHT!");

        while (timer < dashDuration)
        {
            enemy.agent.Move(dashDirection * dashSpeed * Time.deltaTime);

            if (!hasDealtDamage)
            {
                float sqrDistanceToPlayer = (enemy.PlayerTarget.position - enemy.transform.position).sqrMagnitude;

                if (sqrDistanceToPlayer < 1.5f * 1.5f)
                    Player.Instance.TakeDamage(damage);

                Debug.Log($"Dash hat den Spieler getroffen!");
                hasDealtDamage = true;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        enemy.IsActionLocked = false;
    }
}
