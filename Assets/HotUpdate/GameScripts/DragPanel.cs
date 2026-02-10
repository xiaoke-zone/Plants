using UnityEngine;
using QFramework;
using System.Collections.Generic;
using System.Linq;

public class DragPanel : UIBase
{
    public Transform ContentRoot;
    private List<PlantsCard> mCards = new List<PlantsCard>();

    //public override void BindComponents()
    //{
    //    ContentRoot = transform.Find("Scroll View/Viewport/Content");

    //    // 预先获取所有子物体上的 PlantsCard 脚本并隐藏
    //    mCards.Clear();
    //    for (int i = 0; i < ContentRoot.childCount; i++)
    //    {
    //        var card = ContentRoot.GetChild(i).GetComponent<PlantsCard>();
    //        if (card != null)
    //        {
    //            card.BindComponents(); // 确保卡片内部引用就绪
    //            card.gameObject.SetActive(false); // 默认先全部隐藏
    //            mCards.Add(card);
    //        }
    //    }
    //}

    public override void BindComponents()
    {
        ContentRoot = transform.Find("Scroll View/Viewport/Content");

        mCards.Clear();
        for (int i = 0; i < ContentRoot.childCount; i++)
        {
            GameObject childGo = ContentRoot.GetChild(i).gameObject;

            // 关键点：因为是热更代码，预制体上没有这个脚本，必须手动 AddComponent
            // 使用 QFramework 的 GetOrAddComponent 插件方法更方便
            
            var card = childGo.AddComponent<PlantsCard>(); 
            if (card != null)
            {
                card.BindComponents(); // 执行卡片内部的 Find 逻辑
                card.gameObject.SetActive(false); // 初始隐藏
                mCards.Add(card);
            }
        }
    }

    public override void OnOpen(object data = null)
    {
        // 1. 获取当前关卡 ID (从 GlobalData 拿，或者从 data 参数传进来)
        int currentLevelId = GlobalData.SelectedLevelID; 

        // 2. 从存档系统获取截至当前关卡已解锁的所有植物列表
        var saveSystem = this.GetSystem<ISaveSystem>();
        var unlockedPlants = saveSystem.SaveData.AllLevels
            .Where(l => l.LevelID <= currentLevelId && !string.IsNullOrEmpty(l.UnlockPlant))
            .Select(l => l.UnlockPlant)
            .ToList();

        Debug.Log($"当前关卡:{currentLevelId}, 已解锁植物数量:{unlockedPlants.Count}, 第一个植物:{unlockedPlants.FirstOrDefault()}");

        // 3. 根据植物列表数量，控制子物体的显示与数据传递
        for (int i = 0; i < mCards.Count; i++)
        {
            if (i < unlockedPlants.Count)
            {
                // 显示并初始化
                string plantName = unlockedPlants[i];
                mCards[i].gameObject.SetActive(true);

                // 这里我们假设你有一个配置表来获取植物的消耗和冷却
                // 目前先传死数据，或者你可以根据名字做简单的 switch 判断
                //int cost = GetPlantCost(plantName);
                //float cd = GetPlantCooldown(plantName);

                int cost = GlobalData.GetPlantCost(plantName);
                float cd = GlobalData.GetPlantCooldown(plantName); // 直接从这里拿，不用在内部写 switch 了

                mCards[i].Init(plantName, cost, cd);
            }
            else
            {
                // 超出的部分隐藏
                mCards[i].gameObject.SetActive(false);
            }
        }


        // 注册事件监听 (注意：QF 建议在不需要时取消注册，但 UI 生命周期通常随场景)
        // 使用 UnRegisterWhenGameObjectDestroyed 自动处理回收，防止内存泄漏
        TypeEventSystem.Global.Register<SunChangedEvent>(e =>
        {
            RefreshAllCards(e.CurrentSun);
        }).UnRegisterWhenGameObjectDestroyed(gameObject);

        // 初始手动刷新一次
        RefreshAllCards(GlobalData.CurrentSun);

    }

    private void RefreshAllCards(int currentSun)
    {
        foreach (var card in mCards)
        {
            if (card != null && card.gameObject.activeSelf)
            {
                card.OnSunChanged(currentSun);
            }
        }
    }


  
}