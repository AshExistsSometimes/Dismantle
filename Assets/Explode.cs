using UnityEngine;

public class Explode : MonoBehaviour
{
    [SerializeField]
    Rigidbody[] bodyParts;

    [SerializeField]
    float force = 100f;

    [SerializeField]
    float exploForce = 100f;

    [SerializeField]
    float range = 5f;

    [SerializeField]
    float upMod = 5f;

    public void SetExplode(Vector3 linVel, float damage)
    {
        foreach (var part in bodyParts)
        {
            //part.linearVelocity = linVel;
            //part.AddExplosionForce(force,  (-linVel).normalized * (range * 0.2f), range);

            part.AddForce(-(linVel + (GetRandomVector() * (Vector3.Distance(linVel, part.transform.position) / 10f)) - part.transform.position).normalized * force, ForceMode.VelocityChange);

            part.AddExplosionForce(exploForce, transform.position, range, upMod);
        
        }

        //Debug.DrawLine(transform.position, transform.position + linVel, Color.red, 60f);
        //print(linVel);

        Destroy(gameObject, 10f);
    }

    private Vector3 GetRandomVector()
    {
        return new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)).normalized;
    }
}
