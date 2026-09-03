using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Meta Progression State")]
    [SerializeField] private int metaCurrencyAmount = 0;
    public int MetaCurrencyAmount => metaCurrencyAmount;

    // Speichert global ALLE Käufe (Meta + aktuellen Run)
    private Dictionary<string, int> upgradePurchaseCounts = new Dictionary<string, int>();

    // Trackt, welche IDs zu InRun-Upgrades gehören, um sie am Ende des Runs zu löschen
    private List<string> activeInRunUpgrades = new List<string>();

    private List<IWeaponTrait> unlockedTraits = new List<IWeaponTrait>();

    // Event für UI (ShopButton, Meta-Menü etc.)
    public event Action OnUpgradeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.OnLoadData += LoadData;
            DataPersistenceManager.instance.OnSaveData += SaveData;
        }
    }

    private void OnDisable()
    {
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.OnLoadData -= LoadData;
            DataPersistenceManager.instance.OnSaveData -= SaveData;
        }
    }

    private void LoadData(GameData data)
    {
        metaCurrencyAmount = data.metaCurrencyAmount;

        upgradePurchaseCounts.Clear();

        // Save Meta Upgrades to Dictionary
        foreach (var entry in data.metaUpgrades)
        {
            upgradePurchaseCounts[entry.upgradeID] = entry.purchaseCount;
        }

        Debug.Log($"[UpgradeNabager] Meta Currency ({metaCurrencyAmount}) & {data.metaCurrencyAmount}");
        OnUpgradeChanged?.Invoke();
    }

    private void SaveData(GameData data)
    {
        data.metaCurrencyAmount = metaCurrencyAmount;
        data.metaUpgrades.Clear();

        // Persistent Meta Upgrades to Json (Ignore In Run Upgrades for now)
        foreach (var kvp in upgradePurchaseCounts)
        {
            if (!activeInRunUpgrades.Contains(kvp.Key))
            {
                data.metaUpgrades.Add(new MetaUpgradeSaveEntry
                {
                    upgradeID = kvp.Key,
                    purchaseCount = kvp.Value
                });
            }
        }
    }

    public void AddMetaCurrency(int amount)
    {
        metaCurrencyAmount += amount;
        OnUpgradeChanged?.Invoke();
    }

    public void AttemptBuyUpgrade(UpgradeDataSO upgrade)
    {
        if (upgrade == null) return;

        int currentPurchases = GetPurchaseCount(upgrade.upgradeID);

        // 1. Checks
        if (!upgrade.isRepeatable && currentPurchases > 0)
        {
            Debug.Log($"[UpgradeManager] '{upgrade.displayName}' ist einmalig.");
            return;
        }

        if (upgrade.maxPurchases > 0 && currentPurchases >= upgrade.maxPurchases)
        {
            Debug.Log($"[UpgradeManager] Limit für '{upgrade.displayName}' erreicht.");
            return;
        }

        if (upgrade.parentNodes != null)
        {
            foreach (UpgradeDataSO parent in upgrade.parentNodes)
            {
                if (GetPurchaseCount(parent.upgradeID) == 0) return; // Voraussetzungen fehlen
            }
        }

        // 2. Bezahlung
        if (upgrade.nodeType == NodeType.InRunUpgrade)
        {
            int currentCycle = GameManager.Instance != null ? GameManager.Instance.CycleCounter : 1;
            var (scrapCost, manaCost) = upgrade.GetScaledCosts(currentCycle);

            if (GameManager.Instance.Scrap < scrapCost || GameManager.Instance.Mana < manaCost) return;

            if (scrapCost > 0) GameManager.Instance.StealScrap(scrapCost);
            if (manaCost > 0) GameManager.Instance.AddMana(-manaCost);

            // Für den Run-Reset merken
            if (!activeInRunUpgrades.Contains(upgrade.upgradeID))
            {
                activeInRunUpgrades.Add(upgrade.upgradeID);
            }
        }
        else if (upgrade.nodeType == NodeType.MetaProgression)
        {
            if (metaCurrencyAmount < upgrade.metaResourceCost) return;
            metaCurrencyAmount -= upgrade.metaResourceCost;
        }

        // 3. Kauf abschließen
        upgradePurchaseCounts[upgrade.upgradeID] = currentPurchases + 1;

        // Nutze den Singleton deiner Player-Klasse
        Player player = Player.Instance;
        if (player != null)
        {
            upgrade.ApplyEffects(player);
        }
        else
        {
            Debug.LogError("[UpgradeManager] Player.Instance wurde nicht gefunden!");
        }

        Debug.Log($"<color=green>[UpgradeManager] Gekauft: {upgrade.displayName}</color>");
        OnUpgradeChanged?.Invoke();
    }

    /// <summary>
    /// Wird vom GameManager nach Ende eines Runs / Tod des Spielers aufgerufen.
    /// </summary>
    public void ResetRunUpgrades()
    {
        foreach (string id in activeInRunUpgrades)
        {
            if (upgradePurchaseCounts.ContainsKey(id))
            {
                upgradePurchaseCounts.Remove(id);
            }
        }
        activeInRunUpgrades.Clear();

        Debug.Log("[UpgradeManager] In-Run Upgrades zurückgesetzt.");
        OnUpgradeChanged?.Invoke();
    }

    public int GetPurchaseCount(string id)
    {
        return upgradePurchaseCounts.TryGetValue(id, out int count) ? count : 0;
    }

    public void RegisterUnlockedTrait(IWeaponTrait trait)
    {
        if (!unlockedTraits.Contains(trait)) unlockedTraits.Add(trait);
    }

    public List<IWeaponTrait> GetUnlockedTraits() => unlockedTraits;
}