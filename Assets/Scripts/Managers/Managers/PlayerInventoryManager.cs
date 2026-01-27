using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour, ISaveable
{
    public string SaveKey => "PlayerInventoryManager";

    [Header("Economy")]
    public int PlayerMoney;
    public int PlayerTickets;

    [Header("Owned Items")]
    public List<string> OwnedModifiers = new();
    public List<string> OwnedColourOptions = new();


    private void Awake()
    {
        SaveManager.Instance?.Register(this);
    }

    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Register(this);
            Debug.Log("[PlayerInventoryManager] Registered with SaveManager.");
        }
        else
        {
            Debug.LogError("[PlayerInventoryManager] SaveManager not found!");
        }
    }

    private void OnDestroy()
    {
        SaveManager.Instance.Unregister(this);
    }

    public Dictionary<string, string> CaptureSaveData()
    {
        return new Dictionary<string, string>
        {
            { "Money", PlayerMoney.ToString() },
            { "Tickets", PlayerTickets.ToString() },
            { "Colours", OwnedColourOptions.Count == 0 ? "(none)" : string.Join(", ", OwnedColourOptions) }
        };
    }

    public void RestoreSaveData(Dictionary<string, string> data)
    {
        if (data.TryGetValue("Money", out var money))
            PlayerMoney = int.Parse(money);

        if (data.TryGetValue("Tickets", out var tickets))
            PlayerTickets = int.Parse(tickets);

        if (data.TryGetValue("Colours", out var colours) && colours != "(none)")
            OwnedColourOptions = new List<string>(colours.Split(", "));
    }
}
