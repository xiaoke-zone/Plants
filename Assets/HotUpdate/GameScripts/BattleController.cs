using UnityEngine;
using QFramework;
using System.Collections;

// 【热更层代码】
public class BattleController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameApp.Interface;

    private void Start()
    {
        // 注册监听：当选关面板发送 StartBattleEvent 时执行
        this.RegisterEvent<StartBattleEvent>(OnStartBattle);
    }

    private void OnStartBattle(StartBattleEvent e)
    {
        Debug.Log($"<color=red>战斗系统启动！</color> 开始处理关卡: {e.LevelID}");

        // 1. 获取存档数据
        var saveSystem = GameApp.Interface.GetSystem<ISaveSystem>();
        var levelConfig = saveSystem.SaveData.AllLevels.Find(l => l.LevelID == e.LevelID);

        if (levelConfig != null)
        {
            // 2. 开启协程，根据配置刷僵尸
            StartCoroutine(SpawnZombieRoutine(levelConfig));
        }
    }

    private IEnumerator SpawnZombieRoutine(LevelConfig config)
    {
        var sceneSystem = GameApp.Interface.GetSystem<ISceneSystem>();

        // 简单的刷怪逻辑：每隔 5 秒生成一个本关配置里的僵尸
        foreach (var zombieName in config.ZombieTypes)
        {
            yield return new WaitForSeconds(5.0f);

            // 使用你写的 ISceneSystem 生成僵尸
            sceneSystem.LoadAndInstantiatePrefab(zombieName, null, (go) => {
                // 设置僵尸的初始位置（随机行等）
                float randomY = Random.Range(-3f, 3f);
                go.transform.position = new Vector3(10, randomY, 0);
                Debug.Log($"僵尸 {zombieName} 已出场！");
            });
        }
    }

    private void OnDestroy()
    {
        // 记得取消监听
        this.UnRegisterEvent<StartBattleEvent>(OnStartBattle);
    }
}