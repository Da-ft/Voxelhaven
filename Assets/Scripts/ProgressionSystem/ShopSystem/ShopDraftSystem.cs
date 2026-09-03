using System.Collections.Generic;
using UnityEngine;

public class ShopDraftSystem : MonoBehaviour
{
    [Header("Shop Settings")]
    [Tooltip("Die Datenbank aller existierenden Upgrades im Spiel.")]
    [SerializeField] private List<UpgradeDataSO> allAvailableUpgrades;

    [Tooltip("Wie viele Karten sollen pro Shop-Besuch ausgewürfelt werden?")]
    [SerializeField] private int cardsToDraft = 3;

    [Header("Reroll Settings")]
    [Tooltip("Base cost for the first Reroll")]
    private int baseRerollCost = 10;
    [Tooltip("X-Axis: Count of Current Rerolls, Y-Axis: Mult for Base Cost")]
    [SerializeField] private AnimationCurve rerollCostCurve = AnimationCurve.Linear(0, 1, 10, 5);
    private int currentRerolls = 0;

    private void Awake()
    {
        UpgradeDataSO[] loadedUpgrades = Resources.LoadAll<UpgradeDataSO>("UpgradeSO");
        allAvailableUpgrades = new List<UpgradeDataSO>(loadedUpgrades);

        Debug.Log($"[ShopDraftManager] {allAvailableUpgrades.Count} Upgrades zur Laufzeit aus Resources geladen.");
    }

    public int GetCurrentRerollCost()
    {
        float multiplier = rerollCostCurve.Evaluate(currentRerolls);
        return Mathf.RoundToInt(baseRerollCost * multiplier);
    }

    public void RegisterReroll()
    {
        currentRerolls++;
    }

    // Generiert eine Liste an zufälligen, kaufbaren Upgrades basierend auf ihrem dropWeight
        public List<UpgradeDataSO> GenerateShopDraft()
    {
        // 1. Hole alle Karten, die der Spieler überhaupt kaufen DARF
        List<UpgradeDataSO> validPool = GetValidUpgrades();
        List<UpgradeDataSO> draftedCards = new List<UpgradeDataSO>();

        int draftCount = Mathf.Min(cardsToDraft, validPool.Count);

        // 2. Ziehe N Karten aus dem Pool
        for (int i = 0; i < draftCount; i++)
        {
            UpgradeDataSO chosenUpgrade = PickRandomWeighted(validPool);

            if (chosenUpgrade != null)
            {
                draftedCards.Add(chosenUpgrade);

                // Verhindert, dass dieselbe Karte zweimal im selben Shop-Fenster auftaucht
                validPool.Remove(chosenUpgrade);
            }
        }

        return draftedCards;
    }

    // Filtert alle Upgrades heraus, die gesperrt sind, deren Max-Level erreicht ist oder deren Voraussetzungen fehlen
    private List<UpgradeDataSO> GetValidUpgrades()
    {
        List<UpgradeDataSO> valid = new List<UpgradeDataSO>();

        foreach (var upgrade in allAvailableUpgrades)
        {
            // Ist die Karte überhaupt für den Auslosungs-Pool freigegeben?
            if (!upgrade.isUnlockedForShop) continue;

            // Ist es ein In-Run oder Meta-Progression Upgrade? (Shop draftet nur In-Run!)
            if (upgrade.nodeType != NodeType.InRunUpgrade) continue;

            // Kauflimits prüfen
            int currentPurchases = UpgradeManager.Instance.GetPurchaseCount(upgrade.upgradeID);
            if (!upgrade.isRepeatable && currentPurchases > 0) continue;
            if (upgrade.maxPurchases > 0 && currentPurchases >= upgrade.maxPurchases) continue;

            // Voraussetzungen (Parent Nodes) prüfen
            bool parentsValid = true;
            if (upgrade.parentNodes != null && upgrade.parentNodes.Count > 0)
            {
                foreach (var parent in upgrade.parentNodes)
                {
                    if (UpgradeManager.Instance.GetPurchaseCount(parent.upgradeID) == 0)
                    {
                        parentsValid = false;
                        break;
                    }
                }
            }
            if (!parentsValid) continue;

            // Wenn alle Checks bestanden sind, ab in den Lostopf!
            valid.Add(upgrade);
        }

        return valid;
    }

    // Adaptiert aus der WaveManager-Logik: Wählt ein Upgrade anhand der Gewichte aus
    private UpgradeDataSO PickRandomWeighted(List<UpgradeDataSO> pool)
    {
        int totalWeight = 0;

        // Gesamtes Gewicht aller aktuellen Karten im Topf berechnen
        foreach (var upgrade in pool)
        {
            totalWeight += upgrade.dropWeight;
        }

        if (totalWeight <= 0) return null;

        // Zufallswert ziehen, analog zum WaveManager
        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        // Die Karte finden, in deren "Gewichts-Bereich" der Zufallswert fällt
        foreach (var upgrade in pool)
        {
            cumulativeWeight += upgrade.dropWeight;
            if (randomValue < cumulativeWeight)
            {
                return upgrade;
            }
        }

        return null; // Fallback, sollte theoretisch nie erreicht werden
    }
}