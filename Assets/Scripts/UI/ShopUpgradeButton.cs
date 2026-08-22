using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUpgradeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI purchaseCountText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button buyButton;

    [Tooltip("UpgradeDataSO this button should show.")]
    [SerializeField] private UpgradeDataSO currentData;

    private void OnEnable()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeChanged += UpdateVisuals;
        }

        UpdateVisuals();
    }

    private void OnDisable()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeChanged -= UpdateVisuals;
        }
    }

    public void Setup(UpgradeDataSO draftData)
    {
        currentData = draftData;
        gameObject.SetActive(currentData != null);

        UpdateVisuals();
    }

    /// <summary>
    /// Diese Methode sorgt dafür, dass sich Bild, Text und Zustand des Buttons aktualisieren.
    /// Sie wird automatisch aufgerufen, sobald irgendjemand etwas kauft!
    /// </summary>
    private void UpdateVisuals()
    {
        if (currentData == null || GameManager.Instance == null) return;

        titleText.text = currentData.displayName;
        descriptionText.text = currentData.description;

        if (currentData.icon != null)
        {
            iconImage.sprite = currentData.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        // Calc dynamic costs
        int currentCycle = GameManager.Instance.CycleCounter;
        var (scrapCost, manaCost) = currentData.GetScaledCosts(currentCycle);

        string costString = "";
        if (scrapCost > 0) costString += $"<color=#FFD700>{scrapCost} Scrap</color> ";
        if (manaCost > 0) costString += $"<color=#00FFFF>{manaCost} Mana</color>";
        costText.text = costString;

        int purchases = UpgradeManager.Instance.GetPurchaseCount(currentData.upgradeID);
        bool maxReached = false;

        if (!currentData.isRepeatable && purchases > 0)
        {
            purchaseCountText.text = "Gekauft!";
            maxReached = true;
        }
        else if (currentData.isRepeatable)
        {
            if (currentData.maxPurchases > 0)
            {
                purchaseCountText.text = $"{purchases} / {currentData   .maxPurchases}";
                if (purchases >= currentData.maxPurchases) maxReached = true;
            }
            else
            {
                purchaseCountText.text = $"Gekauft: {purchases}";
            }
        }
        else
        {
            purchaseCountText.text = "";
        }

        bool hasEnoughScrap = GameManager.Instance.Scrap >= scrapCost;
        bool hasEnoughMana = GameManager.Instance.Mana >= manaCost;

        buyButton.interactable = hasEnoughScrap && hasEnoughMana && !maxReached;
    }

    public void OnBuyButtonClicked()
    {
        if (currentData != null)
        {
            UpgradeManager.Instance.AttemptBuyUpgrade(currentData);
        }
    }
}