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

    [Header("UI")]
    public Sprite GrappleIcon;
    public Sprite RevolverIcon;
    public Sprite ShotgunIcon;

    public HUDController hudController;

    [Header("Weapon Switching")]
    public float ScrollSwitchCooldown = 0.15f;

    private float lastScrollSwitchTime = -10f;

    private void Awake()
    {
        SaveManager.Instance.Register("Weapons", this);
    }

    private void Start()
    {
        UpdateHUDIcon();
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

            if (scroll > 0f)
            {
                SetWeapon(PlayerWeapon.Shotgun);
            }
            else if (scroll < 0f)
            {
                SetWeapon(PlayerWeapon.Revolver);
            }
        }

        if (scroll == 0f) return;
        if (EquippedWeapon == PlayerWeapon.Grapple) return;

        PlayerWeapon newWeapon =
            EquippedWeapon == PlayerWeapon.Revolver
                ? PlayerWeapon.Shotgun
                : PlayerWeapon.Revolver;

        SetWeapon(newWeapon);
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
