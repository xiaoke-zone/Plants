using UnityEngine;
using QFramework;
using UnityEngine.UI;
using System.Collections.Generic;

// 继承 UIPanel，并显式实现 IController 接口
public class LevelSelectPanel : UIBase
{
    public Button BtnBack;
    public Transform ContentRoot;

    // 用列表存下所有生成的格子，方便刷新
    private List<LevelItem> mLevelItems = new List<LevelItem>();

    public override void BindComponents()
    {
        BtnBack = transform.Find("BtnBack")?.GetComponent<Button>();
        ContentRoot = transform.Find("Scroll View/Viewport/Content");


        BtnBack?.onClick.AddListener(() => SimpleUIManager.Instance.OpenPanel("MainUIPanel"));


        //gameObject.SetActive(false);
    }

     

    /// <summary>
    /// 初始化列表：只在场景加载时跑一次，负责把物体造出来
    /// </summary>
    public void InitLevelList()
    {
        // 如果已经造过了，就不重复造了
        if (mLevelItems.Count > 0) return;

        var levels = this.GetSystem<ISaveSystem>().SaveData.AllLevels;
        var sceneSystem = this.GetSystem<ISceneSystem>();

        foreach (var config in levels)
        {
            sceneSystem.LoadAndInstantiatePrefab("LevelItem", ContentRoot, (go) => {
                var item = go.AddComponent<LevelItem>();

                // 此时还没法 Init(config)，因为异步加载顺序不确定
                // 我们先存进列表
                mLevelItems.Add(item);

                // 如果所有格子都造好了，执行一次整体刷新
                if (mLevelItems.Count == levels.Count)
                {
                    RefreshLevelState();
                }
            });
        }
    }

    /// <summary>
    /// 刷新状态：每次面板打开时调用，负责更新锁定的 UI
    /// </summary>
    public void RefreshLevelState()
    {
        var levels = this.GetSystem<ISaveSystem>().SaveData.AllLevels;
        var sceneSystem = this.GetSystem<ISceneSystem>();

        // 遍历存档数据，更新对应的格子 UI
        for (int i = 0; i < levels.Count; i++)
        {
            if (i < mLevelItems.Count)
            {
                var config = levels[i];
                var item = mLevelItems[i];

                // 重新调用 Init 来刷新 UI（显示/隐藏锁，更新按钮交互）
                item.Init(config.LevelID, config.LevelName, config.IsUnlocked, (id) =>
                {
                    GlobalData.SelectedLevelID = id;
                    sceneSystem.LoadScene("GameScene");
                });
            }
        }
    }

    public override void OnOpen(object data = null)
    {

        //// 1. 先清理旧的关卡格子 (防止每次进去格子越来越多)
        //foreach (Transform child in ContentRoot)
        //{
        //    Destroy(child.gameObject);
        //}
        //// 调用 QF 系统获取存档
        //var levels = this.GetSystem<ISaveSystem>().SaveData.AllLevels;
        //var sceneSystem = this.GetSystem<ISceneSystem>();

        //foreach (var config in levels)
        //{
        //    // 使用你定义的基于 YooAsset 的加载方法
        //    sceneSystem.LoadAndInstantiatePrefab("LevelItem", ContentRoot, (go) => {
        //        var item = go.AddComponent<LevelItem>(); // 同样手动挂载

        //        item.Init(config.LevelID, config.LevelName, config.IsUnlocked, (id) =>
        //        {
        //            Debug.Log(id);
        //            GlobalData.SelectedLevelID = id; // 记录要打哪一关
        //            sceneSystem.LoadScene("GameScene");
        //        });
        //    });
        //}


        // 每次面板打开（比如从战斗回来），都刷新一遍最新存档状态
        RefreshLevelState();
    }
}