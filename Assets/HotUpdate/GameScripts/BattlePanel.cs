 

using UnityEngine;
using QFramework;
using UnityEngine.UI;
using System.Collections.Generic;

// 继承 UIPanel，并显式实现 IController 接口
public class BattlePanel : UIBase
{
    public Transform PlantsRoot;
    public Transform PlantMove;
    private Image mMoveImage;
    private string mCurrentSelectedPlant = "";

    // ... 原有变量 ...
    private bool mIsShovelSelected = false; // 标记当前是否拿着铲子

    public override void BindComponents()
    {
        PlantsRoot = transform.Find("PlantsAllPos");
        PlantMove = transform.Find("PlantMove");
        mMoveImage = PlantMove.GetComponent<Image>();


        // 尽量使用这种方式，即使 Inspector 没拖，代码也能动态找
        if (PlantMove == null)
        {
            PlantMove = transform.Find("PlantMove");
        }

        if (PlantMove != null)
        {
            mMoveImage = PlantMove.GetComponent<Image>();
            PlantMove.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("在 BattlePanel 下没找到名为 'PlantMove' 的子物体！");
        }


        // 默认隐藏虚影
        PlantMove.gameObject.SetActive(false);

        // 给所有的地块按钮绑定事件
        for (int i = 0; i < PlantsRoot.childCount; i++)
        {
            var btn = PlantsRoot.GetChild(i).GetComponent<Button>();
            int index = i; // 闭包陷阱
            btn.onClick.AddListener(() => OnGridClick(index, btn));
        }
    }

