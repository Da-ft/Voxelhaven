using UnityEngine;

[RequireComponent(typeof(EnemyTargeting))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private EnemyDefinitionSO definition;

    [Header("Separation (Anti-Stacking bei vielen Gegnern)")]
    [SerializeField] private float separationRadius = 1f;
    [SerializeField] private float separationStrength = 3f;
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("Reposition Tuning (nur bei canReposition)")]
    [SerializeField] private float repositionTolerance = 0.5f;
    // Toleranzband um die Angriffsreichweite, damit der Gegner nicht bei jeder Mikro-Distanzänderung zwischen Chase und Reposition zittert

    private EnemyTargeting targeting;
    private float moveSpeed;

    // Wiederverwendeter Buffer für Physics.OverlapSphereNonAlloc, um pro-Frame-Allocations bei vielen gleichzeitig aktiven Gegnern zu vermeiden
    private static readonly Collider[] neighborBuffer = new Collider[16];

    private void Awake()
    {
        targeting = GetComponent<EnemyTargeting>();
        moveSpeed = definition != null ? definition.MoveSpeed : 3f;

        if (definition == null)
            Debug.LogWarning("[EnemyMovement] Keine EnemyDefinitionSO zugewiesen, nutze Fallback-Werte.");
    }

    // Für spätere Wave-Skalierung, falls MoveSpeed irgendwann ebenfalls skaliert werden soll
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    private void Update()
    {
        if (!targeting.HasTarget) return;

        Vector3 desiredDirection = GetDesiredDirection();
        Vector3 separation = GetSeparationVector();

        Vector3 combined = desiredDirection + separation;
        if (combined.sqrMagnitude <= 0.0001f) return;

        Vector3 finalMove = combined.normalized;
        transform.position += finalMove * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(finalMove);
    }

    private Vector3 GetDesiredDirection()
    {
        if (definition == null || !definition.CanReposition)
        {
            // Nahkampf: läuft immer direkt auf den Spieler zu
            // Bewusst KEIN Distanz-Check hier - Hineinlaufen/Anstoßen ist gewolltes Verhalten
            return targeting.DirectionToPlayer;
        }

        // Fernkampf: versucht, exakt auf Angriffsreichweite zu bleiben
        float distance = targeting.DistanceToPlayer;
        float target = definition.AttackRange;

        if (distance > target + repositionTolerance)
            return targeting.DirectionToPlayer;   // zu weit weg -> annähern
        if (distance < target - repositionTolerance)
            return -targeting.DirectionToPlayer;  // zu nah -> zurückweichen

        return Vector3.zero; // innerhalb Toleranzband -> Position halten
    }

    private Vector3 GetSeparationVector()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, separationRadius, neighborBuffer, enemyLayerMask);
        Vector3 push = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Collider other = neighborBuffer[i];
            if (other == null || other.transform == transform) continue;

            Vector3 away = transform.position - other.transform.position;
            float dist = away.magnitude;
            if (dist > 0.001f)
                push += away.normalized / dist; // nähere Nachbarn stoßen stärker ab
        }

        return push * separationStrength;
    }
}