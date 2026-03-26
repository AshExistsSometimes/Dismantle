using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("References")]
    public Transform VisualModel;
    public Collider DamageTrigger;

    [Header("Settings")]
    public float baseGrowSpeed = 1.0f;// Meters per second
    public float GrowSpeed = 50f;
    public float MaxLength = 50f;
    public int Damage = 10;

    private float currentLength = 1f;

    // ensures ONE hit only
    private HashSet<IDamagable> hitTargets = new HashSet<IDamagable>();

    public void Initialize(float length, int damage)
    {
        MaxLength = length;
        GrowSpeed = baseGrowSpeed * length;
        Damage = damage;

        currentLength = 1f;
        ApplyScale();

        StartCoroutine(GrowRoutine());
    }

    private IEnumerator GrowRoutine()
    {
        while (currentLength < MaxLength)
        {
            currentLength += GrowSpeed * Time.deltaTime;
            currentLength = Mathf.Min(currentLength, MaxLength);

            ApplyScale();
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    private void ApplyScale()
    {
        if (VisualModel == null) return;

        VisualModel.localScale = new Vector3(3f, currentLength, 3f);

        if (DamageTrigger != null)
            DamageTrigger.transform.localScale = VisualModel.localScale;
    }

    private void TryDamage(Collider other)
    {
        IDamagable d = other.GetComponentInParent<IDamagable>();
        if (d == null) return;

        if (hitTargets.Contains(d)) return;

        hitTargets.Add(d);
        d.TakeDamage(Damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }
}