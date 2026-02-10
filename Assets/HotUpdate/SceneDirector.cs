using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using QFramework;

public class SceneDirector
{
    // 存储所有场景加载器的字典
    private static Dictionary<string, ISceneLoader> mLoaders = new Dictionary<string, ISceneLoader>();

    public static void Init()
    {
        // 1. 注册所有的加载器 (这一步也可以自动化，但手动注册更清晰)
        RegisterLoader(new MainSceneLoader());
        RegisterLoader(new GameSceneLoader());
        // 以后加场景：RegisterLoader(new ShopSceneLoader());

        // 2. 监听事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void RegisterLoader(ISceneLoader loader)
    {
        mLoaders[loader.SceneName] = loader;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($">>> 调度员：切换至场景 {scene.name}");

        SimpleUIManager.Instance.ClearAll();
        var sceneSystem = GameApp.Interface.GetSystem<ISceneSystem>();

        // 3. 策略分发：直接找对应的加载器
        if (mLoaders.TryGetValue(scene.name, out var loader))
        {
            loader.OnLoad(sceneSystem);
        }
        else
        {
            Debug.LogWarning($"场景 {scene.name} 没有对应的加载器逻辑。");
        }
    }
}