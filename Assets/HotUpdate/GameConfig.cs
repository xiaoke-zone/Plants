using System.Collections.Generic;

[System.Serializable]
public class LevelConfig
{
    public int LevelID;
    public string LevelName;
    public bool IsUnlocked;
    public List<string> ZombieTypes; // 本关会出现的僵尸
    public string UnlockPlant;       // 本关通过后解锁的植物名
}

[System.Serializable]
public class GameSaveData
{
    public int CurrentMaxLevel;
    public List<LevelConfig> AllLevels;
    // 还可以记录玩家拥有的金币、已经解锁的植物列表等
    public List<string> OwnedPlants = new List<string> { "Peashooter", "SunFlower" };

}