    public override void OnOpen(object data = null)
    { 

        // 监听卡片选中（植物）
        TypeEventSystem.Global.Register<OnCardSelectedEvent>(e => {
            mIsShovelSelected = false; // 选了植物，铲子就放下
            mCurrentSelectedPlant = e.PlantName;
            PreparePlantMove(e.PlantName);
        }).UnRegisterWhenGameObjectDestroyed(gameObject);

        // 监听铲子选中
        TypeEventSystem.Global.Register<OnShovelSelectedEvent>(e => {
            mCurrentSelectedPlant = ""; // 选了铲子，植物就放下
            mIsShovelSelected = true;
            PrepareShovelMove();
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void PrepareShovelMove()
    {
        PlantMove.gameObject.SetActive(true);
        // 加载铲子虚影图片
        var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
        var handle = package.LoadAssetAsync<Sprite>("shovel 1"); // 假设名字是 shovel_1
        handle.Completed += (op) => {
            if (op.Status == YooAsset.EOperationStatus.Succeed)
            {
                mMoveImage.sprite = op.AssetObject as Sprite;
                mMoveImage.color = new Color(1, 1, 1, 1f); // 铲子通常不需要半透明
            }
        };
    }

    private void PreparePlantMove(string plantName)
    {
        PlantMove.gameObject.SetActive(true);

        // 加载对应的虚影图片（后缀 _1）
        string spriteName = plantName + "_1";
        var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
        var handle = package.LoadAssetAsync<Sprite>(spriteName);
        handle.Completed += (op) => {
            if (op.Status == YooAsset.EOperationStatus.Succeed)
            {
                mMoveImage.sprite = op.AssetObject as Sprite;
                // 设置为半透明，更有虚影感
                mMoveImage.color = new Color(1, 1, 1, 0.7f);
            }
        };
    }

    private void Update()
    {
        if (PlantMove != null)
        {
            // 如果虚影是开启的，跟随鼠标
            if (PlantMove.gameObject.activeSelf)
            {
                PlantMove.position = Input.mousePosition;

                // 右键取消种植
                if (Input.GetMouseButtonDown(1))
                {
                    CancelPlanting();
                }
            }
        }
         
    }

    //private void OnGridClick(int index, Button gridBtn)
    //{
    //    // --- 逻辑 A：铲除模式 ---
    //    if (mIsShovelSelected)
    //    {
    //        // 查找格子里是否有植物（即是否有子物体）
    //        if (gridBtn.transform.childCount > 0)
    //        {
    //            // 销毁所有子物体（植物预制体）
    //            for (int i = 0; i < gridBtn.transform.childCount; i++)
    //            {
    //                Destroy(gridBtn.transform.GetChild(i).gameObject);
    //            }

    //            // 恢复格子状态
    //            gridBtn.image.color = new Color(1, 1, 1, 1); // 恢复原本格子的显示（如果有）
    //            gridBtn.interactable = true; // 重新变为可种植状态
    //            Debug.Log($"格子 {index} 的植物已被铲除");

    //            CancelPlanting(); // 铲完一次后放下铲子（PvZ 原版逻辑）
    //        }
    //        return;
    //    }

    //    // --- 逻辑 B：种植模式 --- 
    //    if (string.IsNullOrEmpty(mCurrentSelectedPlant)) return;

    //    string plantToSpawn = mCurrentSelectedPlant;
    //    int cost = GlobalData.GetPlantCost(plantToSpawn);

    //    // 1. 扣钱
    //    GlobalData.ChangeSun(-cost);

    //    // 2. 加载植物预制体
    //    // 假设你的预制体名字就叫 Peashooter
    //    var package = YooAsset.YooAssets.GetPackage("DefaultPackage"); 
    //    // 2. 重新加载一份干净的 Sprite（Peashooter_1），不带虚影的透明度干扰
    //    string spriteName = plantToSpawn + "_1";
    //    var handle = package.LoadAssetAsync<Sprite>(spriteName);

    //    handle.Completed += (op) => {
    //        if (op.Status == YooAsset.EOperationStatus.Succeed)
    //        {
    //            // 确保按钮渲染组件是开启的
    //            gridBtn.image.enabled = true;

    //            // 赋值干净的 Sprite
    //            gridBtn.image.sprite = op.AssetObject as Sprite;

    //            // 强制设为完全不透明的白色（恢复原始贴图颜色）
    //            gridBtn.image.color = new Color(1f, 1f, 1f, 1f);

    //            // 禁用交互，防止重叠种植
    //            gridBtn.interactable = false;
    //        }
    //    };



    //    CancelPlanting();
    //}


    private void OnGridClick(int index, Button gridBtn)
    {
        // 判断当前格子上是否有植物（检查子物体数量）
        // 注意：如果你目前是用“换图片”的方式，这个判断可能失效，建议统一使用 Prefab 实例化到 gridBtn 下
        bool hasPlant = gridBtn.transform.childCount > 1;

        // --- 逻辑 A：铲除模式 ---
        if (mIsShovelSelected)
        {
            if (hasPlant)
            {
                // 1. 销毁格子里所有的植物预制体
                foreach (Transform child in gridBtn.transform)
                {
                    Destroy(child.gameObject);
                }

                // 2. 视觉恢复：如果是换图片的逻辑，把图片清空
                gridBtn.image.sprite = null;
                // 如果你的地块平时是透明的，这里设为 0；如果有底色，设为 1
                gridBtn.image.color = new Color(1, 1, 1, 0);

                Debug.Log($"格子 {index} 的植物已被铲除");
            }
            else
            {
                Debug.Log("这里是空地，铲了个寂寞");
            }

            // 铲完（或点到空地）后，按照 PvZ 逻辑，放下铲子
            CancelPlanting();
            return;
        }

        // --- 逻辑 B：种植模式 ---
        if (!string.IsNullOrEmpty(mCurrentSelectedPlant))
        {
            // 【关键检查】如果这里已经有植物了，直接拦截，防止重叠种植
            if (hasPlant)
            {
                Debug.Log($"格子 {index} 已经有植物了，不能重叠种植！");
                return;
            }

            string plantToSpawn = mCurrentSelectedPlant;
            int cost = GlobalData.GetPlantCost(plantToSpawn);

            // 1. 扣钱
            GlobalData.ChangeSun(-cost);

            // 2. 加载植物预制体 (Prefab) 
            // 建议：不要加载 Sprite，直接加载名为 plantToSpawn 的 Prefab
            var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
            var handle = package.LoadAssetAsync<GameObject>(plantToSpawn + "Prefab");

            handle.Completed += (op) =>
            {
                if (op.Status == YooAsset.EOperationStatus.Succeed)
                {
                    // 实例化植物到格子下
                    GameObject go = Instantiate(op.AssetObject as GameObject, gridBtn.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one;

                    // 挂载热更逻辑脚本
                    AddPlantLogic(go, plantToSpawn);

                    // 视觉处理：让格子本身的 Button 图片透明（只显示 Prefab）
                    gridBtn.image.color = new Color(1, 1, 1, 0);

                    // 【重要】保持按钮可用，否则铲子点不到这里！
                    gridBtn.interactable = true;
                }
            };

            // 3. 种植动作完成，收起虚影
            CancelPlanting();
        }
    }

    private void CancelPlanting()
    {
        mCurrentSelectedPlant = "";
        mIsShovelSelected = false; // 增加铲子状态重置
        PlantMove.gameObject.SetActive(false);
    }

   

    /// <summary>
    /// 根据植物名字挂载对应的热更脚本
    /// </summary>
    private void AddPlantLogic(GameObject go, string plantName)
    {
        switch (plantName)
        {
            case "Peashooter":
                go.AddComponent<Peashooter>().Init();
                break;
            case "SunFlower":
                // go.AddComponent<SunFlower>().Init();
                break;
                // 后面可以继续扩展
        }
    }
}
