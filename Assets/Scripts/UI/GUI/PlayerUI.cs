using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (Player.Instance != null)
        {
            Player.Instance.OnHealthChanged += UpdateHealthBar;
            isSubscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (isSubscribed && Player.Instance != null)
        {
            Player.Instance.OnHealthChanged -= UpdateHealthBar;
            isSubscribed = false;
        }
    }


    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null || maxHealth > 0f)
        {
            healthSlider.value = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            int currentInt = Mathf.CeilToInt(currentHealth);
            int maxInt = Mathf.CeilToInt(maxHealth);

            healthText.text = $"{currentInt} / {maxInt}";
        }
    }
}
