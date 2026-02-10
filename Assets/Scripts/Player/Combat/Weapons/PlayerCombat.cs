using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public PlayerWeaponManager weaponManager;

    [Header("Weapons")]
    public GameObject Revolver;
    public Transform RevolverFirePoint;
    public Transform RevolverPivot;

    public GameObject Shotgun;
    public Transform ShotgunPrimaryFirePoint;
    public Transform ShotgunSecondaryFirePoint;
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

    [Header("Prefabs")]
    public ShotgunBullet ShotgunBulletPrefab;
    public BolaProjectile BolaProjectilePrefab;

    [Header("Revolver Visuals")]
    public LineRenderer RevolverLineRenderer;
    public float RevolverRange = 100f;
    public float RevolverLineDuration = 0.05f;



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

    private float nextShotgunFireTime;
    private float nextRevolverFireTime;
    private float nextBolaFireTime;
    private float nextRicochetFireTime;

    // ---------
    private void Awake()
    {
        PlayerWeaponManager weaponManager = PlayerWeaponManager.Instance;

        if (weaponManager == null)
        {
            Debug.LogError("PlayerWeaponManager not found!");
            weaponManager = PlayerWeaponManager.Instance;
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
        if (weaponManager == null)
        {
            Debug.LogError("PlayerWeaponManager not found, retrying...");
            weaponManager = PlayerWeaponManager.Instance;
            return;
        }

        // Get the currently equipped weapon
        currentWeapon = weaponManager.EquippedWeapon;
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

        if (Time.time < nextShotgunFireTime)
            return;

        nextShotgunFireTime = Time.time + (1f / ShotgunFireRate);

        float damagePerPellet = MaxShotgunDamage / MaxShotgunProjectiles;

        for (int i = 0; i < MaxShotgunProjectiles; i++)
        {
            Vector3 direction = GetRandomConeDirection(
                ShotgunPrimaryFirePoint.forward,
                ShotgunSpreadAngle
            );

            ShotgunBullet bullet = Instantiate(
                ShotgunBulletPrefab,
                ShotgunPrimaryFirePoint.position,
                Quaternion.LookRotation(direction)
            );

            bullet.Fire(direction, damagePerPellet, ShotgunFalloffCurve);
        }
    }

    private Vector3 GetRandomConeDirection(Vector3 forward, float angle)
    {
        float radius = Mathf.Tan(angle * Mathf.Deg2Rad);
        Vector2 random = Random.insideUnitCircle * radius;

        Vector3 direction =
            forward +
            ShotgunPrimaryFirePoint.right * random.x +
            ShotgunPrimaryFirePoint.up * random.y;

        return direction.normalized;
    }

    private void ShotgunAltFire()
    {
        Debug.Log("Shotgun Alt Fire");
        // Bola - [Projectile] A bola is launched from the bottom of the shotgun, on contacting the ground or any enemies, it will lose all velocity, freeze in place (become kinematic) then pull in all surrounding enemies in a radius around the hit location and hold them still for a short time (can be done by enabling NoAI, then disabling it), doing a small amount of damage

        // Everything with the Base class BaseEnemy in the radius is dragged towards the bola, and has its AI switched off for BolaHoldTime

        if (Time.time < nextBolaFireTime)
            return;

        nextBolaFireTime = Time.time + BolaCooldown;

        BolaProjectile bola = Instantiate(
            BolaProjectilePrefab,
            ShotgunSecondaryFirePoint.position,
            Quaternion.Euler(0, 0, 0)
        );

        bola.pullRange = BolaPullRange;
        bola.holdTime = BolaHoldTime;
        bola.damage = BolaDamage;

        bola.Fire(ShotgunSecondaryFirePoint.forward * 25f);
    }

    // ------------------------------
    // REVOLVER
    // ------------------------------
    private void RevolverPrimaryFire()
    {
        Debug.Log("Revolver Primary Fire");
        // [Hitscan] Fire a single shot that does medium damage (eg: 5)

        if (Time.time < nextRevolverFireTime)
            return;

        nextRevolverFireTime = Time.time + (1f / RevolverFireRate);

        Vector3 start = RevolverFirePoint.position;
        Vector3 dir = RevolverFirePoint.forward;
        Vector3 end = start + dir * RevolverRange;

        if (Physics.Raycast(start, dir, out RaycastHit hit, RevolverRange))
        {
            end = hit.point;

            Entity entity = hit.collider.GetComponentInParent<Entity>();
            if (entity != null)
            {
                entity.TakeDamage(RevolverDamage);
            }
        }

        DrawRevolverLine(start, end);
    }

    private void DrawRevolverLine(Vector3 start, Vector3 end)
    {
        if (RevolverLineRenderer == null)
            return;

        RevolverLineRenderer.positionCount = 2;
        RevolverLineRenderer.SetPosition(0, start);
        RevolverLineRenderer.SetPosition(1, end);

        CancelInvoke(nameof(ClearRevolverLine));
        Invoke(nameof(ClearRevolverLine), RevolverLineDuration);
    }

    private void ClearRevolverLine()
    {
        if (RevolverLineRenderer != null)
            RevolverLineRenderer.positionCount = 0;
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
