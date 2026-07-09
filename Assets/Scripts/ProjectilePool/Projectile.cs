using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float maxLifetime = 5f;
    // Fallback-Absicherung: Projektile prüfen aktuell keine Level-Geometrie (konsistent mit der Entscheidung, dass auch EnemyMovement nicht gegen Wände kollidiert). Ohne diese Grenze würden verfehlte Schüsse endlos weiterfliegen und nie an den Pool zurückgegeben werden.

    private Vector3 direction;
    private float speed;
    private float damage;
    private GameObject source;
    private float lifeTimer;
    private GameObject originatingPrefab;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Wird von EnemyRangedAttack unmittelbar nach dem Herausholen aus dem Pool aufgerufen
    public void Launch(Vector3 direction, float speed, float damage, GameObject source)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.source = source;
        lifeTimer = maxLifetime;
    }

    // Wird vom Pool gesetzt, damit das Projektil weiß, unter welchem Prefab-Key es zurückzugeben ist
    public void SetOriginatingPrefab(GameObject prefab)
    {
        originatingPrefab = prefab;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Setzt eine korrekt konfigurierte Physik-Kollisionsmatrix voraus: Dieser Layer darf ausschließlich mit dem Spieler-Layer kollidieren, nicht mit anderen Gegnern
        if (Player.Instance == null) return;

        DamageInfo info = new DamageInfo(
            amount: damage,
            source: source,
            type: DamageType.Physical,
            isCritical: false,
            hitPoint: transform.position,
            hitDirection: direction
        );

        Player.Instance.TakeDamage(info);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.Return(originatingPrefab, this);
        else
            Destroy(gameObject); // Fallback, falls der Pool zur Laufzeit nicht mehr existiert
    }
}