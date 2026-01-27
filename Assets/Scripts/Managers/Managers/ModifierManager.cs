using System.Collections.Generic;
using UnityEngine;

public class ModifierManager : MonoBehaviour, ISaveable
{
    public string SaveKey => "ModifierManager";

    [Header("Modifiers")]
    public int MaxModifiers = 3;
    public List<string> ActiveModifiers = new();

    private void Awake()
    {
        SaveManager.Instance?.Register(this);
    }

    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Register(this);
            Debug.Log("[ModifierManager] Registered with SaveManager.");
        }
        else
        {
            Debug.LogError("[ModifierManager] SaveManager not found!");
        }
    }

    public bool TryAddModifier(string modifierID)
    {
        if (ActiveModifiers.Count >= MaxModifiers)
            return false;

        if (!ActiveModifiers.Contains(modifierID))
            ActiveModifiers.Add(modifierID);

        return true;
    }

    public void RemoveModifier(string modifierID)
    {
        ActiveModifiers.Remove(modifierID);
    }

    public Dictionary<string, string> CaptureSaveData()
    {
        return new Dictionary<string, string>
        {
            { "MaxModifiers", SaveUtils.Int(MaxModifiers) },
            { "ActiveModifiers", SaveUtils.StringList(ActiveModifiers) }
        };
    }

    public void RestoreSaveData(Dictionary<string, string> data)
    {
        if (data.TryGetValue("MaxModifiers", out var max))
            MaxModifiers = SaveUtils.ToInt(max);

        if (data.TryGetValue("ActiveModifiers", out var mods))
            ActiveModifiers = SaveUtils.ToStringList(mods);
    }
}
