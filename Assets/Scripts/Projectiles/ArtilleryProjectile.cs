using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class ArtilleryProjectile : MonoBehaviour
{
    private float damage;
    private float aoeRadius;
    private bool hasExploded = false;

    public void Initialize(float damageAmount, float radius, Vector3 launchVelocity)
    {
        this.damage = damageAmount;
        this.aoeRadius = radius;

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
            if (hit.CompareTag("Player"))
            {
                Player.Instance.TakeDamage(damage);

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
