using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PeashooterProjectile : MonoBehaviour
{
    private float damage;

    public void Initialize(float damageAmount, float speed, float lifetime)
    {
        this.damage = damageAmount;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;  
        rb.isKinematic = false;

        // Forcepush along z axis
        rb.linearVelocity = transform.forward * speed;

        // Destroy at max range
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.isTrigger) return;

        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent(out EnemyBrain enemy)) enemy.TakeDamage(damage);
            Debug.Log($"Peashooter hat Gegner getroffen f�r {damage} Schaden!");
        }

        // TODO: Pooling
        Destroy(gameObject);
    }
}