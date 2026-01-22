using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public PlayerWeaponManager weaponManager;

    [Header("Weapons")]
    public GameObject Revolver;
    public GameObject Shotgun;
    public GameObject Sword; // Reference only rn

    private void Update()
    {
        UpdateWeaponVisibility();
    }

    private void UpdateWeaponVisibility()
    {
        if (weaponManager == null)
            return;

        bool revolverActive = weaponManager.EquippedWeapon == PlayerWeapon.Revolver;
        bool shotgunActive = weaponManager.EquippedWeapon == PlayerWeapon.Shotgun;

        if (Revolver != null)
            Revolver.SetActive(revolverActive);

        if (Shotgun != null)
            Shotgun.SetActive(shotgunActive);

        // Grapple disables both guns
        if (weaponManager.EquippedWeapon == PlayerWeapon.Grapple)
        {
            if (Revolver != null) Revolver.SetActive(false);
            if (Shotgun != null) Shotgun.SetActive(false);
        }
    }
}
