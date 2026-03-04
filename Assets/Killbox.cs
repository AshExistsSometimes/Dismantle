using UnityEngine;

public class Killbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        IDamagable Target = other.GetComponentInParent<IDamagable>();
        if (Target != null)
        {
            Target.Die();
        }
    }
}
