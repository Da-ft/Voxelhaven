using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class BeamWeaponController : MonoBehaviour
{
    [Header("VFX References")]
    public VisualEffect vfxGraph;

    public bool IsReadyToFire { get; private set; } = false;

    private bool isWindingUp = false;
    private float windupStartTime = 0f;
    private float lastTickTime = 0f;

    private float disconnectTimeout = 0.2f;

    private void Update()
    {
        // stop ray without input
        if ((isWindingUp || IsReadyToFire) && Time.time - lastTickTime > disconnectTimeout)
        {
            ResetBeam();
        }
    }

    public void RegisterAttackTick(float requiredWindup)
    {
        float currentTime = Time.time;
        lastTickTime = currentTime;

        if (!isWindingUp && !IsReadyToFire)
        {
            isWindingUp = true;
            windupStartTime = currentTime;
            if (vfxGraph != null) vfxGraph.SendEvent("OnWindup");
        }

        // check if windup is done
        if (isWindingUp && (currentTime - windupStartTime) >= requiredWindup)
        {
            isWindingUp = false;
            IsReadyToFire = true;
            if (vfxGraph != null) vfxGraph.SendEvent("OnFire");
        }
    }

    public void UpdateBeamVisuals(float length)
    {
        if (vfxGraph != null)
        {
            vfxGraph.SetFloat("BeamLength", length);
        }
    }

    private void ResetBeam()
    {
        isWindingUp = false;
        IsReadyToFire = false;
        if (vfxGraph != null) vfxGraph.SendEvent("OnStop");
    }
}
