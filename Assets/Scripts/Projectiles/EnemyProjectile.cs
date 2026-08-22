using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private bool isInitialized = false;

    public void Initialize(float damageAmount, float moveSpeed)
    {
        this.damage = damageAmount;
        this.speed = moveSpeed;
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
        // Player needs tag:"Player" and collider
        if (other.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(damage);

            Debug.Log($"Projektil trifft Spieler für {damage} Schaden!");
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}