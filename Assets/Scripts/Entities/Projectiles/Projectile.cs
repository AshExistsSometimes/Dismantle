using UnityEngine;
public class Projectile : MonoBehaviour, IParryable
{
    public float Lifetime = 6f;

    private int damage;
    private float speed;
    private Vector3 direction;
    private bool targetingPlayer;

    private float lifeTimer;

    public void Initialize(int dmg, Vector3 dir, float moveSpeed, bool hitPlayer)
    {
        damage = dmg;
        direction = dir.normalized;
        speed = moveSpeed;
        targetingPlayer = hitPlayer;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= Lifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetingPlayer)
        {
            if (!other.CompareTag("Player"))
                return;
        }
        else
        {
            if (other.CompareTag("Player"))
                return;
        }

        IDamagable dmg = other.GetComponent<IDamagable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    public void Parry(Transform cameraTransform)
    {
        direction = cameraTransform.forward.normalized;
        targetingPlayer = false;
    }
}

