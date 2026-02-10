using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HybridCLR;
using QFramework;
using UnityEngine;
using YooAsset;

public interface IHotUpdateSystem : ISystem
{
    IEnumerator LoadHotUpdateDlls();
}

public class HotUpdateSystem : AbstractSystem, IHotUpdateSystem
{
    // --- 修复部分：必须实现这个抽象方法 ---
    protected override void OnInit()
    {
        // 这里可以留空，或者初始化一些系统内部需要的变量
    }
     
    public IEnumerator LoadHotUpdateDlls()
    {
        var assetSystem = this.GetSystem<IAssetSystem>();
        var package = assetSystem.Package;

        // --- 步骤 1: 加载热更 DLL ---
        // 根据你的截图，地址应该是文件名：HotUpdate.dll
        //var handle = package.LoadAssetSync<TextAsset>("HotUpdate.dll");

        //// 等待加载完成（即使是 Sync 模式，在协程里 yield 一下能确保生命周期正确）
        //yield return handle;

        //if (handle.Status != EOperationStatus.Succeed)
        //{
        //    Debug.LogError($"[HotUpdate] 无法找到资源！请检查地址是否为: HotUpdate.dll \n错误详情: {handle.LastError}");
        //    yield break;
        //}

        //TextAsset dllAsset = handle.AssetObject as TextAsset;
        //var hotUpdateAss = System.Reflection.Assembly.Load(dllAsset.bytes);
        //Debug.Log("--- 主热更 DLL 加载成功 ---");


#if !UNITY_EDITOR
    // --- 手机端/发布端：必须从 YooAsset 加载二进制资源 ---
    var handle = package.LoadAssetSync<TextAsset>("HotUpdate.dll");
    yield return handle;
    if (handle.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"[HotUpdate] 无法找到资源！请检查地址是否为: HotUpdate.dll \n错误详情: {handle.LastError}");
            yield break;
        }

        TextAsset dllAsset = handle.AssetObject as TextAsset;
        var hotUpdateAss = System.Reflection.Assembly.Load(dllAsset.bytes);
        Debug.Log("--- 主热更 DLL 加载成功 ---");
#else
        // --- 编辑器模式：直接获取当前项目已有的程序集 ---
        // HybridCLR 在编辑器下会自动编译程序集，我们直接根据名字找即可
        var hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == "HotUpdate");

        // 如果找不到，说明你还没创建这个程序集定义（Assembly Definition）
        if (hotUpdateAss == null)
        {
            Debug.LogError("编辑器下未找到 HotUpdate 程序集，请检查 Assembly Definition 配置");
            yield break;
        }
        Debug.Log("--- 编辑器模式：直接挂载本地程序集成功 ---");
#endif
         

        // --- 步骤 2: 补充 AOT 元数据 ---
        Type cfg = hotUpdateAss.GetType("HotConfig");
        if (cfg != null)
        {
            var aotList = cfg.GetMethod("GetAOTList")?.Invoke(null, null) as List<string>;
            if (aotList != null)
            {
                foreach (var rawPath in aotList)
                {
                    // 【核心修改点】：强行提取文件名
                    // 这样不管传入的是 "Assets/Android/mscorlib.dll.bytes" 还是 "mscorlib.dll"
                    // 最终 address 都会变成 "mscorlib.dll"
                    //string address = System.IO.Path.GetFileName(rawPath).Replace(".bytes", "");

                    //var aotHandle = package.LoadAssetSync<TextAsset>(address);

                    string address = System.IO.Path.GetFileName(rawPath).Replace(".bytes", "");
                    Debug.Log($"[Debug] 正在尝试从 YooAsset 加载 AOT 元数据，地址: {address}");

                    var aotHandle = package.LoadAssetSync<TextAsset>(address);

                    yield return aotHandle;

                    if (aotHandle.Status == EOperationStatus.Succeed)
                    {
                        HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(
                            (aotHandle.AssetObject as TextAsset).bytes,
                            HybridCLR.HomologousImageMode.SuperSet);
                        Debug.Log($"[HybridCLR] 元数据补充成功: {address}");
                    }
                    else
                    {
                        Debug.LogError($"[HybridCLR] 无法加载元数据! 地址: {address}, 错误: {aotHandle.LastError}");
                    }
                }
            }
        }

        // --- 步骤 3: 启动入口 ---
        hotUpdateAss.GetType("Hello")?.GetMethod("Run")?.Invoke(null, null);
    }
}
