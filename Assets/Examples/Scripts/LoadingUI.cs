using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

public class LoadingUI : MonoBehaviour, IController
{
    public Slider ProgressBar;
    public TMP_Text MsgText;
    public TMP_Text VersionText;

    private void Start()
    {
        // 初始化 UI 状态
        ProgressBar.value = 0;
        MsgText.text = "正在检查资源更新...";

        // 1. 监听版本号更新 (显示当前准备更新或已确定的版本)
        this.RegisterEvent<AssetVersionUpdateEvent>(e =>
        {
            VersionText.text = $"当前版本: {e.Version}";
        }).UnRegisterWhenGameObjectDestroyed(gameObject);


        // 监听进度
        this.RegisterEvent<AssetDownloadUpdateEvent>(e => {
            ProgressBar.value = e.Progress;
            MsgText.text = $"资源更新中... {e.DownloadSpeed}";
        }).UnRegisterWhenGameObjectDestroyed(gameObject);

        // 监听错误
        this.RegisterEvent<AssetUpdateErrorEvent>(e => {
            MsgText.text = $"<color=red>错误: {e.Error}</color>";
        }).UnRegisterWhenGameObjectDestroyed(gameObject);

        // 监听游戏初始化完成（也就是你 Command 发出的那个）
        this.RegisterEvent<GameInitedEvent>(e => {
            MsgText.text = "初始化完成，正在进入游戏...";
            // 跳转场景或关闭面板
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    public IArchitecture GetArchitecture() => GameApp.Interface;
}
