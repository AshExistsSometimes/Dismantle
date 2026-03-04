using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss3AI : MonoBehaviour, IDamagable
{
    public bool BossActive = false;
    public enum Phase
    {
        Inactive,
        Phase1,
        Phase2
    }
    public Phase CurrentPhase = Phase.Inactive;
    private bool switchingPhases = false;

    public GameObject BossPivotPoint;
    private Transform bossPivotDefaultPos;

    public AnimationCurve FloatBob = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float BobSpeed = 0.5f;
    public float FloatHeightVariation = 3f;
    private float bobTimer = 0f;
    private float bossStartY;

    public float Speed = 3f;

    public float Phase2TransitionSpeed = 2f;

    private float currentBaseHeight;
    private float targetBaseHeight;

    [Header("Health")]
    public int MaxHealth = 200;
    public int currentHealth;
    public float BossArmour = 2f;


    public float Phase2HeightDifference = 20f;

    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float damageFlashTime = 0.075f;

    private Material[] cachedMaterials;
    private Color[] originalColors;
    private Coroutine flashRoutine;


    private void Awake()
    {
        CurrentPhase = BossActive ? Phase.Phase1 : Phase.Inactive;

        currentHealth = MaxHealth;

        bossPivotDefaultPos = BossPivotPoint.transform;

        currentBaseHeight = BossPivotPoint.transform.position.y;
        targetBaseHeight = currentBaseHeight;

        bossStartY = transform.localPosition.y;

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

    void Update()
    {
        RunPhaseMovement();
    }

    private void RunPhaseMovement()
    {
        if (CurrentPhase == Phase.Inactive)
        {
            BossOrbit();
        }
        else if (CurrentPhase == Phase.Phase1)
        {
            BossOrbit();
        }
        else
        {
            BossOrbit();
        }
    }

    public void BossOrbit()
    {
        // Rotate around Y
        BossPivotPoint.transform.Rotate(Vector3.up, Speed * Time.deltaTime, Space.World);

        // Advance bob timer (loops 0–1 repeatedly)
        bobTimer += Time.deltaTime * BobSpeed;
        float bobT = Mathf.Repeat(bobTimer, 1f);

        float bobOffset = FloatBob.Evaluate(bobT) * FloatHeightVariation;

        Vector3 pos = transform.localPosition;
        pos.y = bossStartY + bobOffset;
        transform.localPosition = pos;
    }



    ///////////////////////////////////////////////////////////

    public void TakeDamage(int damage)
    {
        if (switchingPhases || !BossActive) { return; }


        int damageTaken = Mathf.FloorToInt(damage / BossArmour);
        if (damageTaken > 1) { damageTaken = 1; }

        currentHealth -= damageTaken;
        FlashDamageIndicator();

        Debug.Log("Boss 3 took " + damageTaken + " damage, out of the " + damage + " damage that was dealt by the player");

        if ((CurrentPhase != Phase.Phase2) && (currentHealth <= (MaxHealth / 2)))
        {
            StartCoroutine(SwitchToPhase2());
        }

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
        BossActive = false;
        StopAllCoroutines();
        LevelManager.Instance.LevelComplete();
    }



    private IEnumerator SwitchToPhase2()
    {
        switchingPhases = true;
        targetBaseHeight = bossPivotDefaultPos.position.y + Phase2HeightDifference;

        // Any animations or VFX go here
        LerpToPhase2Height();

        CurrentPhase = Phase.Phase2;
        yield return new WaitForSeconds(0.1f);
        switchingPhases = false;
    }

    private void LerpToPhase2Height()
    {
        Vector3 Phase2Height = new Vector3(
            bossPivotDefaultPos.position.x, 
            bossPivotDefaultPos.position.y + Phase2HeightDifference, 
            bossPivotDefaultPos.position.z);


        BossPivotPoint.transform.position = Vector3.Lerp(
            bossPivotDefaultPos.position, 
            Phase2Height, 
            Phase2TransitionSpeed);
    }

    public void ActivateBoss()
    {
        CurrentPhase = Phase.Phase1;
        targetBaseHeight = bossPivotDefaultPos.position.y;
        BossActive = true;
    }
}
