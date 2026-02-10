using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameSceneLoader : ISceneLoader
{
    public string SceneName => "GameScene";

    public void OnLoad(ISceneSystem sceneSystem)
    {
        
        // 2. 加载场景里的关键预制体（比如主 UI 或 游戏管理器）
        // 假设你有一个地址叫 "BattleCanvas" 的预制体
        sceneSystem.LoadAndInstantiatePrefab("BattleCanvas", null, (go) =>
        {
            Debug.Log(">>> BattleCanvas 加载并实例化完成 <<<");

            // 1. 找到 MainUIPanel 节点并挂载脚本
            Transform battlePanelTrans = go.transform.Find("Battle/BattlePanel");
            if (battlePanelTrans != null)
            {
                var battleUI = battlePanelTrans.gameObject.AddComponent<BattlePanel>();
                // 3. 注册并准备逻辑
                SimpleUIManager.Instance.Register("BattlePanel", battleUI);
                 
                Debug.Log(">>>  找到 BattlePanel，并挂载了 脚本 <<<");
            }

            // 1.1 找到 ZombiesPanel 节点并挂载脚本
            Transform zombiesPanelTrans = go.transform.Find("Zombies/ZombiesPanel");
            if (zombiesPanelTrans != null)
            {
                var zombiesUI = zombiesPanelTrans.gameObject.AddComponent<ZombiesPanel>();
                // 3. 注册并准备逻辑
                SimpleUIManager.Instance.Register("ZombiesPanel", zombiesUI);

                Debug.Log(">>>  找到 ZombiesPanel，并挂载了 脚本 <<<");
            }


            // 2. 找到 DragPanel 节点并挂载脚本
            Transform dragPanelTrans = go.transform.Find("Drag/DragPanel");
            if (dragPanelTrans != null)
            {
                var dragUI = dragPanelTrans.gameObject.AddComponent<DragPanel>();

            
                // 3. 注册并准备逻辑
                SimpleUIManager.Instance.Register("DragPanel", dragUI);
            }

            // 3. 找到 SunPanel 节点并挂载脚本
            Transform sunPanelTrans = go.transform.Find("Sun/SunPanel");
            if (sunPanelTrans != null)
            {
                var sunUI = sunPanelTrans.gameObject.AddComponent<SunPanel>();
                 
                // 3. 注册并准备逻辑
                SimpleUIManager.Instance.Register("SunPanel", sunUI);
            }

            // --- 重点：通过管理器来开启第一个面板 --- 
            SimpleUIManager.Instance.ShowPanel("BattlePanel");
            SimpleUIManager.Instance.ShowPanel("ZombiesPanel");
            SimpleUIManager.Instance.ShowPanel("DragPanel");   
            SimpleUIManager.Instance.ShowPanel("SunPanel");
        });
    }
}
