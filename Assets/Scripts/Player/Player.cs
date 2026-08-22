using UnityEngine;
using System;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    #region Serialized Fields
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthRegen = 0f;

    [Header("Global Modifier")]
    [SerializeField] private float damageModifier = 1f;
    [SerializeField] private float rangeModifier = 1f;
    [SerializeField] private float attackSpeedModifier = 1f;
    [SerializeField] private float critRateModifier = 0f; // 0-1 probability
    [SerializeField] private float critDamageModifier = 1.5f;
    [SerializeField] private float knockbackModifier = 1f;
    [SerializeField] private int projectileCountModifier = 1;
    [SerializeField] private float luckModifier = 1f;
    #endregion

    // Runtime State
    private float currentHealth;
    private PlayerController playerController;

    // Read-only Properties
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float CurrentHealth => currentHealth;
    public float HealthRegen { get => healthRegen; set => healthRegen = value; }
    public float DamageModifier { get => damageModifier; set => damageModifier = value; }
    public float RangeModifier { get => rangeModifier; set => rangeModifier = value; }
    public float AttackSpeedModifier { get => attackSpeedModifier; set => attackSpeedModifier = value; }
    public float CritRateModifier { get => critRateModifier; set => critRateModifier = value; }
    public float CritDamageModifier { get => critDamageModifier; set => critDamageModifier = value; }
    public float KnockbackModifier { get => knockbackModifier; set => knockbackModifier = value; }
    public int ProjectileCountModifier { get => projectileCountModifier; set => projectileCountModifier = value; }
    public float LuckModifier { get => luckModifier; set => luckModifier = value; }
    public bool IsDead => currentHealth <= 0f;

    public Transform AvatarTransform { get; private set; }

    public Vector3 AvatarVelocity => playerController != null ? playerController.HorizontalVelocity : Vector3.zero;


    // Events - consumers (UI, Audio, VFX) Subscribe here; Player never touches them directly

    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;

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
    }

    private void Update()
    {
        if (healthRegen > 0f && !IsDead && currentHealth < maxHealth)
            Heal(healthRegen * Time.deltaTime);
    }

    // Public API
    // Binds Avatar sobald Avatar und PlayerController existieren.
    public void BindAvatar(Transform avatarTransform, PlayerController controller)
    {
        AvatarTransform = avatarTransform;
        playerController = controller;
    }

    // Health and Damage Logic
    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        // Fire event, ui gets event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsDead)
        {
            OnPlayerDied?.Invoke();
            Debug.Log("Player died!");
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        // Fire event, ui gets event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Increment maxHP, bool für heal der dazugewonnenen maxHP
    public void AddMaxHealth(float amount, bool healAmount = true)
    {
        if (amount <= 0f) return;
        maxHealth += amount;

        if (healAmount)
        {
            currentHealth += amount;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
