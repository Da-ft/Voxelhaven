using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProfile", menuName = "ScriptableObjects/Enemy Artillery Profile")]
public class EnemyArtilleryProfile : EnemyProfileSO
{
    [Header("Artillery Settings")]
    public GameObject projectileAOE;

    [Tooltip("Wie lange ist das Projektil in der Luft?")]
    public float flightTime = 1.5f;

    [Tooltip("Der Radius der Explosion.")]
    public float aoeRadius = 3.5f;

    public override void ExecuteAttack(EnemyBrain enemy)
    {
        if (projectileAOE == null) return;

        // Startposition etwas überm Kopp
        Vector3 spawnPos = enemy.transform.position + Vector3.up * 2f;

        // Move Prediction
        Vector3 playerVelocity = Player.Instance.AvatarVelocity;
        // Wurfparabel
        Vector3 predictedTargetPos = enemy.PlayerTarget.position + (playerVelocity * flightTime);
        // distance vector from spawn to target
        Vector3 displacement = predictedTargetPos - spawnPos;
        // lineare geschw
        Vector3 launchVelocity = displacement / flightTime;
        // y config to balance gravity mid air
        // Vy = (y/t) + 0.5 * g * t
        launchVelocity.y += 0.5f * Mathf.Abs(Physics.gravity.y) * flightTime;

        // Projectile Spawn and FIRE
        GameObject proj = Instantiate(projectileAOE, spawnPos, Quaternion.identity);

        if (proj.TryGetComponent(out ArtilleryProjectile artilleryProj))
        {
            artilleryProj.Initialize(damage, aoeRadius, launchVelocity, enemy.gameObject);
        }
    }
}