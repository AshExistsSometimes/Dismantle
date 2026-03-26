using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1AI : MonoBehaviour, IDamagable
{
    [Header("State")]
    public bool BossActive = false;

    [Header("Health")]
    public int MaxHealth = 200;
    public int currentHealth;
    [Space]
    public float BossArmour = 2f;

    [Header("Weak Points")]
    public List<BossWeakPoint> WeakPoints = new List<BossWeakPoint>();

    [Header("Turrets")]
    public List<TurretController> Turrets = new List<TurretController>();

    [Header("Movement")]
    public float WalkSpeed = 10f;// How fast the boss moves along the x axis (must be flipped to negative)
    public float StepTime = 2f;// Each step takes 2s, boss moves during this time
    public float StepHoldTime = 0.25f; // After a step is finished, there will be 0.25s until the next one, boss is not moving during this time
    [Space]
    public AnimationCurve WalkBob = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);// Each step, the bosses height will trace this curve
                                                                             // At the start of the step, it will be at timeStart (0), and by the end it will be at timeEnd (1). the graph will go up from 0 and return to 0 by its end, so it will trace the bob

    public float BobIntensity = 3f; //How much higher than the start height the boss will be at the peak of the walk bob

    private float startHeight = 0f;

    public string BossName = "Boss 1";


    private Coroutine movementRoutine;

    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float damageFlashTime = 0.075f;

    private Material[] cachedMaterials;
    private Color[] originalColors;
    private Coroutine flashRoutine;

    private void Start()
    {
        currentHealth = MaxHealth;
        startHeight = gameObject.transform.position.y;

        foreach (TurretController turret in Turrets)
        {
            turret.TurretActive = true;
        }

        CacheMaterials();
    }

    private void CacheMaterials()
    {
        List<Material> mats = new List<Material>();

        var renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            mats.AddRange(renderers[i].materials);
        }

        cachedMaterials = mats.ToArray();
        originalColors = new Color[cachedMaterials.Length];

        for (int i = 0; i < cachedMaterials.Length; i++)
        {
            originalColors[i] = cachedMaterials[i].color;
        }
    }

    private void Update()
    {
        if (BossActive && movementRoutine == null)
        {
            movementRoutine = StartCoroutine(MovementLoop());
        }
    }

    private IEnumerator MovementLoop()
    {
        while (BossActive)
        {
            yield return new WaitForSeconds(StepHoldTime);
            yield return Step();
        }

        movementRoutine = null;
    }

    IEnumerator Step()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < StepTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / StepTime);

            float xOffset = WalkSpeed * Time.deltaTime;

            float bobT = WalkBob.Evaluate(t);
            float yOffset = startHeight + (bobT * BobIntensity);

            transform.position = new Vector3(
                transform.position.x + xOffset,
                yOffset,
                transform.position.z
            );

            yield return null;
        }

        transform.position = new Vector3(
            transform.position.x,
            startHeight,
            transform.position.z
        );
    }

    // -------------------------------
    // Health / Damage
    // -------------------------------
    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.FloorToInt(damage / BossArmour);
        if (damageTaken < 1) { damageTaken = 1; }

        currentHealth -= damageTaken;
        FlashDamageIndicator();

        Debug.Log("Boss 1 took " + damageTaken + " damage, out of the " + damage + " damage that was dealt by the player");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void FlashDamageIndicator()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        for (int i = 0; i < cachedMaterials.Length; i++)
        {
            cachedMaterials[i].color = damageFlashColor;
        }

        yield return new WaitForSeconds(damageFlashTime);

        for (int i = 0; i < cachedMaterials.Length; i++)
        {
            cachedMaterials[i].color = originalColors[i];
        }
    }

    public void Die()
    {
        LevelManager.Instance.EnemyWasKilled();

        BossActive = false;
        StopAllCoroutines();
        Debug.Log("Boss 1 died");

        LevelManager.Instance.LevelComplete();
    }
}
