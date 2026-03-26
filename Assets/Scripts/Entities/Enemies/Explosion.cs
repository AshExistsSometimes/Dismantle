using UnityEngine;

public class Explosion : MonoBehaviour
{
    public int Damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        IDamagable dmg = other.GetComponent<IDamagable>();
        if (dmg != null)
        {
            dmg.TakeDamage(Damage);
        }
    }

    public void SetDamage(int dmg)
    {
        Damage = dmg;
    }

    public void SetSize(float size)
    {
        transform.localScale = Vector3.one * size;
    }

    private void Start()
    {
        // auto destroy shortly after spawn
        Destroy(gameObject, 0.1f);
    }
}