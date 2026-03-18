using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShotgunBullet : MonoBehaviour, IParryable
{
    [Header("Config")]
    public float speed = 80f;
    public float maxLifetime = 2f;

    [HideInInspector]
    public float maxDamage;

    [HideInInspector]
    public AnimationCurve damageFalloff;

    private Vector3 direction;
    private float traveledDistance;
    private bool hasHit;

    private void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    public void Fire(Vector3 dir, float damage, AnimationCurve falloff)
    {
        direction = dir.normalized;
        maxDamage = damage;
        damageFalloff = falloff;
    }

    private void Update()
    {
        if (hasHit) return;

        float step = speed * Time.deltaTime;
        Ray ray = new Ray(transform.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, step))
        {
            HandleHit(hit);
        }
        else
        {
            transform.position += direction * step;
            traveledDistance += step;
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        hasHit = true;

        float falloff = damageFalloff != null
            ? damageFalloff.Evaluate(traveledDistance)
            : 1f;

        int finalDamage = Mathf.RoundToInt(maxDamage * falloff);

        IDamagable damagableTarget = hit.collider.GetComponentInParent<IDamagable>();
        if (damagableTarget != null)
        {
            damagableTarget.TakeDamage(finalDamage);
        }

        Destroy(gameObject);
    }

    public void Parry(Transform cameraTransform)
    {
        direction = cameraTransform.forward.normalized;
        speed = speed * 1.5f;
    }
}
