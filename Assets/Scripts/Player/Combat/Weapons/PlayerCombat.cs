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

    private PlayerWeapon currentWeapon;

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

        weaponManager.RegisterWeapons(
            Revolver,
            RevolverPivot,
            Shotgun,
            ShotgunPivot,
            Sword
        );
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
        // Fire a scatter of projectiles that do damage equal to the shotguns overall damage / the number of projectiles, will do less damage at greater distances (Falloff)
    }

    private void ShotgunAltFire()
    {
        Debug.Log("Shotgun Alt Fire");
        // Bola - [Projectile] A bola is launched from the bottom of the shotgun, on contacting the ground or any enemies, it will pull in all surrounding enemies in a radius around the hit location, and hold them still for a short time, doing a small amount of damage
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
    }
}
