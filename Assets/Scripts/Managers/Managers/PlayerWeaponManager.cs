using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerWeapon
{
    Grapple,
    Revolver,
    Shotgun,
    None
}

public class PlayerWeaponManager : MonoBehaviour, ISaveable
{
    public string SaveKey => "PlayerWeaponManager";
    public static PlayerWeaponManager Instance;

    public bool EquipmentEnabled = true;

    [Header("Weapon State")]
    public PlayerWeapon EquippedWeapon;

    [SerializeField]
    public PlayerWeapon lastRegularWeapon = PlayerWeapon.Revolver;

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

    [Header("<b><size=150%>Weapon Animation Values")]//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // REVOLVER //
    [Header("<b><size=110%><color=#15D7ED>Revolver")]
    [Header("<b><i><size=105%><color=#1EA9BA>References")]
    public GameObject RevolverObject;
    public Transform RevolverPivot;
    public GameObject RevolverDrum;
    public GameObject RevolverMuzzleFlash;
    public GameObject RevolverBody;
    [Header("<b><i><size=105%><color=#1EA9BA>Animation Values")]
    [Header("Recoil Rotation")]
    public AnimationCurve RevolverFireRecoilAnimation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float RevolverRecoilAnimSpeed = 0.5f;
    public float RevolverRecoilRotation = 25f;
    [Space]
    [Header("Recoil Pushback")]
    // Needs to move the Revolver pivot on the local Z axis position, from its current position, to the currentPos + MaxPushback, over the time of PushbackAnimSpeed
    public AnimationCurve RevolverFireRecoilPushbackAnimation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float RevolverRecoilPushbackAnimSpeed = 0.5f;
    public float RevolverRecoilPushbackMaxPushback = -0.3f;
    public Vector3 revolverPivotDefaultLocalPos;

    public Vector3 RevolverPivotDefaultEuler = new Vector3(0f, -180f, 0f);
    

    [Header("Revolver Alt Fire - Spin")]
    public float RevolverSpinSpeed = 720f; // degrees per second max
    public float RevolverPivotTargetZ = 15f; // target Z rotation while spinning

    private bool isRevolverSpinning = false;
    private float currentRevolverSpinSpeed = 0f; // current speed for spin-up
    [Space]
    [Header("Muzzle Flash")]
    [Space]

    // SHOTGUN //
    [Header("<b><size=110%><color=#ED3215>Shotgun")]
    [Header("<b><i><size=105%><color=#BA321E>References")]
    public GameObject ShotgunObject;
    public Transform ShotgunPivot;
    public GameObject ShotgunPump;
    public GameObject BolaEnclosure;
    public GameObject ShotgunMuzzleFlash;
    [Header("<b><i><size=105%><color=#BA321E>Animation Values")]
    [Header("Recoil Rotation")]
    public AnimationCurve ShotgunFireRecoilAnimation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float ShotgunRecoilAnimSpeed = 0.5f;
    public float ShotgunRecoilRotation = 14f;
    [Space]
    [Header("Recoil Pushback")]
    // Needs to move the Shotgun pivot on the local Z axis position, from its current position, to the currentPos + MaxPushback, over the time of PushbackAnimSpeed
    public AnimationCurve ShotgunFireRecoilPushbackAnimation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float ShotgunRecoilPushbackAnimSpeed = 0.5f;
    public float ShotgunRecoilPushbackMaxPushback = -0.3f;
    public Vector3 shotgunPivotDefaultLocalPos;

    public Vector3 ShotgunPivotDefaultEuler = new Vector3(0f, -180f, 0f);

    [Space]
    [Header("Pump")]
    public AnimationCurve ShotgunPumpAnimation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float ShotgunPumpAnimSpeed = 0.6f;
    [Space]
    [Header("Bola")]
    public AnimationCurve BolaOpenAnimation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float BolaOpenAnimSpeed = 0.1f;
    [Space]
    public AnimationCurve BolaCloseAnimation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float BolaCloseAnimSpeed = 0.5f;
    [Space]

    // SWORD / PARRY //
    [Header("<b><size=110%><color=#FAE016>Sword")]
    [Header("<b><i><size=105%><color=#B5A422>References")]
    public GameObject SwordObject;
    public ParryHitbox SwordHitbox;
    [Header("<b><i><size=105%><color=#B5A422>Animation Values")]
    public AnimationCurve SwordSwingAnimCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float SwordSwingSpeed = 0.5f;
    public float SwordMaxSwingAngle = 90f; // degrees
    [Space]
    [Space]

    // GENERAL //
    [Header("<b><size=110%><color=#2BDE18>General")]
    public float MuzzleFlashTime = 0.075f;


