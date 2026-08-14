using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance { get; private set; }

    [Header("Meta Progression State")]
    [SerializeField] private int metaCurrencyAmount = 0;

    public int MetaCurrencyAmount => metaCurrencyAmount;

    private Dictionary<string, int> nodePurchaseCounts = new Dictionary<string, int>();

    private List<IWeaponTrait> unlockedTraits = new List<IWeaponTrait>();

    public event Action OnSkillTreeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AttemptBuyNode(SkillNodeSO node)
    {
        if (node == null) return;

        int currentPurchases = GetPurchaseCount(node.nodeID);

        // Single Purchase Only Node Check
        if (!node.isRepeatable && currentPurchases > 0)
        {
            Debug.Log($"[SkillTree] '{node.displayName}' ist ein einmaliges Upgrade und wurde bereits gekauft.");
            return;
        }

        // Purchase Limit capped?
        if (node.maxPurchases > 0 && currentPurchases >= node.maxPurchases)
        {
            Debug.Log($"[SkillTree] Maximum an Käufen ({node.maxPurchases}) für '{node.displayName}' erreicht.");
            return;
        }

        // Fulfilled Prerequisit?
        if (node.parentNodes != null)
        {
            foreach (SkillNodeSO parent in node.parentNodes)
            {
                if (GetPurchaseCount(parent.nodeID) == 0)
                {
                    Debug.Log($"[SkillTree] Kauf abgelehnt: Voraussetzung '{parent.displayName}' fehlt.");
                    return;
                }
            }
        }

        // In Run Upgrade purchase
        if (node.nodeType == SkillNodeSO.NodeType.InRunUpgrade)
        {
            int currentCycle = GameManager.Instance != null ? GameManager.Instance.CycleCounter : 1;
            var (scrapCost, manaCost) = node.GetScaledCosts(currentCycle);

            if (GameManager.Instance.Scrap < scrapCost || GameManager.Instance.Mana < manaCost)
            {
                Debug.Log($"[Shop] Nicht genug Scrap/Mana für '{node.displayName}'!");
                return;
            }

            if (scrapCost > 0) GameManager.Instance.StealScrap(scrapCost);
            if (manaCost > 0) GameManager.Instance.AddMana(-manaCost);
        }
        // Meta Progression purchase
        else if (node.nodeType == SkillNodeSO.NodeType.MetaProgression)
        {
            if (metaCurrencyAmount < node.metaResourceCost)
            {
                Debug.Log($"[MetaProgression] Nicht genug Meta-Ressourcen! ({metaCurrencyAmount}/{node.metaResourceCost})");
                return;
            }

            metaCurrencyAmount -= node.metaResourceCost;
        }

        // Increment purchase count
        nodePurchaseCounts[node.nodeID] = currentPurchases + 1;

        // Resolve Effect
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (node.effects != null)
        {
            foreach (SkillEffectSO effect in node.effects)
            {
                effect.Execute(controller);
            }
        }

        Debug.Log($"<color=green>[SkillTree] Gekauft: {node.displayName} (Kauf #{nodePurchaseCounts[node.nodeID]})</color>");

        OnSkillTreeChanged?.Invoke();
    }

    public void ResetRunUpgrades()
    {
        Debug.Log("[SkillTree] In-Run Upgrades wurden für den neuen Run zurückgesetzt.");
        OnSkillTreeChanged?.Invoke();
    }

    public int GetPurchaseCount(string nodeID)
    {
        return nodePurchaseCounts.TryGetValue(nodeID, out int count) ? count : 0;
    }

    public bool IsNodeUnlocked(string nodeID)
    {
        return GetPurchaseCount(nodeID) > 0;
    }

    public void RegisterUnlockedTrait(IWeaponTrait trait)
    {
        if (!unlockedTraits.Contains(trait))
        {
            unlockedTraits.Add(trait);
        }
    }

    public List<IWeaponTrait> GetUnlockedTraits()
    {
        return unlockedTraits;
    }

    public void AddMetaCurrency(int amount)
    {
        metaCurrencyAmount += amount;
        OnSkillTreeChanged?.Invoke();
    }
}