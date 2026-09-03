using System.Collections.Generic;
using System;

[Serializable]
public struct MetaUpgradeSaveEntry
{
    public string upgradeID;
    public int purchaseCount;
}

[Serializable]
public class GameData
{
    public int cycleCount;
    public int metaCurrencyAmount;
    public List<MetaUpgradeSaveEntry> metaUpgrades = new List<MetaUpgradeSaveEntry>();

    // Constructor for new SaveGame
    public GameData()
    {
        cycleCount = 1;
        metaCurrencyAmount = 0;
        metaUpgrades = new List<MetaUpgradeSaveEntry>();
    }
}
