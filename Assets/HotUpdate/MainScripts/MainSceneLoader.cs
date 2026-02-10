using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MainSceneLoader : ISceneLoader
{
    public string SceneName => "MainScene";

    public void OnLoad(ISceneSystem sceneSystem)
    {
        // 2. 加载场景里的关键预制体（比如主 UI 或 游戏管理器）
        // 假设你有一个地址叫 "MainUIRoot" 的预制体
        sceneSystem.LoadAndInstantiatePrefab("MainUICanvas", null, (go) =>
        {
            Debug.Log(">>> MainUIRoot 加载并实例化完成 <<<");

            // 1. 找到 MainUIPanel 节点并挂载脚本
            Transform mainPanelTrans = go.transform.Find("Bg/MainUIPanel");
            if (mainPanelTrans != null)
            {
                var mainUI = mainPanelTrans.gameObject.AddComponent<MainUIPanel>();
                // 3. 注册并准备逻辑
                SimpleUIManager.Instance.Register("MainUIPanel", mainUI); 

                Debug.Log(">>>  找到 MainUIPanel，并挂载了 脚本 <<<");
            }
            else
            {
                Debug.Log(">>> 没找到 MainUIPanel <<<");
            }

            // 2. 找到 LevelSelectPanel 节点并挂载脚本
            Transform levelSelectTrans = go.transform.Find("Level/LevelSelectPanel");
            if (levelSelectTrans != null)
            {
                var levelUI = levelSelectTrans.gameObject.AddComponent<LevelSelectPanel>();

                // 初始状态隐藏选关界面
                //levelSelectTrans.gameObject.SetActive(false);
                Debug.Log(">>>  找到 LevelSelectPanel，并挂载了 脚本   并且隐藏了  <<<");
                // 3. 注册并准备逻辑
                SimpleUIManager.Instance.Register("LevelSelectPanel", levelUI);

                // 在 MainSceneLoader.cs 中
                levelUI.InitLevelList(); // 异步开始创建，由于物体此时不可见，不会影响性能
            }
            else
            {
                Debug.Log(">>> 没找到 LevelSelectPanel <<<");
            }


            // --- 重点：通过管理器来开启第一个面板 ---
            // 这样 mCurrentPanel 就会被正确赋值为 MainUIPanel
            SimpleUIManager.Instance.OpenPanel("MainUIPanel");


        });
    }
}
