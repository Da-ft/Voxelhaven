using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PeashooterProjectile : MonoBehaviour
{
    private float damage;

    public void Initialize(float damageAmount, float speed, float lifetime)
    {
        this.damage = damageAmount;

        // Rigidbody konfigurieren
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;          // Kugel soll nicht auf den Boden fallen
        rb.isKinematic = false;         // Muss false sein, damit velocity funktioniert

        // Impuls genau nach vorne (lokale Z-Achse) geben
        rb.linearVelocity = transform.forward * speed;

        // Projektil automatisch zerstören, wenn es seine maximale Reichweite erreicht hat
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignoriere den Spieler selbst oder andere Trigger
        if (other.CompareTag("Player") || other.isTrigger) return;

        // Prüfen, ob wir einen Gegner getroffen haben
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent(out EnemyBrain enemy)) enemy.TakeDamage(damage);
            Debug.Log($"Peashooter hat Gegner getroffen f�r {damage} Schaden!");
        }

        // Egal ob Wand oder Gegner: Das Projektil wird beim Aufprall zerstört
        Destroy(gameObject);
    }
}