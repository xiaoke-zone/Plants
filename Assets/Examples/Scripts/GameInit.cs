
using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic; 
using System.IO;
using System.Linq;
using System.Reflection; 
using UnityEngine; 
using YooAsset;


public class GameInit : MonoBehaviour
{
    public EPlayMode PlayMode = EPlayMode.HostPlayMode;
    public string packageName = "DefaultPackage";
    public string packageVersion = "";

    public string defaultHostServer = "http://127.0.0.1:8000/CDN/PC/v1.0";
    public string fallbackHostServer = "http://127.0.0.1:8000/CDN/PC/v1.0";

    public int downloadingMaxNum = 10;
    public int filedTryAgain = 3;

    private ResourcePackage package;
    private ResourceDownloaderOperation downloader;
    public string localCacheRoot = "";

    //  防止按钮多次点击
    private bool isUpdating = false;

    //   YooAsset 是否已初始化
    private bool isYooInitialized = false;

    // 外部回调
    public Action<bool> OnUpdateFinish;

    private string _ipState;
    // 值变化事件
    public event Action<string> OnValueChanged;

    public string ipState
    {
        get => _ipState;
        set
        {
            if (_ipState != value)
            {
                _ipState = value;
                OnValueChanged?.Invoke(_ipState);
            }
        }
    }


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        localCacheRoot = Path.Combine(Application.persistentDataPath, "AssetBundles");
    }

    public void SetHostIP(string ip)
    {
        // 判断平台
        string targetPlatform;

#if UNITY_STANDALONE   // PC / Mac / Linux 构建
        targetPlatform = "PC";
#elif UNITY_ANDROID
        targetPlatform = "Android";
#else
    targetPlatform = "PC";  // 默认 PC
#endif

        // 拼出最终地址
        defaultHostServer = $"http://{ip}:8000/CDN/{targetPlatform}/v1.0";
        fallbackHostServer = $"http://{ip}:8000/CDN/{targetPlatform}/v1.0";

        Debug.Log("defaultHostServer = " + defaultHostServer);
        Debug.Log("fallbackHostServer = " + fallbackHostServer);
    }

    //==========================================
    // 按钮调用入口
    //==========================================
    public void StartUpdateFromButton()
    {
        if (isUpdating)
        {
            Debug.Log(" 正在更新中，按钮点击被忽略");
            return;
        }

        Debug.Log("开始执行更新流程"); 

#if UNITY_EDITOR
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            StartCoroutine(StartUpdateEditor());
        }
        else if (PlayMode == EPlayMode.HostPlayMode)
        {

            StartCoroutine(StartUpdateLocal());
        }

#elif UNITY_ANDROID
     StartCoroutine(StartUpdateLocal());

#elif UNITY_STANDALONE_WIN
     StartCoroutine(StartUpdateLocal());

#else
    Debug.LogError("当前平台未处理 HotUpdate 加载逻辑");
