using UnityEngine;

public class ParryHitbox : MonoBehaviour
{
    private PlayerCombat pc;
    private bool active;

    private void Awake()
    {
        SetActive(false);
    }

    public void Init(PlayerCombat pcReference)
    {
        pc = pcReference;
    }

    public void SetActive(bool value)
    {
        active = value;
        gameObject.SetActive(value);
    }

    private void OnTriggerEnter(Collider other)
    {     
        if (!active) return;

        // Don't hit player
        if (other.CompareTag("Player"))
            return;

        Debug.Log("OVERLAP DETECTED WITH: " + other.name);

        IDamagable dmg = other.GetComponentInParent<IDamagable>();
        if (dmg != null)
        {
            Debug.Log("Hit " +  dmg + " for " + pc.SwordDamage + " damage");
            dmg.TakeDamage(pc.SwordDamage);
        }

        IParryable parryableObj = other.GetComponentInParent<IParryable>();
        if (parryableObj != null)
        {
            parryableObj.Parry(pc.PlayerCamera.transform);
        }
    }

    private Vector3 lastPos;

    private void LateUpdate()
    {
        if (transform.localPosition != lastPos)
        {
            Debug.Log("Hitbox moved to: " + transform.localPosition);
            lastPos = transform.localPosition;
        }
    }
}
