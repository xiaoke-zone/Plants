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

        // 1. 第一步：初始化场景调度员
        SceneDirector.Init();

        // 1. 动态注册热更层的 System（因为 AOT 层不知道这些类的存在） 
        // 必须注册，否则 GetSystem 会找不到
        GameApp.Interface.RegisterSystem<ISaveSystem>(new SaveSystem());
        GameApp.Interface.RegisterSystem<ISceneSystem>(new SceneSystem());

        // 2. 调用场景加载 

        // 启动主场景业务流
        MainSceneLauncher.Launch();

    }

     
}