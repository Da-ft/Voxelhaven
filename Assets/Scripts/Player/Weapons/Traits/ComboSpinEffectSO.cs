using UnityEngine;

[CreateAssetMenu(fileName = "ComboSpinEffect", menuName = "Shop Upgrade Effects/Combo Spin")]
public class ComboSpinEffectSO : UpgradeEffectSO
{
    [Header("Spin Settings")]
    public int hitsRequired = 3;
    public float spinRadius = 4f;
    public float comboDamageMultiplier = 1.5f;

    [Header("Visuals")]
    [Tooltip("Das VFX Prefab für den Rundumschlag.")]
    public GameObject spinVfxPrefab;
    [Tooltip("Größen-Multiplikator für den Effekt (z.B. 2.0 für doppelte Größe).")]
    public float vfxScaleMultiplier = 1.5f;

    public override void Execute(Player player)
    {
        // Prefab an den Konstruktor übergeben
        ComboSpinTrait spinTrait = new ComboSpinTrait(hitsRequired, spinRadius, comboDamageMultiplier, spinVfxPrefab, vfxScaleMultiplier);

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.RegisterUnlockedTrait(spinTrait);
        }

        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller != null)
        {
            controller.AddTraitToActiveWeapon(spinTrait);
        }
    }
}
