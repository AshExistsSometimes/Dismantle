using UnityEngine;

public class TutorialSetup : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueSO Dialogue;

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
        weaponManager.SetWeapon(PlayerWeapon.Revolver);

        weaponManager.EquipmentEnabled = false;

        ShotgunEquipped(false);
        GrappleEquipped(false);
        RevolverEquipped(false);  
    }


    public void ShotgunEquipped(bool equipped)
    {
        TryEnableEquipment();

        weaponManager.ShotgunEnabled = equipped;
        weaponManager.RefreshWeaponUI();
    }

    public void RevolverEquipped(bool equipped)
    {
        TryEnableEquipment();

        weaponManager.RevolverEnabled = equipped;
        weaponManager.RefreshWeaponUI();
    }

    public void GrappleEquipped(bool equipped)
    {
        TryEnableEquipment();

        weaponManager.GrappleEnabled = equipped;
        weaponManager.RefreshWeaponUI();
    }

    public void TryEnableEquipment()
    {
        if (!weaponManager.EquipmentEnabled)
        {
            weaponManager.EquipmentEnabled = true;
        }
    }
}
