using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
 


using UnityEngine;
using QFramework;
using YooAsset; // 记得引入命名空间

public class GameEntry : MonoBehaviour, IController
{
    // 在 Inspector 面板直接下拉选择模式
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    public IArchitecture GetArchitecture() => GameApp.Interface;

//#if UNITY_EDITOR
//    private void OnValidate()
//    {
//        // 只是个建议：默认给编辑器设置成模拟模式
//        PlayMode = EPlayMode.EditorSimulateMode;
//    }
//#endif

    private void Start()
    {
        // 将 Inspector 中选中的模式传给 Command
        this.SendCommand(new InitGameCommand(PlayMode));
    }
}
