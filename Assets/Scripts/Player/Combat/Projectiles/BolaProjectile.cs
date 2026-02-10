using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BolaProjectile : MonoBehaviour
{
    public float pullRange = 2.5f;
    public float pullForce = 20f;
    public float holdTime = 1f;
    public int damage = 5;

    private Rigidbody rb;
    private bool activated;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (activated)
            return;

        activated = true;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        StartCoroutine(BolaEffect());
    }

    private IEnumerator BolaEffect()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pullRange);

        foreach (Collider hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null)
                continue;

            enemy.TakeDamage(damage);
            enemy.NoAI = true;

            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 dir = (transform.position - enemy.transform.position).normalized;
                enemyRb.AddForce(dir * pullForce, ForceMode.VelocityChange);
            }
        }

        yield return new WaitForSeconds(holdTime);

        foreach (Collider hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.NoAI = false;
            }
        }

        Destroy(gameObject);
    }
}
