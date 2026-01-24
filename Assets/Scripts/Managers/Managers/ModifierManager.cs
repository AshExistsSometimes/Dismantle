using System.Collections.Generic;
using UnityEngine;

public class ModifierManager : MonoBehaviour, ISaveable
{
    [Header("Modifiers")]
    public int MaxModifiers = 3;
    public List<string> ActiveModifiers = new();

    private void Start()
    {
        SaveManager.Instance.Register("Modifiers", this);
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

    [System.Serializable]
    public class SaveData
    {
        public List<string> activeModifiers;
    }

    public object SaveState()
    {
        return new SaveData
        {
            activeModifiers = ActiveModifiers
        };
    }

    public void LoadState(object data)
    {
        var save = (SaveData)data;
        ActiveModifiers = save.activeModifiers;
    }
}
