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

        // Projektil nach 5 Sekunden automatisch zerstören, falls es nichts trifft (Speicherplatz sparen!)
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Fliegt simpel in die Blickrichtung (forward)
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Wenn das Projektil den Spieler trifft
        // WICHTIG: Dein Spieler braucht den Tag "Player" und einen Collider
        if (other.CompareTag("Player"))
        {
            // TODO: Player.Instance.TakeDamage(damage); aufrufen
            Debug.Log($"Projektil trifft Spieler für {damage} Schaden!");
            Destroy(gameObject); // Projektil beim Treffer zerstören
        }
        else if (!other.isTrigger)
        {
            // Zerstören, wenn es gegen eine Wand fliegt
            Destroy(gameObject);
        }
    }
}