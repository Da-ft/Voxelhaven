using UnityEngine;

public abstract class SkillEffectSO : ScriptableObject
{
    [Tooltip("Note for Editor!")]
    public string editorDescription;

    public abstract void Execute(PlayerController player);
}
