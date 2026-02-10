using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// 【热更层代码】
public static class MainSceneLauncher
{
    public static void Launch()
    {
        var sceneSystem = GameApp.Interface.GetSystem<ISceneSystem>();


        sceneSystem.LoadScene("MainScene");
        
    }
}
