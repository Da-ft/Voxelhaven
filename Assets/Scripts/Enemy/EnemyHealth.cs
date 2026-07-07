using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamagable
{
    [SerializeField] private EnemyDefinitionSO definition;

    private float currentHealth;
    private bool isDead;

    public float MaxHealth { get; private set; }
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action<DamageInfo> OnDamageTaken;
    public event Action OnDeath;

    // Wird vom Spawner/WaveManager aufgerufen, NACHDEM Wave-Skalierung auf den Basiswert angewendet wurde. So kennt EnemyHealth selbst keine Skalierungslogik.
    public void Initialize(float scaledMaxHealth)
    {
        MaxHealth = scaledMaxHealth;
        currentHealth = scaledMaxHealth;
        isDead = false;
    }

    private void Awake()
    {
        // Fallback, falls Initialize() nicht aufgerufen wird (isolierter Test im Editor).
        if (MaxHealth <= 0f)
        {
            if (definition != null)
                Initialize(definition.BaseHealth);
            else
                Debug.LogWarning("[EnemyHealth] Keine EnemyDefinitionSO zugewiesen und Initialize() nicht aufgerufen.");
        }
    }

    public void TakeDamage(DamageInfo info)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - info.Amount, 0f, MaxHealth);

        OnDamageTaken?.Invoke(info);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        if (currentHealth <= 0f)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }
}
