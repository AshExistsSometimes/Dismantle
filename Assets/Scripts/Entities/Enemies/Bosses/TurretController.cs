using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public bool TurretActive = false;

    [Header("References")]
    public Transform YawPivot;
    public Transform PitchPivot;
    public Transform FirePoint;

    [Header("Attack Prefabs")]
    public GameObject ProjectilePrefab;
    public GameObject LaserPrefab;

    public float ProjectileSpeed = 25f;

    [Header("Attack Types")]
    public bool SingleFire;
    public bool BurstFire;
    public bool ShotgunFire;
    public bool Laser;

    [Header("Cooldown")]
    public Vector2 AttackCooldownRange = new Vector2(1f, 3f);

    [Header("Burst Fire")]
    public float BurstRate = 0.1f;
    public int BurstCount = 5;

    [Header("Shotgun")]
    public int ShotgunCount = 8;
    public float ShotgunSpread = 20f;

    [Header("Laser")]
    public LayerMask LaserBlockMask;
    public int LaserDamage = 10;

    [Header("Laser Visual")]
    public LineRenderer LaserLine;
    public float LaserChargeTime = 3f;

    private Transform player;
    private bool canAttack = true;

    private Quaternion pitchBaseRot;
    private Vector3 pitchBaseForward;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        pitchBaseRot = PitchPivot.localRotation;
        pitchBaseForward = PitchPivot.forward; // record initial direction

        if (LaserLine != null)
            LaserLine.enabled = false;
    }

    private void Update()
    {
        if (player == null || !TurretActive) return;

        Aim();

        if (canAttack && HasClearShotToPlayer())
            StartCoroutine(AttackRoutine());
    }

    private void Aim()
    {
        if (player == null) return;

        // -------- YAW --------
        Vector3 toPlayer = player.position - YawPivot.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetYaw = Quaternion.LookRotation(toPlayer);
            YawPivot.rotation = Quaternion.Slerp(YawPivot.rotation, targetYaw, Time.deltaTime * 5f);
        }

        // -------- PITCH --------
        Vector3 toPlayerWorld = player.position - PitchPivot.position;

        // Compute rotation from the original forward direction
        Quaternion targetPitch = Quaternion.FromToRotation(pitchBaseForward, toPlayerWorld);
        PitchPivot.localRotation = Quaternion.Slerp(
            PitchPivot.localRotation,
            pitchBaseRot * targetPitch,
            Time.deltaTime * 5f
        );
    }



    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        List<System.Func<IEnumerator>> attacks = new List<System.Func<IEnumerator>>();

        if (SingleFire) attacks.Add(() => DoSingle());
        if (BurstFire) attacks.Add(() => DoBurst());
        if (ShotgunFire) attacks.Add(() => DoShotgun());
        if (Laser) attacks.Add(() => DoLaser());

        if (attacks.Count > 0)
        {
            yield return StartCoroutine(attacks[Random.Range(0, attacks.Count)]());
        }

        yield return new WaitForSeconds(Random.Range(AttackCooldownRange.x, AttackCooldownRange.y));
        canAttack = true;
    }

    private IEnumerator DoSingle()
    {
        FireProjectile(FirePoint.forward);
        yield break;
    }

    private IEnumerator DoBurst()
    {
        for (int i = 0; i < BurstCount; i++)
        {
            if (!HasClearShotToPlayer()) yield break;

            FireProjectile(FirePoint.forward);
            yield return new WaitForSeconds(BurstRate);
        }
    }

    private IEnumerator DoShotgun()
    {
        for (int i = 0; i < ShotgunCount; i++)
        {
            Vector3 dir = Quaternion.Euler(
                Random.Range(-ShotgunSpread, ShotgunSpread),
                Random.Range(-ShotgunSpread, ShotgunSpread),
                0f
            ) * FirePoint.forward;

            FireProjectile(dir);
        }

        yield break;
    }

    

    private IEnumerator DoLaser()
    {
        float t = 0f;

        LaserLine.enabled = true;

        // -------- CHARGE PHASE --------
        while (t < LaserChargeTime)
        {
            if (!HasClearShotToPlayer())
            {
                LaserLine.enabled = false;
                yield break;
            }

            t += Time.deltaTime;

            Vector3 start = FirePoint.position;
            Vector3 end = start + FirePoint.forward * 500f;

            LaserLine.SetPosition(0, start);
            LaserLine.SetPosition(1, end);

            // controlled flashing (non-epileptic)
            float normalized = t / LaserChargeTime;
            float flashSpeed = Mathf.Lerp(1.5f, 6f, normalized);
            float flash = Mathf.PingPong(t * flashSpeed, 1f);

            LaserLine.enabled = flash > 0.5f;

            yield return null;
        }

        LaserLine.enabled = false;

        if (!HasClearShotToPlayer())
            yield break;

        // -------- ACTUAL LASER HIT (RAYCAST DAMAGE) --------
        Ray ray = new Ray(FirePoint.position, FirePoint.forward);
        float dist = 500f;

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, LaserBlockMask))
        {
            dist = hit.distance;

            IDamagable d = hit.collider.GetComponentInParent<IDamagable>();
            if (d != null)
            {
                d.TakeDamage(LaserDamage);
            }
        }

        // -------- VISUAL LASER --------
        Quaternion rot = Quaternion.LookRotation(-FirePoint.forward) * Quaternion.Euler(-90f, 0f, 0f);

        GameObject laserObj = Instantiate(LaserPrefab, FirePoint.position, rot);

        Laser l = laserObj.GetComponent<Laser>();
        if (l != null)
        {
            l.Initialize(dist, LaserDamage); // purely visual now
        }

        yield return new WaitForSeconds(1f);
    }

    private void FireProjectile(Vector3 dir)
    {
        GameObject obj = Instantiate(ProjectilePrefab, FirePoint.position, Quaternion.LookRotation(dir));

        Projectile p = obj.GetComponent<Projectile>();
        if (p != null)
            p.Initialize(10, dir, ProjectileSpeed, true);
    }

    private bool HasClearShotToPlayer()
    {
        Vector3 origin = FirePoint.position;
        Vector3 dir = (player.position - origin).normalized;
        float dist = Vector3.Distance(origin, player.position);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            return hit.collider.GetComponentInParent<IDamagable>() != null;
        }

        return false;
    }
}