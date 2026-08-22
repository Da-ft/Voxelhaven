using UnityEngine;

public abstract class UpgradeEffectSO : ScriptableObject
{
    public abstract void Execute(Player player);
}