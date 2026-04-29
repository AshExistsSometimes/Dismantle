using UnityEngine;
using UnityEngine.Audio;

public class TutorialSetup : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueSO Dialogue;

    public AudioSource audioSource;
    public AudioClip ItemPickup_SFX;

    private PlayerWeaponManager weaponManager;

    void Start()
    {
        weaponManager = PlayerWeaponManager.Instance;

        UnequipWeapons();

        DialogueManager.Instance.InHub = false;

        if (Dialogue != null)
        {
            DialogueManager.Instance.StartDialogue(Dialogue);
        }

        weaponManager.GrappleEnabled = false;
        weaponManager.RefreshWeaponUI();
    }

    public void UnequipWeapons()
    {
        PlayerWeaponManager.Instance.ForceDisableAllWeapons();

        ShotgunEquipped(false);
        GrappleEquipped(false);
        RevolverEquipped(false);
    }

    // PICKUP LOGIC //
    public void ShotgunEquipped(bool equipped)
    {
        weaponManager.ShotgunEnabled = equipped;

        audioSource.PlayOneShot(ItemPickup_SFX);

        if (equipped)
        {
            weaponManager.EnableStartingWeapon(PlayerWeapon.Shotgun);
        }

        weaponManager.RefreshWeaponUI();
    }

    public void RevolverEquipped(bool equipped)
    {
        weaponManager.RevolverEnabled = equipped;

        audioSource.PlayOneShot(ItemPickup_SFX);

        if (equipped)
        {
            weaponManager.EnableStartingWeapon(PlayerWeapon.Revolver);
        }

        weaponManager.RefreshWeaponUI();
    }

    public void GrappleEquipped(bool equipped)
    {
        TryEnableEquipment();

        audioSource.PlayOneShot(ItemPickup_SFX);

        weaponManager.GrappleEnabled = equipped;
        weaponManager.RefreshWeaponUI();
    }

    // END OF PICKUP LOGIC
    public void TryEnableEquipment()
    {
        if (!weaponManager.EquipmentEnabled)
        {
            weaponManager.EquipmentEnabled = true;
        }
    }
}
