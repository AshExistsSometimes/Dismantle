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

        ShotgunEquipped(false);
        GrappleEquipped(false);
    }


    public void ShotgunEquipped(bool equipped)
    {
        weaponManager.ShotgunEnabled = equipped;
        weaponManager.RefreshWeaponUI();
    }

    public void GrappleEquipped(bool equipped)
    {
        weaponManager.GrappleEnabled = equipped;
        weaponManager.RefreshWeaponUI();
    }
}
