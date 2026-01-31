using UnityEngine;

public class TicketSpawner : MonoBehaviour
{
    public GameObject x1Ticket;
    public GameObject x5Ticket;

    public bool x5TicketSpawner;
    public bool spawnRandomTicket;

    [Range(0f, 1f)]
    public float x5TicketChance = 0.1f;

    private GameObject ticketToSpawn;

    private float heightFixOffset = 0.3f;

    private void Awake()
    {
        if (!spawnRandomTicket)
        {
            if (!x5TicketSpawner)
            {
                // Spawn x1 Ticket
                Instantiate(x1Ticket, transform.position - new Vector3(0, heightFixOffset, 0), transform.rotation);
            }
            else
            {
                // Spawn x5 Ticket
                Instantiate(x5Ticket, transform.position - new Vector3(0, heightFixOffset, 0), transform.rotation);
            }

            // Remove spawner
            Destroy(gameObject);
        }
        else
        {
            SpawnRandomTicket();
            Destroy(gameObject);
        }
    }

    private void SpawnRandomTicket()
    {
        float roll = Random.value;

        // Decide which ticket to spawn
        if (roll < x5TicketChance)
        {
            ticketToSpawn = x5Ticket;
        }
        else
        {
            ticketToSpawn = x1Ticket;
        }

        // Spawn ticket
        Instantiate(ticketToSpawn, transform.position - new Vector3(0, heightFixOffset, 0), transform.rotation);
    }
}

