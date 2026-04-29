using System.Collections.Generic;
using UnityEngine;

public class ParryHitbox : MonoBehaviour
{
    private PlayerCombat pc;
    private bool active;

    private Collider hitboxCollider;

    private Vector3 cachedLocalPosition;
    private Quaternion cachedLocalRotation;

    // Track already hit objects to avoid spam
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();

        // Cache initial local transform relative to camera
        cachedLocalPosition = transform.localPosition;
        cachedLocalRotation = transform.localRotation;

        SetActive(false);
    }

    public void Init(PlayerCombat pcReference)
    {
        pc = pcReference;
    }

    public void SetActive(bool value)
    {
        active = value;

        if (value)
        {
            // Force reset to original local transform
            transform.localPosition = cachedLocalPosition;
            transform.localRotation = cachedLocalRotation;

            hitObjects.Clear();
        }

        gameObject.SetActive(value);
    }

    private void Update()
    {
        if (!active) return;

        CheckOverlaps();
    }

    /// <summary>
    /// Manually checks for all overlaps every frame while active.
    /// Fixes missed OnTriggerEnter events.
    /// </summary>
    private void CheckOverlaps()
    {
        if (hitboxCollider == null) return;

        Collider[] hits = Physics.OverlapBox(
            hitboxCollider.bounds.center,
            hitboxCollider.bounds.extents,
            transform.rotation
        );

        foreach (var other in hits)
        {
            TryHit(other);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!active) return;

        TryHit(other);
    }

    /// <summary>
    /// Handles hit/parry logic safely and prevents duplicate hits.
    /// </summary>
    private void TryHit(Collider other)
    {
        if (other == null) return;

        // Prevent duplicate hits
        if (hitObjects.Contains(other.gameObject))
            return;

        // Don't hit player
        if (other.CompareTag("Player"))
            return;

        hitObjects.Add(other.gameObject);

        Debug.Log("HIT DETECTED WITH: " + other.name);

        IDamagable dmg = other.GetComponentInParent<IDamagable>();
        if (dmg != null)
        {
            Debug.Log("Hit " + dmg + " for " + pc.SwordDamage + " damage");
            dmg.TakeDamage(pc.SwordDamage);
            pc.PlayParryImpactSFX();
        }

        IParryable parryableObj = other.GetComponentInParent<IParryable>();
        if (parryableObj != null)
        {
            parryableObj.Parry(pc.PlayerCamera.transform);
            pc.PlayParryImpactSFX();
        }
    }

    private Vector3 lastPos;

    private void LateUpdate()
    {
        if (!active) return;

        transform.localPosition = cachedLocalPosition;
        transform.localRotation = cachedLocalRotation;
    }
}