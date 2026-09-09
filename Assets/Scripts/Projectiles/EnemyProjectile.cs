using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private GameObject source;
    private bool isInitialized = false;

    public void Initialize(float damageAmount, float moveSpeed, GameObject sourceObject = null)
    {
        this.damage = damageAmount;
        this.speed = moveSpeed;
        this.source = sourceObject;
        this.isInitialized = true;

        // TODO: Back to the bulletpool at the end of lifetime
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (!isInitialized) return;

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player needs tag:"Player" and IDamageable
        if (other.CompareTag("Player") && other.TryGetComponent(out IDamageable damageable))
        {
            DamageInfo info = new DamageInfo
            {
                amount = damage,
                isCritical = false,
                knockback = 0f,
                source = source
            };

            damageable.TakeDamage(info);

            Debug.Log($"Projektil trifft Spieler für {damage} Schaden!");
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}