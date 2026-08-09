using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoReturnVFX : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (ps != null)
        {
            // Alter Zustand wird aufgeräumt und der Effekt neu gestartet
            ps.Clear();
            ps.Play(true);
        }
    }

    private void Update()
    {
        // ps.IsAlive(true) prüft auch alle untergeordneten Partikelsysteme (Child VFX)
        if (ps != null && !ps.IsAlive(true))
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.ParticleSystems);
        }
    }
}