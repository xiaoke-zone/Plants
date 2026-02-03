// 【热更层代码】
using System;
using YooAsset;
using UnityEngine;
using QFramework;

public interface ISceneSystem : ISystem
{
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName">场景资源地址</param>
    /// <param name="onSuccess">加载成功回调</param>
    void LoadScene(string sceneName, Action onSuccess = null);
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
}