using System;
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

    [Header("Weapon GameObjects")]
    public GameObject RevolverObject;
    public GameObject ShotgunObject;
    public GameObject SwordObject;

    // --------------------
    // Weapon Animation
    // --------------------

    [Header("Weapon Animation")]
    public Transform RevolverPivot;
    public Transform ShotgunPivot;

    [Tooltip("0 = unequipped, 1 = equipped (supports overshoot)")]
    public AnimationCurve EquipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float EquipDuration = 0.25f;
    public Vector3 EquippedRotation = Vector3.zero;
    public Vector3 UnequippedRotation = new Vector3(-90f, 0f, 0f);

    private float weaponAnimTime;
    private PlayerWeapon animatingWeapon;
    private bool animatingEquip;
    private bool isAnimating;

    private PlayerWeapon pendingDisableWeapon = PlayerWeapon.Grapple;
    private bool disableOnAnimEnd;

    private void Start()
    {
        UpdateHUDIcon();
        UpdateSecondaryHUDIcon();

        ApplyImmediateRotation(PlayerWeapon.Revolver, EquippedWeapon == PlayerWeapon.Revolver);
        ApplyImmediateRotation(PlayerWeapon.Shotgun, EquippedWeapon == PlayerWeapon.Shotgun);

        // Ensure only the equipped weapon is visible at start
        EnableWeaponObject(EquippedWeapon);
        foreach (PlayerWeapon w in new PlayerWeapon[] { PlayerWeapon.Revolver, PlayerWeapon.Shotgun })
        {
            if (w != EquippedWeapon)
                DisableWeaponObject(w);
        }

        SaveManager.Instance.Register("Weapons", this);
    }

    private void Update()
    {
        HandleGrappleInput();
        HandleWeaponScroll();
        UpdateWeaponAnimation();
    }

    // --------------------
    // Input Handling
    // --------------------

    private void HandleGrappleInput()
    {
        if (!GrappleEnabled) return;

        if (Input.GetKeyDown(GrappleKey))
        {
            if (EquippedWeapon != PlayerWeapon.Grapple)
            {
                lastRegularWeapon = EquippedWeapon;
                StartUnequip(EquippedWeapon);
                SetWeapon(PlayerWeapon.Grapple);
            }
        }

        if (Input.GetKeyUp(GrappleKey))
        {
            SetWeapon(lastRegularWeapon);
            StartEquip(lastRegularWeapon);
        }
    }

    private void HandleWeaponScroll()
    {
        if (EquippedWeapon == PlayerWeapon.Grapple)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        if (Time.time - lastScrollSwitchTime < ScrollSwitchCooldown)
            return;

        lastScrollSwitchTime = Time.time;

        if (EquippedWeapon == PlayerWeapon.Revolver && ShotgunEnabled)
        {
            StartUnequip(PlayerWeapon.Revolver);
            SetWeapon(PlayerWeapon.Shotgun);
            StartEquip(PlayerWeapon.Shotgun);
        }
        else if (EquippedWeapon == PlayerWeapon.Shotgun && RevolverEnabled)
        {
            StartUnequip(PlayerWeapon.Shotgun);
            SetWeapon(PlayerWeapon.Revolver);
            StartEquip(PlayerWeapon.Revolver);
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

    // --------------------
    // Weapon Animation Logic
    // --------------------

    private void StartEquip(PlayerWeapon weapon)
    {
        if (weapon == PlayerWeapon.Grapple) return;

        // Disable all other weapons immediately except the one being equipped
        foreach (PlayerWeapon w in new PlayerWeapon[] { PlayerWeapon.Revolver, PlayerWeapon.Shotgun })
        {
            if (w != weapon)
                DisableWeaponObject(w);
        }

        // Enable the weapon we're equipping
        EnableWeaponObject(weapon);

        animatingWeapon = weapon;
        animatingEquip = true;
        weaponAnimTime = 0f;
        isAnimating = true;
    }

    private void StartUnequip(PlayerWeapon weapon)
    {
        if (weapon == PlayerWeapon.Grapple) return;

        animatingWeapon = weapon;
        animatingEquip = false;
        weaponAnimTime = 0f;
        isAnimating = true;

        pendingDisableWeapon = weapon;
        disableOnAnimEnd = true;
    }

    private void UpdateWeaponAnimation()
    {
        if (!isAnimating) return;

        weaponAnimTime += Time.deltaTime;
        float t = Mathf.Clamp01(weaponAnimTime / EquipDuration);

        float curveT = animatingEquip ? t : 1f - t;
        float curveValue = EquipCurve.Evaluate(curveT);

        Transform pivot = GetPivot(animatingWeapon);
        if (pivot != null)
        {
            // Rotate between UnequippedRotation -> EquippedRotation
            pivot.localRotation = Quaternion.Euler(
                Vector3.LerpUnclamped(
                    UnequippedRotation,
                    EquippedRotation,
                    curveValue
                )
            );
        }

        // **Disable the weapon only after unequip animation finishes**
        if (t >= 1f)
        {
            isAnimating = false;

            if (!animatingEquip && disableOnAnimEnd)
            {
                DisableWeaponObject(pendingDisableWeapon);
                disableOnAnimEnd = false;
            }
        }
    }

    private void EnableWeaponObject(PlayerWeapon weapon)
    {
        GameObject obj = GetWeaponObject(weapon);
        if (obj != null) obj.SetActive(true);
    }

    private void DisableWeaponObject(PlayerWeapon weapon)
    {
        GameObject obj = GetWeaponObject(weapon);
        if (obj != null) obj.SetActive(false);
    }

    private void ApplyImmediateRotation(PlayerWeapon weapon, bool equipped)
    {
        Transform pivot = GetPivot(weapon);
        if (pivot == null) return;

        pivot.localRotation = Quaternion.Euler(
            equipped ? EquippedRotation : UnequippedRotation
        );
    }

    private Transform GetPivot(PlayerWeapon weapon)
    {
        return weapon switch
        {
            PlayerWeapon.Revolver => RevolverPivot,
            PlayerWeapon.Shotgun => ShotgunPivot,
            _ => null
        };
    }

    private GameObject GetWeaponObject(PlayerWeapon weapon)
    {
        return weapon switch
        {
            PlayerWeapon.Revolver => RevolverObject,
            PlayerWeapon.Shotgun => ShotgunObject,
            PlayerWeapon.Grapple => null,
            _ => null
        };
    }



    // --------------------
    // UI
    // --------------------

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
            Sprite sprite =
                lastRegularWeapon == PlayerWeapon.Shotgun ? ShotgunIcon : RevolverIcon;

            SetImage(hudController.SecondaryEquipmentIcon, sprite, 1f);
            return;
        }

        if (GrappleEnabled)
            SetImage(hudController.SecondaryEquipmentIcon, GrappleIcon, 1f);
        else
            SetImage(hudController.SecondaryEquipmentIcon, null, 0f);
    }

    private void SetImage(Image image, Sprite sprite, float alpha)
    {
        if (image == null) return;

        image.sprite = sprite;
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    // --------------------
    // Saving
    // --------------------

    [Serializable]
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

        ApplyImmediateRotation(PlayerWeapon.Revolver, EquippedWeapon == PlayerWeapon.Revolver);
        ApplyImmediateRotation(PlayerWeapon.Shotgun, EquippedWeapon == PlayerWeapon.Shotgun);
    }
}
