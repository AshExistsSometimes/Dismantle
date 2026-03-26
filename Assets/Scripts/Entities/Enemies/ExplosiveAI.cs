using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Suicide enemy that flashes and explodes when near player.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ExplosiveAI : BaseEnemy
{
    public Transform VisualRoot;

    [Header("Explosion")]
    public GameObject ExplosionPrefab;
    public float TriggerRange = 3f;
    public float EscapeRange = 5f;
    public float ExplosionStartRadius = 1f;
    public float ExplosionSize = 5f;
    public float FlashTime = 0.2f;
    public LayerMask SightBlockingLayers;

    public Material WhiteMaterial;

    private NavMeshAgent agent;
    private Transform player;

    private bool isDetonating = false;
    private Vector3 originalScale;
    private Renderer rend;
    private Color originalColor;

    private Renderer[] renderers;
    private Material[][] originalMaterials;

    private Coroutine detonationCoroutine;

    protected override void Awake()
    {
        base.Awake();

        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        originalScale = VisualRoot.localScale;

        rend = VisualRoot.GetComponentInChildren<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;


        renderers = VisualRoot.GetComponentsInChildren<Renderer>();

        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].materials;
        }
    }

    private new void Update()
    {
        if (NoAI || player == null) return;

        if (!HasLOS()) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (!isDetonating)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (dist <= TriggerRange && detonationCoroutine == null)
            {
                detonationCoroutine = StartCoroutine(DetonationRoutine());
            }
        }
        else
        {
            if (dist > EscapeRange && detonationCoroutine != null)
            {
                CancelDetonation();
            }
        }
    }

    private IEnumerator DetonationRoutine()
    {
        Debug.Log("DETONATING");
        isDetonating = true;
        agent.isStopped = true;

        // Check for visual root and renderers
        if (VisualRoot == null || renderers == null || renderers.Length == 0)
            Debug.LogWarning("VisualRoot or renderers missing!");

        // 3 flashes
        for (int i = 0; i < 3; i++)
        {
            Flash(true);
            yield return new WaitForSeconds(FlashTime);

            Flash(false);
            yield return new WaitForSeconds(FlashTime);
        }

        // Final flash before explosion
        Flash(true);

        // Expansion animation
        float duration = 0.2f;
        Vector3 startScale = VisualRoot.localScale;
        Vector3 targetScale = originalScale * 1.3f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            VisualRoot.localScale = Vector3.Lerp(startScale, targetScale, t / duration);
            yield return null;
        }

        Explode();

        // Ensure the coroutine reference is cleared
        detonationCoroutine = null;
        isDetonating = false;
    }


    private void Flash(bool on)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (on)
            {
                Material[] mats = new Material[renderers[i].materials.Length];
                for (int j = 0; j < mats.Length; j++)
                    mats[j] = WhiteMaterial;

                renderers[i].materials = mats;
            }
            else
            {
                renderers[i].materials = originalMaterials[i];
            }
        }
    }

    private void CancelDetonation()
    {
        Debug.Log("DETONATION CANCELLED");

        if (detonationCoroutine != null)
        {
            StopCoroutine(detonationCoroutine);
            detonationCoroutine = null;
        }

        isDetonating = false;
        agent.isStopped = false;

        StartCoroutine(ResetScaleRoutine());
        Flash(false);
    }

    private void Explode()
    {
        Debug.Log("KABOOM");
        GameObject obj = Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);

        Explosion exp = obj.GetComponent<Explosion>();
        if (exp != null)
        {
            exp.SetSize(ExplosionSize);
            exp.SetDamage(AttackDamage);
        }

        // Ensure AI fully stops BEFORE dying
        NoAI = true;
        StopAllCoroutines();

        Die(AttackDamage);
    }

    private IEnumerator ResetScaleRoutine()
{
    float t = 0f;
    float duration = 0.2f;

    Vector3 startScale = VisualRoot.localScale;

    while (t < duration)
    {
        t += Time.deltaTime;
        VisualRoot.localScale = Vector3.Lerp(startScale, originalScale, t / duration);
        yield return null;
    }

    VisualRoot.localScale = originalScale;
}

    private bool HasLOS()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = player.position - origin;
        float dist = dir.magnitude;

        if (Physics.Raycast(origin, dir.normalized, dist, SightBlockingLayers))
            return false;

        return true;
    }
}
