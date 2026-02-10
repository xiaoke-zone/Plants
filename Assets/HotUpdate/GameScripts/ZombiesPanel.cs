using UnityEngine;
using QFramework;
using System.Collections;
using System.Collections.Generic;

public class ZombiesPanel : UIBase
{
    public Transform ZombiesRoot;
    private List<Transform> mLanes = new List<Transform>();

    private float mTimer = 0f;
    private float mSpawnInterval = 5f; // 每 5 秒出一只
    private bool mIsGameStarted = false;

    public override void BindComponents()
    {
        ZombiesRoot = transform.Find("ZombiesAllPos");

        // 获取 5 条道路的 Transform
        for (int i = 0; i < ZombiesRoot.childCount; i++)
        {
            mLanes.Add(ZombiesRoot.GetChild(i));
        }
    }

    public override void OnOpen(object data = null)
    {
        // 游戏开始 5 秒后启动
        ActionKit.Delay(5f, () => {
            mIsGameStarted = true;
            Debug.Log("僵尸波次开始！");
        }).Start(this);
    }

    private void Update()
    {
        if (!mIsGameStarted) return;

        mTimer += Time.deltaTime;
        if (mTimer >= mSpawnInterval)
        {
            mTimer = 0f;
            SpawnZombie();
        }
    }

    private void SpawnZombie()
    {
        // 1. 随机选择一条路 (0-4)
        int laneIndex = Random.Range(0, mLanes.Count);
        Transform targetLane = mLanes[laneIndex];

        // 2. 使用对象池加载僵尸
        // 依然建议先通过 YooAsset 加载好 Prefab 引用，或者直接动态加载
        var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
        var handle = package.LoadAssetSync<GameObject>("ZombiePrefab");

        if (handle.AssetObject != null)
        {
            GameObject zombieGo = PoolManager.Instance.Allocate(handle.AssetObject as GameObject, targetLane);

            // 3. 设置初始位置（在道路的最右侧外）
            // 假设道路子物体本身就在屏幕右边缘
            zombieGo.transform.localPosition = new Vector3(0, 0, 0);
            zombieGo.transform.localScale = Vector3.one;

            // 4. 初始化脚本
            var script = zombieGo.GetComponent<ZombieItem>();
            if (script == null) script = zombieGo.AddComponent<ZombieItem>();
            script.Init();
        }
    }
}