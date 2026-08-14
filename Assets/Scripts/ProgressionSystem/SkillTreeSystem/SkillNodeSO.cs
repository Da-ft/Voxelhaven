using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Skill Node", menuName = "Skill Tree/Skill Node")]
public class SkillNodeSO : ScriptableObject
{
    [Header("Identifikation")]
    [Tooltip("Definitiv ID (eg. 'node_scythe_dmg_1'). Important for Saves!")]
    public string nodeID;
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Prerequisites")]
    [Tooltip("List of Nodes which needs to be bought before Unlocking this one. Leave Empty to make it always available.")]
    public List<SkillNodeSO> parentNodes;

    [Header("Payload (Effects)")]
    [Tooltip("What happens when those Nodes get bought.")]
    public List<SkillEffectSO> effects;

    [Header("In-Run Cost (UpgradeShop)")]
    public int baseScrapCost = 10;
    public int baseManaCost = 0;

    [Header("Meta Progression Cost")]
    public int metaResourceCost = 50;

    [Tooltip("X = CycleCounter, Y = Mult (z.B. 1.0 bei Cycle 1, 2.5 bei Cycle 5). If no definition, Fallback mult is 1!")]
    public AnimationCurve costScalingCurve = AnimationCurve.Linear(1, 1, 10, 5);

    public (int scrap, int mana) GetScaledCosts(int currentCycle)
    {
        // If no Curve, use 1
        float multiplier = (costScalingCurve != null && costScalingCurve.length > 0)
            ? costScalingCurve.Evaluate(currentCycle)
            : 1f;

        int finalScrap = Mathf.RoundToInt(baseScrapCost * multiplier);
        int finalMana = Mathf.RoundToInt(baseManaCost * multiplier);

        return (finalScrap, finalMana);
    }

    public enum NodeType
    {
        InRunUpgrade,
        MetaProgression
    }

    [Header("Behavior")]
    public NodeType nodeType = NodeType.InRunUpgrade;

    [Tooltip("Can be bought Repeatable Yes/No.")]
    public bool isRepeatable = false;

    [Tooltip("Max purchase Count. 0 = Infinite purchases.")]
    public int maxPurchases = 0;
}