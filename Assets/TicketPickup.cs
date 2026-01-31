using UnityEngine;

public class TicketPickup : MonoBehaviour
{
    public bool isX5Ticket = false;

    public int TicketValue = 1;

    private LevelManager levelManager;

    private bool givenTicket = false;

    private void Awake()
    {
        if (isX5Ticket)
        {
            TicketValue = 5;
        }
        else
        {
            TicketValue = 1;
        }

        levelManager = FindFirstObjectByType<LevelManager>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!givenTicket)
            {
                givenTicket = true;
                Debug.Log("Giving player " + TicketValue + " tickets");
                if (levelManager != null)
                {
                    levelManager.AddTicket(TicketValue);
                }
                gameObject.SetActive(false);
            }          
        }
    }
}
