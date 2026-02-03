using QFramework;
using System.Collections;
using UnityEngine;
using YooAsset;


public interface IAssetSystem : ISystem
{
    // 提供给外部（如 HotUpdateSystem）获取 YooAsset 的 Package 对象
    ResourcePackage Package { get; }

    // 提供给 InitGameCommand 调用的一键初始化/更新方法
    IEnumerator InitYooAssets(EPlayMode playMode, string hostServer);
}

public class YooAssetSystem : AbstractSystem, IAssetSystem
{
    public ResourcePackage Package { get; private set; }
    public int DownloadingMaxNum = 10;
    public int FailedTryAgain = 3;

    protected override void OnInit() { }

    public IEnumerator InitYooAssets(EPlayMode playMode, string hostServer)
    {
        // 1. 初始化
        YooAssets.Initialize();
        //Package = YooAssets.CreatePackage("DefaultPackage");
        Package = YooAssets.TryGetPackage("DefaultPackage");
        if (Package == null)
        {
            Package = YooAssets.CreatePackage("DefaultPackage");
            Debug.Log("package   为空， 新创建一个   ");
        }
        else
        {
            Debug.Log("package  不为空，获取成功了 ");
        }

        YooAssets.SetDefaultPackage(Package);

        // 2. 参数配置 (补充了模拟模式)
        InitializationOperation initOp = null; 
        //if (playMode == EPlayMode.EditorSimulateMode)
        //{
        //    var playParams = new EditorSimulateModeParameters();
        //    playParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(Package.PackageName);
        //    initOp = Package.InitializeAsync(playParams);
        //}

        if (playMode == EPlayMode.EditorSimulateMode)
        {
            //var playParams = new EditorSimulateModeParameters();
            //// 模拟模式需要根据 PackageName 自动匹配清单
            //playParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(Package.PackageName);
            //initOp = Package.InitializeAsync(playParams);


            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = fileSystemParams;


            //InitializationOperation initializationOperation = Package.InitializeAsync(createParameters);
            initOp = Package.InitializeAsync(createParameters);

        }
        else if (playMode == EPlayMode.HostPlayMode)
        {
            //var remoteServices = new RemoteServices(hostServer, hostServer);
            //var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            //var playParams = new HostPlayModeParameters
            //{
            //    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(),
            //    CacheFileSystemParameters = cacheParams
            //};
            //initOp = Package.InitializeAsync(playParams);


            IRemoteServices remoteServices = new RemoteServices(hostServer, hostServer);


            // 初始化注意事项
            // 注意：设置参数COPY_BUILDIN_PACKAGE_MANIFEST，可以初始化的时候拷贝内置清单到沙盒目录
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            buildinFileSystemParams.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST, true);

            // 注意：设置参数INSTALL_CLEAR_MODE，可以解决覆盖安装的时候将拷贝的内置清单文件清理的问题。
            var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            cacheFileSystemParams.AddParameter(FileSystemParametersDefine.INSTALL_CLEAR_MODE, EOverwriteInstallClearMode.None);

            var playModeParameters = new HostPlayModeParameters();
            playModeParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            playModeParameters.CacheFileSystemParameters = cacheFileSystemParams; 


            //InitializationOperation initializationOperation = Package.InitializeAsync(playModeParameters);
            initOp = Package.InitializeAsync(playModeParameters);


        }

        yield return initOp;

        if (initOp.Status == EOperationStatus.Succeed)
        {

            if (playMode == EPlayMode.HostPlayMode)
            {
                yield return UpdateResourceProcess();
            }
            else
            {
                // 【核心修改】：编辑器模拟模式也需要获取版本并更新清单（只是它是从本地配置生成的）
                var versionOp = Package.RequestPackageVersionAsync();
                yield return versionOp;

                if (versionOp.Status == EOperationStatus.Succeed)
                {
                    // 必须执行这一步，才会正式激活（Active）模拟清单
                    var manifestOp = Package.UpdatePackageManifestAsync(versionOp.PackageVersion);
                    yield return manifestOp;

                    Debug.Log($"资源系统: 模拟模式就绪，模拟版本号: {versionOp.PackageVersion}");
                }
                else
                {
                    Debug.LogError($"模拟模式获取版本失败: {versionOp.Error}");
                }
            }
        }
        else
        {
          
            Debug.LogError($"YooAsset 初始化失败: {initOp.Error}");
            this.SendEvent(new AssetUpdateErrorEvent { Error = initOp.Error });
        }
    }

    private IEnumerator UpdateResourceProcess()
    {
        // 1. 获取最新版本
        var versionOp = Package.RequestPackageVersionAsync(true, 10);
        yield return versionOp;

        string targetVersion = "";
        if (versionOp.Status == EOperationStatus.Succeed)
        {
            targetVersion = versionOp.PackageVersion;
        }
        else
        {
            // 失败则尝试读取本地缓存版本
            targetVersion = PlayerPrefs.GetString("GAME_VERSION", string.Empty);
            if (string.IsNullOrEmpty(targetVersion))
            {
                this.SendEvent(new AssetUpdateErrorEvent { Error = "网络连接失败且本地无备份版本" });
                yield break;
            }
        }

        // 2. 更新清单
        var manifestOp = Package.UpdatePackageManifestAsync(targetVersion);
        yield return manifestOp;
        if (manifestOp.Status != EOperationStatus.Succeed)
        {
            this.SendEvent(new AssetUpdateErrorEvent { Error = "更新清单失败" });
            yield break;
        }

        // 3. 准备下载
        var downloader = Package.CreateResourceDownloader(DownloadingMaxNum, FailedTryAgain);

        if (downloader.TotalDownloadCount > 0)
        {
            // --- 绑定 QFramework 事件回调 ---
            downloader.DownloadUpdateCallback = (data) => {
                float progress = (float)data.CurrentDownloadBytes / data.TotalDownloadBytes;
                this.SendEvent(new AssetDownloadUpdateEvent
                {
                    Progress = progress,
                    DownloadSpeed = $"{(data.CurrentDownloadBytes / 1048576f):F2}MB/{(data.TotalDownloadBytes / 1048576f):F2}MB"
                });
            };

            downloader.BeginDownload();
            yield return downloader;

            if (downloader.Status != EOperationStatus.Succeed)
            {
                this.SendEvent(new AssetUpdateErrorEvent { Error = "资源下载中断" });
                yield break;
            }
        }

        // 4. 清理旧缓存 (重要：清理完才算真正结束)
        var clearOp = Package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        yield return clearOp;

        // 5. 保存版本号
        PlayerPrefs.SetString("GAME_VERSION", targetVersion);
        Debug.Log($"资源系统就绪，当前版本: {targetVersion}");
    }
}