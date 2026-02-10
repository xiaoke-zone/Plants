// 【热更层代码】
using System;
using YooAsset;
using UnityEngine;
using QFramework;

// 【热更层代码】
public interface ISceneSystem : ISystem
{
    void LoadScene(string sceneName, Action onSuccess = null);
    // 新增：加载预制体并实例化的简化接口
    void LoadAndInstantiatePrefab(string location, Transform parent = null, Action<GameObject> onComplete = null);
}

public class SceneSystem : AbstractSystem, ISceneSystem
{
    protected override void OnInit() { }

    public void LoadScene(string sceneName, Action onSuccess = null)
    {
        var package = YooAssets.GetPackage("DefaultPackage");
        var handle = package.LoadSceneAsync(sceneName);

        handle.Completed += (op) =>
        {
            if (op.Status == EOperationStatus.Succeed)
            {
                Debug.Log($">>> 场景 {sceneName} 加载成功！<<<");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError($"场景 {sceneName} 加载失败: {op.LastError}");
            }
        };
    }

    public void LoadAndInstantiatePrefab(string location, Transform parent = null, Action<GameObject> onComplete = null)
    {
        var package = YooAssets.GetPackage("DefaultPackage");
        var handle = package.LoadAssetAsync<GameObject>(location);
        handle.Completed += (op) => {
            if (op.Status == EOperationStatus.Succeed)
            {
                var go = GameObject.Instantiate(handle.AssetObject as GameObject, parent);
                onComplete?.Invoke(go);
            }
        };
    }
}