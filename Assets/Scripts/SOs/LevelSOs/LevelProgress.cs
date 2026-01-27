[System.Serializable]
public class LevelProgress
{
    public string LevelID;     // SceneName
    public bool Unlocked;
    public bool Played;
    public float BestTime;     // seconds
    public Rank BestRank;
}
