using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QFramework;
using UnityEngine;
using YooAsset;



public class InitGameCommand : AbstractCommand
{

    private readonly EPlayMode mPlayMode;

    // 通过构造函数传入参数
    public InitGameCommand(EPlayMode playMode)
    {
        mPlayMode = playMode;
    }

    protected override void OnExecute()
    {
        // 1. 这种写法不需要传入 this，ActionKit 会自动托管到全局的驱动对象上
        ActionKit.Coroutine(Process).Start(GameObject.Find("GameEntry").GetComponent<MonoBehaviour>());
         
      
    }

    //private IEnumerator Process()
    //{
    //    // ... 原有逻辑 ...
    //    yield return null;

    //    var assetSys = this.GetSystem<IAssetSystem>();
    //    var hotUpdateSys = this.GetSystem<IHotUpdateSystem>();

    //    // 1. 初始化资源系统（传入你之前的服务器地址）
    //    // 注意：这里可以根据平台动态拼接地址
    //    string url = "http://127.0.0.1:8000/CDN/PC/v1.0";
    //    //yield return assetSys.InitYooAssets(EPlayMode.HostPlayMode, url);

    //    // 将选择的模式传递给资源系统
    //    yield return assetSys.InitYooAssets(mPlayMode, url);

    //    // 建议在这里加一个保险检查
    //    if (assetSys.Package.InitializeStatus != EOperationStatus.Succeed)
    //    {
    //        Debug.LogError("资源系统未就绪，取消后续加载。");
    //        yield break;
    //    }

    //    // 2. 执行热更新与 DLL 加载
    //    yield return hotUpdateSys.LoadHotUpdateDlls();

    //    // 3. 发送初始化完成事件
    //    this.SendEvent<GameInitedEvent>();
    //}

    private IEnumerator Process()
    {
        var assetSys = this.GetSystem<IAssetSystem>();
        // 必须确保这个协程完全跑完 InitYooAssets 里的所有 yield

        // 1. 初始化资源系统（传入你之前的服务器地址）
        // 注意：这里可以根据平台动态拼接地址
        string url = "http://127.0.0.1:8000/CDN/PC/v1.0";
        yield return assetSys.InitYooAssets(mPlayMode, url);

        // 加个保险检查：如果初始化没成功，不要往下跑
        if (assetSys.Package.InitializeStatus != EOperationStatus.Succeed)
        {
            yield break;
        }

        var hotUpdateSys = this.GetSystem<IHotUpdateSystem>();
        yield return hotUpdateSys.LoadHotUpdateDlls();

        // 3. 发送初始化完成事件
        this.SendEvent<GameInitedEvent>();
    }
}

 

 
