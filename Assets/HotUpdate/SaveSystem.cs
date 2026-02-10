using System.Collections.Generic;
using System.IO;
using UnityEngine;
using QFramework;
using Newtonsoft.Json;

public interface ISaveSystem : ISystem
{
    GameSaveData SaveData { get; }
    void Save();
    void Load();
}

public class SaveSystem : AbstractSystem, ISaveSystem
{
    private string mSavePath;
    public GameSaveData SaveData { get; private set; }

    protected override void OnInit()
    {
        // 定义存档路径
        //mSavePath = Path.Combine(Application.persistentDataPath, "PvZ_SaveData.json");

        // 使用预编译指令判断环境
#if UNITY_EDITOR
        // 如果是在 Unity 编辑器里运行
        // 我们将路径指向你指定的 D 盘目录
        string editorFolder = @"D:\UnityExamples2025\UnityAdvanced\QFrameworkPlants\Plants\Apks";

        // 确保文件夹存在，如果不存在则创建一个
        if (!System.IO.Directory.Exists(editorFolder))
        {
            System.IO.Directory.CreateDirectory(editorFolder);
        }

        mSavePath = Path.Combine(editorFolder, "PvZ_SaveData.json");
        Debug.Log($"<color=cyan>编辑器模式：存档路径已重定向至: </color>{mSavePath}");
#else
        // 如果是在手机（Apk）或打包出来的 PC 端运行
        mSavePath = Path.Combine(Application.persistentDataPath, "PvZ_SaveData.json");
        Debug.Log($"<color=green>发布模式：存档路径使用系统默认: </color>{mSavePath}");
#endif


        Load();
    }

    public void Save()
    {
        string json = JsonConvert.SerializeObject(SaveData, Formatting.Indented);
        File.WriteAllText(mSavePath, json);
        Debug.Log($"<color=green>存档成功:</color> {mSavePath}");
    }

    public void Load()
    {
        if (!File.Exists(mSavePath))
        {
            Debug.Log("未发现存档，正在初始化默认关卡数据...");
            InitDefaultData();
            return;
        }

        try
        {
            string json = File.ReadAllText(mSavePath);
            SaveData = JsonConvert.DeserializeObject<GameSaveData>(json);
            Debug.Log("<color=yellow>存档加载完成</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"存档解析失败: {e.Message}");
            InitDefaultData();
        }
    }

    private void InitDefaultData()
    {
        SaveData = new GameSaveData
        {
            CurrentMaxLevel = 1,
            AllLevels = new List<LevelConfig>()
        };

        // 植物池定义
        string[] allPlantPool = { "Peashooter", "SunFlower", "WallNut", "PotatoMine", "SnowPea" };

        for (int i = 1; i <= 5; i++)
        {
            var level = new LevelConfig
            {
                LevelID = i,
                LevelName = $"关卡 1-{i}",
                IsUnlocked = (i == 1),
                ZombieTypes = new List<string> { "Zombie" },

                // --- 修改点：使用 i-1 确保第一关解锁索引 0 的植物 ---
                UnlockPlant = (i - 1) < allPlantPool.Length ? allPlantPool[i - 1] : ""
            };

            // 难度动态配置
            if (i >= 2) level.ZombieTypes.Add("ConeheadZombie");
            if (i >= 4) level.ZombieTypes.Add("BucketheadZombie");

            SaveData.AllLevels.Add(level);
        }

        Save(); // 第一次运行立即持久化
    }
}