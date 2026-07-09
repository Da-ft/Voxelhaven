using UnityEngine;

[RequireComponent(typeof(EnemyTargeting))]
public class EnemyMeleeAttack : MonoBehaviour
{
    [SerializeField] private EnemyDefinitionSO definition;
    [SerializeField] private float damage = 10f;

    [Header("Windup")]
    [SerializeField] private float windupDuration = 0.3f;
    [SerializeField] private float hitRadius = 1f;
    [SerializeField] private float hitForwardOffset = 0.5f;
    [SerializeField] private LayerMask hittableLayerMask;
    // Layer-Maske MUSS im Editor so gesetzt werden, dass ausschließlich der Spieler-Layer getroffen wird - sonst können sich Nahkampf-Gegner theoretisch gegenseitig treffen.

    private EnemyTargeting targeting;
    private float cooldown;
    private float waitDuration;   // Cooldown minus Windup - die reine Wartephase
    private float effectiveWindup;
    private float phaseTimer;
    private bool isWindingUp;

    private void Awake()
    {
        targeting = GetComponent<EnemyTargeting>();

        float attackSpeed = definition != null ? definition.AttackSpeed : 1f;
        cooldown = attackSpeed > 0f ? 1f / attackSpeed : 1f;

        effectiveWindup = Mathf.Min(windupDuration, cooldown);
        if (windupDuration > cooldown)
            Debug.LogWarning($"[EnemyMeleeAttack] Windup-Dauer ({windupDuration}s) auf {name} ist länger als der volle Cooldown ({cooldown}s) und wird auf den Cooldown begrenzt.");

        waitDuration = cooldown - effectiveWindup;
        phaseTimer = waitDuration;
    }

    private void Update()
    {
        if (!targeting.HasTarget || definition == null) return;

        phaseTimer -= Time.deltaTime;

        if (isWindingUp)
        {
            if (phaseTimer <= 0f)
                ResolveHit();
            return;
        }

        if (phaseTimer <= 0f)
        {
            // Wartephase abgeschlossen. Windup startet erst, sobald der Spieler in Reichweite ist - bis dahin hält der Timer bei 0 und wird jeden Frame neu geprüft (kein Nachrücken ins Negative).
            phaseTimer = 0f;

            if (targeting.DistanceToPlayer <= definition.AttackRange)
            {
                isWindingUp = true;
                phaseTimer = effectiveWindup;
                // Hier später: Trigger für Angriffsanimation / Windup-VFX.
            }
        }
    }

    // Öffentlich und unabhängig vom internen Timer aufrufbar, damit dieser Aufruf später durch ein Animation Event ersetzt werden kann, ohne den Rest der Komponente zu ändern.
    public void ResolveHit()
    {
        isWindingUp = false;
        phaseTimer = waitDuration;

        Vector3 queryCenter = transform.position + transform.forward * hitForwardOffset;
        Collider[] hits = Physics.OverlapSphere(queryCenter, hitRadius, hittableLayerMask);

        if (hits.Length == 0 || Player.Instance == null) return; // Whiff - Spieler ist ausgewichen

        DamageInfo info = new DamageInfo(
            amount: damage,
            source: gameObject,
            type: DamageType.Physical,
            isCritical: false,
            hitPoint: queryCenter,
            hitDirection: targeting.DirectionToPlayer
        );

        Player.Instance.TakeDamage(info);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * hitForwardOffset, hitRadius);
    }
}