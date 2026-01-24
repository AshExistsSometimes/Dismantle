using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour, ISaveable
{
    [Header("Economy")]
    public int PlayerMoney;
    public int PlayerTickets;

    [Header("Owned Items")]
    public List<string> OwnedModifiers = new();
    public List<string> OwnedColourOptions = new();

    private void Start()
    {
        SaveManager.Instance.Register("Inventory", this);
    }

    [System.Serializable]
    public class SaveData
    {
        public int money;
        public int tickets;
        public List<string> modifiers;
        public List<string> colours;
    }

    public object SaveState()
    {
        return new SaveData
        {
            money = PlayerMoney,
            tickets = PlayerTickets,
            modifiers = OwnedModifiers,
            colours = OwnedColourOptions
        };
    }

    public void LoadState(object data)
    {
        var save = (SaveData)data;
        PlayerMoney = save.money;
        PlayerTickets = save.tickets;
        OwnedModifiers = save.modifiers;
        OwnedColourOptions = save.colours;
    }
}
