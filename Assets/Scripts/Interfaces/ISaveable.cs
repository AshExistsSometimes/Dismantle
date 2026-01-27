using System.Collections.Generic;

public interface ISaveable
{
    string SaveKey { get; }
    Dictionary<string, string> CaptureSaveData();
    void RestoreSaveData(Dictionary<string, string> data);
}

