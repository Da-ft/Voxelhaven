using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUpgradeButton : MonoBehaviour
{
    [Header("UI Referenzen")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI purchaseCountText; // Zeigt z.B. "2/5 Gekauft"
    [SerializeField] private Image iconImage;
    [SerializeField] private Button buyButton;

    [Header("Die Daten")]
    [Tooltip("Ziehe hier im Editor die SkillNodeSO-Karteikarte rein, die dieser Button verkaufen soll.")]
    [SerializeField] private SkillNodeSO nodeData;

    private void OnEnable()
    {
        // Wenn das UI-Fenster (RestScreen) aufgeht, abonnieren wir uns auf Änderungen
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.OnSkillTreeChanged += UpdateVisuals;
        }

        // Direkter visueller Refresh beim Einblenden
        UpdateVisuals();
    }

    private void OnDisable()
    {
        // WICHTIG: Immer abmelden, wenn der Button unsichtbar wird
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

        // 1. Statische Daten aus der Karteikarte laden
        titleText.text = nodeData.displayName;
        descriptionText.text = nodeData.description;

        if (nodeData.icon != null)
        {
            iconImage.sprite = nodeData.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false; // Fallback, falls kein Icon zugewiesen wurde
        }

        // 2. Dynamische Kosten berechnen (basierend auf CycleCounter aus dem GameManager)
        int currentCycle = GameManager.Instance.CycleCounter;
        var (scrapCost, manaCost) = nodeData.GetScaledCosts(currentCycle);

        string costString = "";
        if (scrapCost > 0) costString += $"<color=#FFD700>{scrapCost} Scrap</color> ";
        if (manaCost > 0) costString += $"<color=#00FFFF>{manaCost} Mana</color>";
        costText.text = costString;

        // 3. Kauf-Zähler überprüfen
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

        // 4. Prüfen, ob der Spieler genug Ressourcen hat
        bool hasEnoughScrap = GameManager.Instance.Scrap >= scrapCost;
        bool hasEnoughMana = GameManager.Instance.Mana >= manaCost;

        // 5. Button-Interaktion steuern (Gesperrt, wenn pleite oder Max-Level erreicht)
        buyButton.interactable = hasEnoughScrap && hasEnoughMana && !maxReached;
    }

    /// <summary>
    /// Diese Funktion verknüpfen wir im Unity-Editor mit dem OnClick()-Event des Buttons.
    /// </summary>
    public void OnBuyButtonClicked()
    {
        if (nodeData != null)
        {
            // Leitet den Kauf-Versuch an den Manager weiter
            SkillTreeManager.Instance.AttemptBuyNode(nodeData);
        }
    }
}