    // --------------------
    // Weapon Animation
    // --------------------

    [Header("Weapon Equipping")]



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

    public PlayerCombat PCReference;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SaveManager.Instance?.Register(this);
    }
    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Register(this);
            Debug.Log("[ModifierManager] Registered with SaveManager.");
        }
        else
        {
            Debug.LogError("[ModifierManager] SaveManager not found!");
        }

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

        if (RevolverPivot != null)
            revolverPivotDefaultLocalPos = RevolverPivot.localPosition;

        if (ShotgunPivot != null)
            shotgunPivotDefaultLocalPos = ShotgunPivot.localPosition;
    }

    private void Update()
    {
        if (!EquipmentEnabled) return;

        if (GrappleEnabled)
        {
            HandleGrappleInput();
        }
        HandleWeaponScroll();
        UpdateWeaponAnimation();

        if (!isRevolverSpinning) { return; }
        UpdateRevolverSpin(Time.deltaTime);
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

        // Disable the weapon after unequip animation finishes
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

        Vector3 baseEuler = GetPivotDefault(weapon);
        pivot.localRotation = Quaternion.Euler(
            baseEuler + (equipped ? EquippedRotation : UnequippedRotation)
        );
    }

    private Vector3 GetPivotDefault(PlayerWeapon weapon)
    {
        return weapon switch
        {
            PlayerWeapon.Revolver => RevolverPivotDefaultEuler,
            PlayerWeapon.Shotgun => ShotgunPivotDefaultEuler,
            _ => Vector3.zero
        };
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

    public void RefreshWeaponUI()
    {
        UpdateHUDIcon();
        UpdateSecondaryHUDIcon();
    }

    public void RegisterHUD(HUDController hud)
    {
        hudController = hud;
        UpdateHUDIcon();
        UpdateSecondaryHUDIcon();
    }

    public void RegisterWeapons(
    GameObject revolver,
    GameObject revolverBody,
    Transform revolverPivot,
    GameObject revolverDrum,
    GameObject revolverMuzzleFlash,
    GameObject shotgun,
    Transform shotgunPivot,
    GameObject shotgunFlash,
    GameObject shotgunPump,
    GameObject bolaEnclosure,
    GameObject sword,
    ParryHitbox swordHitbox = null
)
    {
        // Revolver
        RevolverObject = revolver;
        RevolverBody = revolverBody;
        RevolverPivot = revolverPivot;
        RevolverDrum = revolverDrum;
        RevolverMuzzleFlash = revolverMuzzleFlash;

        // Shotgun
        ShotgunObject = shotgun;
        ShotgunPivot = shotgunPivot;
        ShotgunMuzzleFlash = shotgunFlash;
        ShotgunPump = shotgunPump;

        // Bola enclosure
        BolaEnclosure = bolaEnclosure;

        // Sword
        SwordObject = sword;
        SwordHitbox = swordHitbox;

        // Ensure only the equipped weapon is visible
        SyncWeaponVisibility();
    }

    private void SyncWeaponVisibility()
    {
        if (RevolverObject != null)
            RevolverObject.SetActive(EquippedWeapon == PlayerWeapon.Revolver);

        if (ShotgunObject != null)
            ShotgunObject.SetActive(EquippedWeapon == PlayerWeapon.Shotgun);

        // Sword later
    }

    // --------------------
    // Weapon Fire Animation Logic
    // --------------------

    #region Revolver Animations
    // Revolver
    public void FireRevolverAnimation()
    {
        if (RevolverMuzzleFlash != null)
            StartCoroutine(RevolverMuzzleFlashRoutine());

        if (RevolverPivot != null)
            StartCoroutine(RevolverRecoilRoutine());
    }

    private IEnumerator RevolverMuzzleFlashRoutine()
    {
        RevolverMuzzleFlash.SetActive(true);
        yield return new WaitForSeconds(MuzzleFlashTime);
        RevolverMuzzleFlash.SetActive(false);
    }

    private IEnumerator RevolverRecoilRoutine()
    {
        Vector3 baseEuler = RevolverPivotDefaultEuler;
        Vector3 basePos = revolverPivotDefaultLocalPos;

        float timer = 0f;

        while (timer < RevolverRecoilAnimSpeed)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / RevolverRecoilAnimSpeed);

            float rotCurve = RevolverFireRecoilAnimation.Evaluate(t);
            float pushCurve = RevolverFireRecoilPushbackAnimation.Evaluate(t);

            // Rotation recoil
            RevolverPivot.localRotation = Quaternion.Euler(
                baseEuler + new Vector3(rotCurve * RevolverRecoilRotation, 0f, 0f)
            );

            // Pushback recoil (local Z)
            RevolverPivot.localPosition = basePos + new Vector3(
                0f,
                0f,
                pushCurve * RevolverRecoilPushbackMaxPushback
            );

            yield return null;
        }

        // Reset both
        RevolverPivot.localRotation = Quaternion.Euler(baseEuler);
        RevolverPivot.localPosition = revolverPivotDefaultLocalPos;
    }

    public void DrumNextBulletAnimation()
    {
        if (RevolverDrum == null) return;

        // Revolver has 6 chambers -> 360 / 6 = 60 degrees
        RevolverDrum.transform.Rotate(Vector3.forward, 60f, Space.Self);
    }

    public void RevolverSpinAnimation(bool spinning)
    {
        isRevolverSpinning = spinning;
    }

    private void UpdateRevolverSpin(float deltaTime)
    {
        if (RevolverBody == null || RevolverPivot == null) return;

        float targetSpeed = isRevolverSpinning ? RevolverSpinSpeed : 0f;
        currentRevolverSpinSpeed = Mathf.MoveTowards(currentRevolverSpinSpeed, targetSpeed, RevolverSpinSpeed * deltaTime * 2f);

        RevolverBody.transform.Rotate(Vector3.right, currentRevolverSpinSpeed * deltaTime, Space.Self);

        Vector3 baseline = RevolverPivotDefaultEuler;
        Vector3 rot = RevolverPivot.localEulerAngles;
        float currentZ = rot.z;
        if (currentZ > 180f) currentZ -= 360f;

        float targetZ = isRevolverSpinning ? RevolverPivotDefaultEuler.z + RevolverPivotTargetZ : RevolverPivotDefaultEuler.z;
        rot.z = Mathf.Lerp(currentZ, targetZ, deltaTime * 5f);
        RevolverPivot.localEulerAngles = rot;
    }

    public void StopRevolverSpin()
    {
        isRevolverSpinning = false;

        // Snap back to baseline instantly
        if (RevolverBody != null)
            RevolverBody.transform.localRotation = Quaternion.Euler(Vector3.zero);
        if (RevolverPivot != null)
            RevolverPivot.localRotation = Quaternion.Euler(RevolverPivotDefaultEuler);
    }
    #endregion

    #region Shotgun Animations
    // Shotgun
    public void FireShotgunAnimation()
    {
        if (ShotgunMuzzleFlash != null)
            StartCoroutine(ShotgunMuzzleFlashRoutine());

        if (ShotgunPivot != null)
            StartCoroutine(ShotgunRecoilRoutine());
    }

    private IEnumerator ShotgunMuzzleFlashRoutine()
    {
        ShotgunMuzzleFlash.SetActive(true);
        yield return new WaitForSeconds(MuzzleFlashTime);
        ShotgunMuzzleFlash.SetActive(false);
    }

    private IEnumerator ShotgunRecoilRoutine()
    {
        if (ShotgunPivot == null) yield break;

        Vector3 baseEuler = ShotgunPivotDefaultEuler;
        Vector3 basePos = shotgunPivotDefaultLocalPos;

        float timer = 0f;

        while (timer < ShotgunRecoilAnimSpeed)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / ShotgunRecoilAnimSpeed);

            float rotCurve = ShotgunFireRecoilAnimation.Evaluate(t);
            float pushCurve = ShotgunFireRecoilPushbackAnimation.Evaluate(t);

            // Rotation recoil
            ShotgunPivot.localRotation = Quaternion.Euler(
                baseEuler + new Vector3(rotCurve * ShotgunRecoilRotation, 0f, 0f)
            );

            // Pushback recoil (local Z)
            ShotgunPivot.localPosition = basePos + new Vector3(
                0f,
                0f,
                pushCurve * ShotgunRecoilPushbackMaxPushback
            );

            yield return null;
        }

        // Reset both
        ShotgunPivot.localRotation = Quaternion.Euler(baseEuler);
        ShotgunPivot.localPosition = shotgunPivotDefaultLocalPos;
    }

    public void PumpShotgun()
    {
        if (ShotgunPump == null) return;
        StartCoroutine(ShotgunPumpRoutine());
    }

    private IEnumerator ShotgunPumpRoutine()
    {
        Vector3 startPos = ShotgunPump.transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0f, 0f, -0.4f);

        float timer = 0f;
        while (timer < ShotgunPumpAnimSpeed)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / ShotgunPumpAnimSpeed);
            float curveT = ShotgunPumpAnimation.Evaluate(t);
            ShotgunPump.transform.localPosition = Vector3.Lerp(startPos, endPos, curveT);
            yield return null;
        }

        // Reset pump back to start
        ShotgunPump.transform.localPosition = startPos;
    }

    // Bola enclosure
    public void OpenBolaEnclosure()
    {
        if (BolaEnclosure == null) return;
        StartCoroutine(BolaEnclosureRoutine(-50f, BolaOpenAnimation, BolaOpenAnimSpeed));
    }

    public void CloseBolaEnclosure()
    {
        if (BolaEnclosure == null) return;
        StartCoroutine(BolaEnclosureRoutine(0f, BolaCloseAnimation, BolaCloseAnimSpeed));
    }

    private IEnumerator BolaEnclosureRoutine(float targetXRot, AnimationCurve curve, float duration)
    {
        float timer = 0f;
        float startRot = BolaEnclosure.transform.localEulerAngles.x;

        // Fix angle wrapping
        if (startRot > 180f) startRot -= 360f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float curveValue = curve.Evaluate(t);
            float rotX = Mathf.Lerp(startRot, targetXRot, curveValue);
            Vector3 rot = BolaEnclosure.transform.localEulerAngles;
            rot.x = rotX;
            BolaEnclosure.transform.localEulerAngles = rot;
            yield return null;
        }

        // Ensure final rotation
        Vector3 finalRot = BolaEnclosure.transform.localEulerAngles;
        finalRot.x = targetXRot;
        BolaEnclosure.transform.localEulerAngles = finalRot;
    }
    #endregion
    // Sword
    #region Sword Animations

    private bool swordBusy = false;

    public void PlaySwordParry(float parryWindow, float cooldown)
    {
        if (swordBusy) return;
        StartCoroutine(SwordParryRoutine(parryWindow, cooldown));
    }

    private IEnumerator SwordParryRoutine(float parryWindow, float cooldown)
    {
        swordBusy = true;

        if (SwordObject != null)
            SwordObject.SetActive(true);

        if (SwordHitbox != null)
            SwordHitbox.SetActive(true);

        // Cache starting rotation
        Quaternion startRot = SwordObject.transform.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, SwordMaxSwingAngle, 0f);

        float timer = 0f;

        while (timer < SwordSwingSpeed)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / SwordSwingSpeed);
            float curve = SwordSwingAnimCurve.Evaluate(t);

            SwordObject.transform.localRotation = Quaternion.Slerp(startRot, targetRot, curve);

            yield return null;
        }

        // Snap back cleanly
        SwordObject.transform.localRotation = startRot;

        if (SwordHitbox != null)
            SwordHitbox.SetActive(false);

        if (SwordObject != null)
            SwordObject.SetActive(false);

        yield return new WaitForSeconds(cooldown);

        swordBusy = false;
    }

    #endregion


    // --------------------
    // Saving
    // --------------------

    [Serializable]
    public class SaveData
    {
        public PlayerWeapon equippedWeapon;
        public PlayerWeapon lastRegularWeapon;
    }

    public void LoadState(object data)
    {
        var save = (SaveData)data;
        lastRegularWeapon = save.lastRegularWeapon;
        SetWeapon(save.equippedWeapon);

        ApplyImmediateRotation(PlayerWeapon.Revolver, EquippedWeapon == PlayerWeapon.Revolver);
        ApplyImmediateRotation(PlayerWeapon.Shotgun, EquippedWeapon == PlayerWeapon.Shotgun);
    }

    public Dictionary<string, string> CaptureSaveData()
    {
        return new Dictionary<string, string>
        {
            { "EquippedWeapon", EquippedWeapon.ToString() },
            { "LastRegularWeapon", lastRegularWeapon.ToString() },
        };
    }

    public void RestoreSaveData(Dictionary<string, string> data)
    {
        if (data.TryGetValue("EquippedWeapon", out var equipped))
        {
            if (Enum.TryParse<PlayerWeapon>(equipped, out var parsed))
                EquippedWeapon = parsed;
            else
                EquippedWeapon = PlayerWeapon.Revolver; // fallback
        }

        if (data.TryGetValue("LastRegularWeapon", out var last))
        {
            if (Enum.TryParse<PlayerWeapon>(last, out var parsed))
                lastRegularWeapon = parsed;
            else
                lastRegularWeapon = PlayerWeapon.Revolver; // fallback
        }

        ApplyImmediateRotation(PlayerWeapon.Revolver, EquippedWeapon == PlayerWeapon.Revolver);
        ApplyImmediateRotation(PlayerWeapon.Shotgun, EquippedWeapon == PlayerWeapon.Shotgun);
        SyncWeaponVisibility();
        UpdateHUDIcon();
        UpdateSecondaryHUDIcon();
    }
}
