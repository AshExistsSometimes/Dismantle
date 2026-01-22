using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerWeapon
{
    Grapple,
    Revolver,
    Shotgun
}

public class PlayerWeaponManager : MonoBehaviour, ISaveable
{
    [Header("Weapon State")]
    public PlayerWeapon EquippedWeapon;

    [SerializeField]
    private PlayerWeapon lastRegularWeapon = PlayerWeapon.Revolver;

    [Header("Input")]
    public KeyCode GrappleKey = KeyCode.LeftShift;
    public KeyCode ParryKey = KeyCode.F;

    [Header("UI")]
    public Sprite GrappleIcon;
    public Sprite RevolverIcon;
    public Sprite ShotgunIcon;

    public HUDController hudController;

    [Header("Weapon Switching")]
    public float ScrollSwitchCooldown = 0.15f;

    private float lastScrollSwitchTime = -10f;

    [Header("Weapon Bools")]
    public bool GrappleEnabled = true;
    public bool ShotgunEnabled = true;
    public bool RevolverEnabled = true;

    private void Awake()
    {
        SaveManager.Instance.Register("Weapons", this);
    }

    private void Start()
    {
        UpdateHUDIcon();
        UpdateSecondaryHUDIcon();
    }

    private void Update()
    {
        HandleGrappleInput();
        HandleWeaponScroll();
    }

    // --------------------
    // Input Handling
    // --------------------

    private void HandleGrappleInput()
    {
        if (!GrappleEnabled) { return; }

        if (Input.GetKeyDown(GrappleKey))
        {
            if (EquippedWeapon != PlayerWeapon.Grapple)
            {
                lastRegularWeapon = EquippedWeapon;
                SetWeapon(PlayerWeapon.Grapple);
            }
        }

        if (Input.GetKeyUp(GrappleKey))
        {
            SetWeapon(lastRegularWeapon);
        }
    }

    private void HandleWeaponScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            if (Time.time - lastScrollSwitchTime < ScrollSwitchCooldown)
                return;

            lastScrollSwitchTime = Time.time;

            if (EquippedWeapon == PlayerWeapon.Revolver && ShotgunEnabled)
            {
                SetWeapon(PlayerWeapon.Shotgun);
            }
            else if (EquippedWeapon == PlayerWeapon.Shotgun && RevolverEnabled)
            {
                SetWeapon(PlayerWeapon.Revolver);
            }
        }

        if (scroll == 0f) return;
        if (EquippedWeapon == PlayerWeapon.Grapple) return;

    }

    private void HandleParryInput()
    {
        if (Input.GetKeyDown(ParryKey))
        {
            //  Tell PlayerCombat to parry
        }
    }

    // --------------------
    // Weapon Switching
    // --------------------

    public void SetWeapon(PlayerWeapon weapon)
    {
        EquippedWeapon = weapon;

        if (weapon != PlayerWeapon.Grapple)
            lastRegularWeapon = weapon;

        UpdateHUDIcon();
        UpdateSecondaryHUDIcon();
    }

    private void UpdateHUDIcon()
    {
        if (hudController == null || hudController.EquipmentIcon == null)
            return;

        switch (EquippedWeapon)
        {
            case PlayerWeapon.Grapple:
                hudController.EquipmentIcon.sprite = GrappleIcon;
                break;

            case PlayerWeapon.Revolver:
                hudController.EquipmentIcon.sprite = RevolverIcon;
                break;

            case PlayerWeapon.Shotgun:
                hudController.EquipmentIcon.sprite = ShotgunIcon;
                break;
        }
    }

    private void UpdateSecondaryHUDIcon()
    {
        if (hudController == null || hudController.SecondaryEquipmentIcon == null)
            return;

        if (EquippedWeapon == PlayerWeapon.Grapple)
        {
            if (lastRegularWeapon == PlayerWeapon.Revolver)
            {
                //hudController.SecondaryEquipmentIcon.color.a = 1f;// Needs to set alpha to 1
                hudController.SecondaryEquipmentIcon.sprite = RevolverIcon;
            }
            else if (lastRegularWeapon == PlayerWeapon.Shotgun)
            {
                //hudController.SecondaryEquipmentIcon.color.a = 1f;
                hudController.SecondaryEquipmentIcon.sprite = ShotgunIcon;
            }
        }   
        else
        {
            if (GrappleEnabled)
            {
                //hudController.SecondaryEquipmentIcon.color.a = 1f;
                hudController.SecondaryEquipmentIcon.sprite = GrappleIcon;
            }
            else
            {
                //hudController.SecondaryEquipmentIcon.color.a = 0f;// Needs to set alpha to 0
            }
        }
    }

    // Animate weapon that is being unequipped rotating down (shotgun, revolver)
    // animate weapon that is being equipped going up (shotgun, revolver)

    // --------------------
    // Saving
    // --------------------

    [System.Serializable]
    public class SaveData
    {
        public PlayerWeapon equippedWeapon;
        public PlayerWeapon lastRegularWeapon;
    }

    public object SaveState()
    {
        return new SaveData
        {
            equippedWeapon = EquippedWeapon,
            lastRegularWeapon = lastRegularWeapon
        };
    }

    public void LoadState(object data)
    {
        var save = (SaveData)data;
        lastRegularWeapon = save.lastRegularWeapon;
        SetWeapon(save.equippedWeapon);
    }
}
