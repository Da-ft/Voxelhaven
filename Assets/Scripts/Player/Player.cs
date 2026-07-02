using UnityEngine;
using System;

public class Player : MonoBehaviour, IDamagable
{
    public static Player Instance { get; private set; }

    #region Serialized Fields
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthRegen = 0f;

    [Header("Progression")]
    [SerializeField] private float xpPerLevel = 100f;

    [Header("Global Modifier")]
    [SerializeField] private float globalDamage = 1f;
    [SerializeField] private float globalAttackSpeed = 1f;
    [SerializeField] private float globalCritRate = 0f; // 0-1 probability
    [SerializeField] private float globalCritDamage = 1.25f; // Multiplier
    [SerializeField] private float globalProjectileCount = 1f;
    #endregion

    // Runtime State

    private float currentHealth;
    private float currentXp;
    private int currentLevel;

    // Read-only Properties

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public float CurrentXp => currentXp;
    public int CurrentLevel => currentLevel;
    public float GlobalDamage => globalDamage;
    public float GlobalAttackSpeed => globalAttackSpeed;
    public float GlobalCritRate => globalCritRate;
    public float GlobalCritDamage => globalCritDamage;
    public float GlobalProjectileCount => globalProjectileCount;

    // Events - consumers (UI, Audio, VFX) Subscribe here; Player never touches them directly

    public event Action<float, float> OnHealthChanged; // (currentHealth, MaxHealth)
    public event Action<DamageInfo> OnDamageTaken; // Seperate from OnHealthChanged for crit/type reactions
    public event Action OnDeath;
    public event Action<float> OnXpGained; // (amount gained this call)
    public event Action<int> OnLevelUp; // (new level)

    // Lifecycle

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentHealth = maxHealth;
        currentXp = 0f; // TODO: Tweak XP stuff
        currentLevel = 1;
    }

    private void Update()
    {
        if (healthRegen > 0f && !IsDead)
            Heal(healthRegen * Time.deltaTime);
    }

    // Public API

    public void TakeDamage(DamageInfo info)
    {
        if (IsDead) return;

        currentHealth = Mathf.Clamp(currentHealth - info.Amount, 0f, maxHealth);

        OnDamageTaken?.Invoke(info);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (!IsDead)
            OnDeath.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddXp(float amount)
    {
        if (amount <= 0f) return;

        currentXp += amount;
        OnXpGained?.Invoke(amount);

        while (currentXp >= xpPerLevel)
        {
            currentXp -= xpPerLevel;
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
        }
    }
}
