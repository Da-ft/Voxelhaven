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
    [SerializeField] private float globalDamage = 1f;
    [SerializeField] private float globalAttackSpeed = 1f;
    [SerializeField] private float globalCritRate = 0f; // 0-1 probability
    [SerializeField] private float globalCritDamage = 1.25f; // Multiplier
    [SerializeField] private float globalProjectileCount = 1f;
    #endregion

    // Runtime State
    private float currentHealth;
    private PlayerController playerController;

    // Read-only Properties

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public float GlobalDamage => globalDamage;
    public float GlobalAttackSpeed => globalAttackSpeed;
    public float GlobalCritRate => globalCritRate;
    public float GlobalCritDamage => globalCritDamage;
    public float GlobalProjectileCount => globalProjectileCount;

    // Der Avatar in der aktuellen Szene - unterscheidet sich von diesem persistenten
    // Singleton-GameObject selbst, Wird von SceneBootstrapper via BindAvatar() gesetzt
    public Transform AvatarTransform { get; private set; }

    // Fassade auf PlayerController.HorizontalVelocity, damit Enemy-Skripte (z. B. für
    // Leading Shots) nicht direkt auf PlayerController zugreifen müssen.
    public Vector3 AvatarVelocity => playerController != null ? playerController.HorizontalVelocity : Vector3.zero;


    // Events - consumers (UI, Audio, VFX) Subscribe here; Player never touches them directly

    public event Action<float, float> OnHealthChanged; // (currentHealth, MaxHealth)
    public event Action OnPlayerDied;

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
    }

    private void Update()
    {
        if (healthRegen > 0f && !IsDead && currentHealth < maxHealth)
            Heal(healthRegen * Time.deltaTime);
    }

    // Public API

    // Wird von SceneBootstrapper aufgerufen, sobald Avatar und PlayerController existieren.
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

        if(IsDead)
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

    // Erhöht maximale Health, bool für heal der dazugewonnenen maxHP
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
