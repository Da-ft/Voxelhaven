using UnityEngine;
using UnityEditor;
using Codice.Client.Commands;
using System.IO.Ports;

[CustomEditor(typeof(UpgradeDataSO))]

public class UpgradeDataEditor : Editor
{
    // Allgemeine Info
    SerializedProperty upgradeIDProp;
    SerializedProperty displayNameProp;
    SerializedProperty descriptionProp;
    SerializedProperty iconProp;

    // Shop & Drafting
    SerializedProperty nodeTypeProp;
    SerializedProperty upgradeRarityProp;
    SerializedProperty dropWeightProp;
    SerializedProperty isUnlockedForShopProp;

    // Kauf Regeln
    SerializedProperty isRepeatableProp;
    SerializedProperty maxPurchasesProp;
    SerializedProperty parentNodesProp;

    // In Run Cost Stuff
    SerializedProperty baseScrapCostProp;
    SerializedProperty baseManaCostProp;
    SerializedProperty costScalingCurveProp;

    // Meta Progression
    SerializedProperty metaResourceCostProp;

    // Payload 1 - raw Stats
    SerializedProperty statModifiersProp;

    // Payload 2 - special effects
    SerializedProperty specialEffectsProp;

    void OnEnable()
    {
        upgradeIDProp = serializedObject.FindProperty("upgradeID");
        displayNameProp = serializedObject.FindProperty("displayName");
        descriptionProp = serializedObject.FindProperty("description");
        iconProp = serializedObject.FindProperty("icon");

        nodeTypeProp = serializedObject.FindProperty("nodeType");
        upgradeRarityProp = serializedObject.FindProperty("rarity");
        dropWeightProp = serializedObject.FindProperty("dropWeight");
        isUnlockedForShopProp = serializedObject.FindProperty("isUnlockedForShop");

        isRepeatableProp = serializedObject.FindProperty("isRepeatable");
        maxPurchasesProp = serializedObject.FindProperty("maxPurchases");
        parentNodesProp = serializedObject.FindProperty("parentNodes");

        baseScrapCostProp = serializedObject.FindProperty("baseScrapCost");
        baseManaCostProp = serializedObject.FindProperty("baseManaCost");
        costScalingCurveProp = serializedObject.FindProperty("costScalingCurve");

        metaResourceCostProp = serializedObject.FindProperty("metaResourceCost");

        statModifiersProp = serializedObject.FindProperty("statModifiers");

        specialEffectsProp = serializedObject.FindProperty("specialEffects");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(nodeTypeProp);
        EditorGUILayout.Space();

        NodeType currentType = (NodeType)nodeTypeProp.enumValueIndex;

        if (currentType == NodeType.InRunUpgrade)
        {
            // In Run Upgrades
            EditorGUILayout.PropertyField(upgradeIDProp);
            EditorGUILayout.PropertyField(displayNameProp);
            EditorGUILayout.PropertyField(descriptionProp);
            EditorGUILayout.PropertyField(iconProp);

            EditorGUILayout.PropertyField(upgradeRarityProp);
            EditorGUILayout.PropertyField(dropWeightProp);
            EditorGUILayout.PropertyField(isUnlockedForShopProp);

            EditorGUILayout.PropertyField(isRepeatableProp);
            EditorGUILayout.PropertyField(maxPurchasesProp);

            EditorGUILayout.PropertyField(baseScrapCostProp);
            EditorGUILayout.PropertyField(baseManaCostProp);
            EditorGUILayout.PropertyField(costScalingCurveProp);

            EditorGUILayout.PropertyField(statModifiersProp);
            EditorGUILayout.PropertyField(specialEffectsProp);

        }
        else if (currentType == NodeType.MetaProgression)
        {
            // Meta Progression Upgrades
            EditorGUILayout.PropertyField(upgradeIDProp);
            EditorGUILayout.PropertyField(displayNameProp);
            EditorGUILayout.PropertyField(descriptionProp);
            EditorGUILayout.PropertyField(iconProp);

            EditorGUILayout.PropertyField(isRepeatableProp);
            EditorGUILayout.PropertyField(maxPurchasesProp);

            EditorGUILayout.PropertyField(metaResourceCostProp);

            EditorGUILayout.PropertyField(statModifiersProp);
            EditorGUILayout.PropertyField(specialEffectsProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
