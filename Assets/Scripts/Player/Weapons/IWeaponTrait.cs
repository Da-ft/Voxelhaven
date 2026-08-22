using UnityEngine;

public interface IWeaponTrait
{
    // Wird aufgerufen, BEVOR die Basis-Waffe ihren Angriff macht
    void OnPreAttack(PlayerController player, WeaponInstance instance, Transform currentTarget);

    // Wird aufgerufen, NACHDEM die Basis-Waffe ihren Angriff gemacht hat
    void OnPostAttack(PlayerController player, WeaponInstance instance, Transform currentTarget);
}
