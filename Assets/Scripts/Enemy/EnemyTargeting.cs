using UnityEngine;

public class EnemyTargeting : MonoBehaviour
{
    private Transform playerTransform;

    public bool HasTarget => playerTransform != null;

    public float DistanceToPlayer =>
        HasTarget ? Vector3.Distance(transform.position, playerTransform.position) : float.MaxValue;

    public Vector3 DirectionToPlayer =>
        HasTarget ? (playerTransform.position - transform.position).normalized : Vector3.zero;

    private void Start()
    {
        TryResolveTarget();
    }

    private void Update()
    {
        // Falls der Gegner vor dem Avatar-Spawn instanziiert wurde, hier erneut versuchen, bis eine gültige Referenz vorliegt. Sobald gesetzt, kein Overhead mehr.
        if (!HasTarget)
            TryResolveTarget();
    }

    private void TryResolveTarget()
    {
        if (Player.Instance != null && Player.Instance.AvatarTransform != null)
            playerTransform = Player.Instance.AvatarTransform;
    }
}
