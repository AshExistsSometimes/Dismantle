using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : MonoBehaviour, IDamagable
{
    public UnityEvent OnBreak;

    public int MaxHP = 1;
    private int CurrentHP;

    private void Awake()
    {
        CurrentHP = MaxHP;
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // Particles
        OnBreak.Invoke();
        gameObject.SetActive(false);
    }
}
