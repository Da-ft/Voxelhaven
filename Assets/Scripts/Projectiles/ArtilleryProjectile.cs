using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArtilleryProjectile : MonoBehaviour
{
    private float damage;
    private float aoeRadius;
    private GameObject source;
    private bool hasExploded = false;

    public void Initialize(float damageAmount, float radius, Vector3 launchVelocity, GameObject sourceObject = null)
    {
        this.damage = damageAmount;
        this.aoeRadius = radius;
        this.source = sourceObject;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.linearVelocity = launchVelocity;

        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded || other.CompareTag("Enemy") || other.isTrigger) return;
        Explode();
    }

    private void Explode()
    {
        hasExploded = true;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent(out IDamageable damageable))
            {
                DamageInfo info = new DamageInfo
                {
                    amount = damage,
                    isCritical = false,
                    knockback = 0f,
                    source = source
                };

                damageable.TakeDamage(info);

                Debug.Log($"Artillery Damaged Player!");
            }
        }

        // TODO: Artillery Particle Effect and Sounds!
        // TODO: Pooling for Artillery!
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, aoeRadius);
    }
}