#endif

    }

    IEnumerator StartUpdateEditor()
    {
        isUpdating = true;

        yield return null;

        //----------【初始化 YooAsset】--------
        if (!isYooInitialized)
        {
            Debug.Log("初始化 YooAsset...");


            // 1. 初始化
            YooAssets.Initialize();

            package = YooAssets.TryGetPackage(packageName);
            if (package == null)
            {
                package = YooAssets.CreatePackage(packageName);
            }


            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = fileSystemParams;


            InitializationOperation initializationOperation = package.InitializeAsync(createParameters);

            yield return initializationOperation;
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(initializationOperation.Error);
                FinishUpdate(false);
            }
            else
            {
                Debug.Log("-------------初始化成功-------------");
                isYooInitialized = true;  //  标记仅初始化一次
            }

        }
         

        //  2.获取资源版本

        var operation = package.RequestPackageVersionAsync();
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            FinishUpdate(false);
        }
        else
        {
            Debug.Log($" 请求的版本 ： {operation.PackageVersion}");
            packageVersion = operation.PackageVersion;


        }
         

        //  3.  获取资源清单

        var operationManifest = package.UpdatePackageManifestAsync(packageVersion);
        yield return operationManifest;
        if (operationManifest.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operationManifest.Error);
            FinishUpdate(false);

        }
        else
        {
            Debug.Log("----------更新资源清单成功 ----------");
        }

        //4. 创建下载器
         
        downloader = package.CreateResourceDownloader(downloadingMaxNum, filedTryAgain);

        Debug.Log(" 创造下载器 成功 ");
        //yield return downloader;
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("没有需要更新的文件 ");
            FinishUpdate(true);

            UpdateDone();     // 普通 同步和异步方式         Task 资源加载也是这样的 

            yield break;
        }
        else
        {
            int count = downloader.TotalDownloadCount;
            long bytes = downloader.TotalDownloadBytes;
            Debug.Log($" 需要更新  {count}个文件 ，大小是  {bytes / 1024 / 1024} MB");
        }





        //----------【绑定事件】--------
        downloader.DownloadFinishCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）
        downloader.DownloadErrorCallback = OnDownloadErrorFunction; //当下载器发生错误
        downloader.DownloadUpdateCallback = OnDownloadUpdateFunction; //当下载进度发生变化
                                                                      //
        downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction; //当开始下载某个文件



        //----------【开始下载】--------
        downloader.BeginDownload();
        yield return downloader;

        if (downloader.Status != EOperationStatus.Succeed)
        {
            Debug.LogError("部分资源下载失败");
            FinishUpdate(false);
            yield break;
        }

        Debug.Log("所有资源文件下载完成");





        // 6. 清理文件  

        var operationClear = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        yield return operationClear;

        operationClear.Completed += Operation_Completed;

        if (operationClear.Status == EOperationStatus.Succeed)
        {
            //清理成功
            Debug.Log("--------清理未使用的缓存文件完成---------");
        }
        else
        {
            //清理失败
            Debug.LogWarning(operationClear.Error);
        }


        //----------【更新结束】--------
        FinishUpdate(true);
    }

    //==========================================
    // 热更新流程
    //==========================================
  
    IEnumerator StartUpdateLocal()
    {
        isUpdating = true;

        yield return null;

        //----------【初始化 YooAsset】--------
        if (!isYooInitialized)
        {
            Debug.Log("初始化 YooAsset...");


            // 1. 初始化
            YooAssets.Initialize();

            package = YooAssets.TryGetPackage(packageName);
            if (package == null)
            {
                package = YooAssets.CreatePackage(packageName);
                Debug.Log("package   为空， 新创建一个   ");
            }
            else
            {
                Debug.Log("package  不为空，获取成功了 ");
            }



            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);


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


            InitializationOperation initializationOperation = package.InitializeAsync(playModeParameters);

            yield return initializationOperation;
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(initializationOperation.Error);
                FinishUpdate(false);
            }
            else
            {
                Debug.Log("-------------初始化成功-------------");
                isYooInitialized = true;  //  标记仅初始化一次
            }

        }


        // 先获取远端最新的资源版本
        var versionOp = package.RequestPackageVersionAsync(true, 10);
        yield return versionOp;
        if (versionOp.Status == EOperationStatus.Succeed)
        {

            packageVersion = versionOp.PackageVersion;

            // 如果获取远端资源版本成功，说明当前网络连接通畅，可以走正常更新流程。
            var manifestOp = package.UpdatePackageManifestAsync(versionOp.PackageVersion);
            yield return manifestOp;
            if (manifestOp.Status != EOperationStatus.Succeed)
            {
                //ShowMessageBox("请检查本地网络，资源清单更新失败！");
                FinishUpdate(false);
                yield break;
            }
            else
            {
                Debug.Log("  更新资源清单成功    ");
            }


            //package.CreateResourceDownloader();
            downloader = package.CreateResourceDownloader(downloadingMaxNum, filedTryAgain);

            Debug.Log(" 创造下载器 成功 ");
            //yield return downloader;
            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("没有需要更新的文件 ");
                FinishUpdate(true);

                UpdateDone();     // 普通 同步和异步方式         Task 资源加载也是这样的 

                yield break;
            }
            else
            {
                int count = downloader.TotalDownloadCount;
                long bytes = downloader.TotalDownloadBytes;
                Debug.Log($" 需要更新  {count}个文件 ，大小是  {bytes / 1024 / 1024} MB");
            }
             

            //----------【绑定事件】--------
            downloader.DownloadFinishCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）
            downloader.DownloadErrorCallback = OnDownloadErrorFunction; //当下载器发生错误
            downloader.DownloadUpdateCallback = OnDownloadUpdateFunction; //当下载进度发生变化
                                                                          //
            downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction; //当开始下载某个文件



            //----------【开始下载】--------
            downloader.BeginDownload();
            yield return downloader;

            if (downloader.Status != EOperationStatus.Succeed)
            {
                Debug.LogError("部分资源下载失败");
                FinishUpdate(false);
                yield break;
            }

            Debug.Log("所有资源文件下载完成");





            // 6. 清理文件 


            var operationClear = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            yield return operationClear;

            operationClear.Completed += Operation_Completed;

            if (operationClear.Status == EOperationStatus.Succeed)
            {
                //清理成功
                Debug.Log("--------清理未使用的缓存文件完成---------");
            }
            else
            {
                //清理失败
                Debug.LogWarning(operationClear.Error);
            }


            //----------【更新结束】--------
            FinishUpdate(true);


            // 注意：下载完成之后再保存本地版本
            PlayerPrefs.SetString("GAME_VERSION", versionOp.PackageVersion);
             
        }
        else
        {
            // 获取上次成功记录的版本
            string version = PlayerPrefs.GetString("GAME_VERSION", string.Empty);
            if (string.IsNullOrEmpty(version))
            {
                //ShowMessageBox("没有找到本地版本记录，需要更新资源！");
                yield break;
            }

            // 加载本地缓存的资源清单文件
            var manifestOp = package.UpdatePackageManifestAsync(version);
            yield return manifestOp;
            if (manifestOp.Status != EOperationStatus.Succeed)
            {
                //ShowMessageBox("加载本地资源清单文件失败，需要更新资源！");
                yield break;
            }
             
            downloader = package.CreateResourceDownloader(downloadingMaxNum, filedTryAgain);

            Debug.Log(" 创造下载器 成功 ");
            //yield return downloader;
            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("没有需要更新的文件 ");
                FinishUpdate(true);

                UpdateDone();     // 普通 同步和异步方式         Task 资源加载也是这样的
                                  //StartCoroutine(UpdateDone());  // 协程方式 更新结束了   加载资源 

                yield break;
            }
            else
            {
                int count = downloader.TotalDownloadCount;
                long bytes = downloader.TotalDownloadBytes;
                Debug.Log($" 需要更新  {count}个文件 ，大小是  {bytes / 1024 / 1024} MB");
            } 

            //----------【绑定事件】--------
            downloader.DownloadFinishCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）
            downloader.DownloadErrorCallback = OnDownloadErrorFunction; //当下载器发生错误
            downloader.DownloadUpdateCallback = OnDownloadUpdateFunction; //当下载进度发生变化
                                                                          //
            downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction; //当开始下载某个文件
             

            //----------【开始下载】--------
            downloader.BeginDownload();
            yield return downloader;

            if (downloader.Status != EOperationStatus.Succeed)
            {
                Debug.LogError("部分资源下载失败");
                FinishUpdate(false);
                yield break;
            }

            Debug.Log("所有资源文件下载完成");

             
            // 6. 清理文件 


            var operationClear = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            yield return operationClear;

            operationClear.Completed += Operation_Completed;

            if (operationClear.Status == EOperationStatus.Succeed)
            {
                //清理成功
                Debug.Log("--------清理未使用的缓存文件完成---------");
            }
            else
            {
                //清理失败
                Debug.LogWarning(operationClear.Error);
            }


            //----------【更新结束】--------
            FinishUpdate(true);

             
        }


        //----------【更新结束】--------
        FinishUpdate(true);
        yield break;
    }



    //==========================================
    // 统一结束处理
    //==========================================
    void FinishUpdate(bool success)
    {
        isUpdating = false;

        Debug.Log(success ? "更新成功" : "更新失败");
        OnUpdateFinish?.Invoke(success);
    }


    private void OnDownloadFileBeginFunction(DownloadFileData data)
    {
    }

    private void OnDownloadUpdateFunction(DownloadUpdateData data)
    {
    }

    private void OnDownloadErrorFunction(DownloadErrorData data)
    {
    }

    private void OnDownloadFinishFunction(DownloaderFinishData data)
    {
    }

    private void ProgressCallback(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        float percent = (float)currentDownloadBytes / totalDownloadBytes * 100f;
        Debug.Log($"[下载进度] {percent:F2}%  ({currentDownloadCount}/{totalDownloadCount})");

        Debug.Log($" 需要更新 {totalDownloadCount} 个文件 ， 当前已经更新 {currentDownloadCount}个，" +
            $" 大小是 {totalDownloadBytes / 1024 / 1024} MB ，已经下载 {currentDownloadBytes / 1024 / 1024} MB ");

    }


    // Task  的方式 进行资源加载的 
    private void UpdateDone()
    {
        Debug.Log(" -- 热更新结束--- ");




        StartLoadDLL();



    }
    void StartLoadDLL()
    {

        Assembly hotUpdateAss = null;

#if UNITY_EDITOR
        //   Editor 下自动编译，不需要 dll.bytes
        hotUpdateAss = AppDomain.CurrentDomain.GetAssemblies()
            .First(a => a.GetName().Name == "HotUpdate");

#elif UNITY_ANDROID
    //  Android 平台加载 Android 的 HotUpdate.dll.bytes
    var package = YooAssets.GetPackage("DefaultPackage");
    var handle = package.LoadAssetSync<TextAsset>("Assets/Examples/AB/HotDll/Android/HotUpdate.dll.bytes");

    TextAsset text = handle.AssetObject as TextAsset;
    hotUpdateAss = Assembly.Load(text.bytes);

#elif UNITY_STANDALONE_WIN
    //  Windows 平台加载 Windows 的 HotUpdate.dll.bytes
    var package = YooAssets.GetPackage("DefaultPackage");
    var handle = package.LoadAssetSync<TextAsset>("Assets/Examples/AB/HotDll/PC/HotUpdate.dll.bytes");

    TextAsset text = handle.AssetObject as TextAsset;
    hotUpdateAss = Assembly.Load(text.bytes);

#else
    Debug.LogError("当前平台未处理 HotUpdate 加载逻辑");
#endif


        // 从热更 DLL 中获取 AOT 集
        Type cfg = hotUpdateAss.GetType("HotConfig");
        var aotList = cfg.GetMethod("GetAOTList").Invoke(null, null) as List<string>;


        foreach (string dll in aotList)
        {
            var handleAOT = package.LoadAssetSync<TextAsset>(dll);
            TextAsset aotBytes = handleAOT.AssetObject as TextAsset;
            RuntimeApi.LoadMetadataForAOTAssembly(aotBytes.bytes, HomologousImageMode.SuperSet);
        }


        // 调用入口类方法
        Type type = hotUpdateAss.GetType("Hello");
        type.GetMethod("Run").Invoke(null, null);
    }



    private void Operation_Completed(AsyncOperationBase obj)
    {
        UpdateDone();     // 普通 同步和异步方式  // Task 资源加载也是这样的 
        //StartCoroutine(UpdateDone());  // 协程方式 更新结束了   加载资源 
    }

    public void LocalUpdateDone()
    {
        StartCoroutine(StartUpdateLocal());

    }
}


public class RemoteServices : IRemoteServices
{

    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;


    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }

    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }

    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }
}