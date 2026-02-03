using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using HybridCLR;
using QFramework;

public class Hello
{
      
    public static void Run()
    {
        Debug.Log("Hello, HybridCLR     111111111 ");

        Debug.Log(">>> 热更程序集已启动！<<<");

        // 1. 动态注册热更层的 System（因为 AOT 层不知道这些类的存在）
        GameApp.Interface.RegisterSystem<ISceneSystem>(new SceneSystem());

        // 2. 调用场景加载
        var sceneSystem = GameApp.Interface.GetSystem<ISceneSystem>();

        sceneSystem.LoadScene("MainScene", () =>
        {
            // 加载成功后的逻辑，比如发送事件关闭 LoadingUI
            GameApp.Interface.SendEvent<GameInitedEvent>();
        });

    }

     
}