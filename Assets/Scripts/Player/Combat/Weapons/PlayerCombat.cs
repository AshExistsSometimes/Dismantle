using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public PlayerWeaponManager weaponManager;

    [Header("Weapons")]
    public GameObject Revolver;
    public Transform RevolverPivot;

    public GameObject Shotgun;
    public Transform ShotgunPivot;

    public GameObject Sword; // Reference only rn
    

    [Header("Shotgun - Primary")]
    public float MaxShotgunDamage = 20f;
    public AnimationCurve ShotgunFalloffCurve;

    public int MaxShotgunProjectiles = 10;

    public float ShotgunSpreadAngle = 30f; // Radius of cone

    public float ShotgunFireRate = 1f; // Max 1 shot every second


    [Header("Shotgun - Alternate")]
    public float BolaPullRange = 2.5f;
    public float BolaHoldTime = 1f;
    public int BolaDamage = 5;

    public float BolaCooldown = 10f; // 10s Cooldown


    [Header("Revolver - Primary")]
    public int RevolverDamage = 10;

    public float RevolverFireRate = 2f; // Max 2 shots every second


    [Header("Revolver - Alternate")]
    public int MaxRicochetTargets = 5;
    private int RicochetTargets;

    public float RicochetCooldown = 5f; // 5s Cooldown

    [Header("Sword")]
    public int SwordDamage = 30;
    public float SwordFireRate = 3f; // Max 3 Swings a second
    public float ParryWindow = 0.25f;


    private PlayerWeapon currentWeapon;

    // Cached default weapon values // 
    private float defaultShotgunDamage;
    private int defaultShotgunProjectiles;
    private float defaultShotgunSpread;
    private float defaultShotgunFireRate;

    private float defaultBolaPullRange;
    private int defaultBolaDamage;
    private float defaultBolaCooldown;
    private float defaultBolaHoldTime;

    private int defaultRevolverDamage;
    private float defaultRevolcerFireRate;

    private int defaultMaxRicochetTargets;
    private float defaultRicochetCooldown;

    private int defaultSwordDamage;
    private float defaultSwordFireRate;
    private float defaultParryWindow;

    [HideInInspector]
    public bool DefaultsCached = false;

    // ---------
    private void Awake()
    {
        PlayerWeaponManager weaponManager =
            FindFirstObjectByType<PlayerWeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogError("PlayerWeaponManager not found!");
            return;
        }

        weaponManager.RegisterWeapons
        (
            Revolver,
            RevolverPivot,
            Shotgun,
            ShotgunPivot,
            Sword
        );

        CacheAllDefaults();
    }


    private void Update()
    {
        UpdateActiveWeapon();
        HandleInput();
    }

    private void UpdateActiveWeapon()
    {
        if (weaponManager == null) return;

        // Get the currently equipped weapon
        currentWeapon = weaponManager.EquippedWeapon;

        // Debug to see active weapon
        // Debug.Log("Current Weapon: " + currentWeapon);

        // Here we could do additional per-frame checks based on active weapon
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            MeleeAttack();
        }

        // Primary fire
        if (Input.GetMouseButtonDown(0))
        {
            switch (currentWeapon)
            {
                case PlayerWeapon.Revolver:
                    RevolverPrimaryFire();
                    break;
                case PlayerWeapon.Shotgun:
                    ShotgunPrimaryFire();
                    break;
                // Any more guns require extra additions to this
            }
        }

        // Alt fire
        if (Input.GetMouseButtonDown(1))
        {
            switch (currentWeapon)
            {
                case PlayerWeapon.Revolver:
                    RevolverAltFire();
                    break;
                case PlayerWeapon.Shotgun:
                    ShotgunAltFire();
                    break;
                // Any more guns require extra additions to this
            }
        }
    }

    // ------------------------------
    // SHOTGUN
    // ------------------------------
    private void ShotgunPrimaryFire()
    {
        Debug.Log("Shotgun Primary Fire");
        // [Projectile] Fire a scatter of projectiles at random directions in a cone, that do damage equal to the shotguns overall damage / the number of projectiles, will do less damage at greater distances (Falloff)
        // Determine damage falloff with an animation curve, round damage to closest int
    }

    private void ShotgunAltFire()
    {
        Debug.Log("Shotgun Alt Fire");
        // Bola - [Projectile] A bola is launched from the bottom of the shotgun, on contacting the ground or any enemies, it will lose all velocity, freeze in place (become kinematic) then pull in all surrounding enemies in a radius around the hit location and hold them still for a short time (can be done by enabling NoAI, then disabling it), doing a small amount of damage

        // Everything with the Base class BaseEnemy in the radius is dragged towards the bola, and has its AI switched off for BolaHoldTime
    }

    // ------------------------------
    // REVOLVER
    // ------------------------------
    private void RevolverPrimaryFire()
    {
        Debug.Log("Revolver Primary Fire");
        // [Hitscan] Fire a single shot that does medium damage (eg: 5)
    }

    private void RevolverAltFire()
    {
        Debug.Log("Revolver Alt Fire");
        // Ricochet - [Hitscan]  Player spins gun, the longer they spin it, the more enemies the bullet can ricochet to, maxing out at 5. Ricochet will do nothing if the player misses, but if they hit an enemy, it will search a radius around said enemy, and travel to the closest enemy in that radius, it will repeat this for every enemy that hasnt been hit by that bullet, each enemy in the chain will receive less damage than the one before it by 20 % of the max damage(so from 5 - 4 - 3 - 2 - 1 or with 10 base damage 10 - 8 - 6 - 4 - 2)
    }

    // ------------------------------
    // PARRY/SLICE
    // ------------------------------
    private void MeleeAttack()
    {
        Debug.Log("Parry");
        // Hits enemies in close range, dealing high damage (potentially cuts them in 2 if it lands the final blow)
        // Parry Projectiles

        // Make a trigger ParryHitbox active for ParryWindow seconds
        // If anything with IParryable is in the hitbox, run its Parry() function
        // If anything with IDamagable is in the hitbox, run its Damage() function, inputting SwordDamage
    }

    // -----------------------------
    // CACHE DEFAULTS
    // -----------------------------

    private void CacheAllDefaults()
    {
        // Shotgun Primary
        defaultShotgunDamage = MaxShotgunDamage;
        defaultShotgunProjectiles = MaxShotgunProjectiles;
        defaultShotgunSpread = ShotgunSpreadAngle;
        defaultShotgunFireRate = ShotgunFireRate;

        // Shotgun Alt
        defaultBolaPullRange = BolaPullRange;
        defaultBolaDamage = BolaDamage;
        defaultBolaCooldown = BolaCooldown;
        defaultBolaHoldTime = BolaHoldTime;

        // Revolver Primary
        defaultRevolverDamage = RevolverDamage;
        defaultRevolcerFireRate = RevolverFireRate;

        // Revolver Alt
        defaultMaxRicochetTargets = MaxRicochetTargets;
        defaultRicochetCooldown = RicochetCooldown;

        // Sword
        defaultSwordDamage = SwordDamage;
        defaultSwordFireRate = SwordFireRate;
        defaultParryWindow = ParryWindow;

        DefaultsCached = true;
    }

    public void ResetStats()
    {
        // Shotgun Primary
        MaxShotgunDamage = defaultShotgunDamage;
        MaxShotgunProjectiles = defaultShotgunProjectiles;
        ShotgunSpreadAngle = defaultShotgunSpread;
        ShotgunFireRate = defaultShotgunFireRate;

        // Shotgun Alt
        BolaPullRange = defaultBolaPullRange;
        BolaDamage = defaultBolaDamage;
        BolaCooldown = defaultBolaCooldown;
        BolaHoldTime = defaultBolaHoldTime;
        
        // Revolver Primary
        RevolverDamage = defaultRevolverDamage;
        RevolverFireRate = defaultRevolcerFireRate;
        
        // Revolver Alt
        MaxRicochetTargets = defaultMaxRicochetTargets;
        RicochetCooldown = defaultRicochetCooldown;

        // Sword
        SwordDamage = defaultSwordDamage;
        SwordFireRate = defaultSwordFireRate;
        ParryWindow = defaultParryWindow;
    }

    // -----------------------------
    // APPLY MODIFIERS
    // -----------------------------

    // If the defaults have been cached, apply all active modifiers to the stats
    public bool AttemptApplyModifiers()
    {
        if (!DefaultsCached) { return false; }
        else 
        {
            // Set New MaxShotgunDamage;
            // Set New MaxShotgunProjectiles;
            // Set New ShotgunSpreadAngle;
            // Set New ShotgunFireRate;

            // Set New BolaPullRange;
            // Set New BolaDamage
            // Set New BolaCooldown;
            // Set New BolaHoldTime

            // Set New RevolverDamage;
            // Set New RevolverFireRate;

            // Set New MaxRicochetTargets;
            // Set New RicochetCooldown

            // Set New SwordDamage
            // Set New SwordFireRate
            // Set New ParryWindow

            return true; 
        }
        
    }
}
