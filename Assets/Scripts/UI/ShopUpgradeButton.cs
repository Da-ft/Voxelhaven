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

    [Tooltip("SkillNodeSO this button should show.")]
    [SerializeField] private SkillNodeSO nodeData;

    private void OnEnable()
    {
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.OnSkillTreeChanged += UpdateVisuals;
        }

        UpdateVisuals();
    }

    private void OnDisable()
    {
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.OnSkillTreeChanged -= UpdateVisuals;
        }
    }

    /// <summary>
    /// Diese Methode sorgt dafür, dass sich Bild, Text und Zustand des Buttons aktualisieren.
    /// Sie wird automatisch aufgerufen, sobald irgendjemand etwas kauft!
    /// </summary>
    private void UpdateVisuals()
    {
        if (nodeData == null || GameManager.Instance == null) return;

        titleText.text = nodeData.displayName;
        descriptionText.text = nodeData.description;

        if (nodeData.icon != null)
        {
            iconImage.sprite = nodeData.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        // Calc dynamic costs
        int currentCycle = GameManager.Instance.CycleCounter;
        var (scrapCost, manaCost) = nodeData.GetScaledCosts(currentCycle);

        string costString = "";
        if (scrapCost > 0) costString += $"<color=#FFD700>{scrapCost} Scrap</color> ";
        if (manaCost > 0) costString += $"<color=#00FFFF>{manaCost} Mana</color>";
        costText.text = costString;

        int purchases = SkillTreeManager.Instance.GetPurchaseCount(nodeData.nodeID);
        bool maxReached = false;

        if (!nodeData.isRepeatable && purchases > 0)
        {
            purchaseCountText.text = "Gekauft!";
            maxReached = true;
        }
        else if (nodeData.isRepeatable)
        {
            if (nodeData.maxPurchases > 0)
            {
                purchaseCountText.text = $"{purchases} / {nodeData.maxPurchases}";
                if (purchases >= nodeData.maxPurchases) maxReached = true;
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
        if (nodeData != null)
        {
            SkillTreeManager.Instance.AttemptBuyNode(nodeData);
        }
    }
}