using System.Collections.Generic;
using UnityEngine;

public class SimpleUIManager
{
    public static readonly SimpleUIManager Instance = new SimpleUIManager();
    private Dictionary<string, UIBase> mPanels = new Dictionary<string, UIBase>();
    private UIBase mCurrentPanel;

    public void Register(string name, UIBase panel)
    {
        mPanels[name] = panel;
        // 在注册时强制执行一次组件绑定，确保引用就绪
        panel.BindComponents();
        panel.SetVisible(false);
    }

    //public void OpenPanel(string name, object data = null)
    //{
    //    if (mCurrentPanel != null) mCurrentPanel.SetVisible(false);
    //    if (mPanels.TryGetValue(name, out var panel))
    //    {
    //        mCurrentPanel = panel;
    //        mCurrentPanel.SetVisible(true);
    //        mCurrentPanel.OnOpen(data);
    //    }
    //}


    public void OpenPanel(string name, object data = null)
    {
        // 1. 如果要打开的就是当前面板，直接跳过，防止重复调用 OnOpen
        if (mCurrentPanel != null && mPanels.ContainsKey(name) && mCurrentPanel == mPanels[name])
            return;

        // 2. 隐藏之前的面板
        if (mCurrentPanel != null)
        {
            mCurrentPanel.SetVisible(false);
        }

        // 3. 显示新的面板
        if (mPanels.TryGetValue(name, out var panel))
        {
            mCurrentPanel = panel;
            mCurrentPanel.SetVisible(true);
            mCurrentPanel.OnOpen(data);
            Debug.Log($"成功打开面板: {name}");
        }
        else
        {
            Debug.LogError($"面板 {name} 未注册！");
        }
    }


    // --- 新增内容 ---

    /// <summary>
    /// 场景切换时调用，彻底清空引用
    /// </summary>
    public void ClearAll()
    {
        mPanels.Clear();
        mCurrentPanel = null;
        Debug.Log("<color=cyan>UI 管理器已清空</color>");
    }

    /// <summary>
    /// 叠加显示方法：直接打开面板，不影响其他面板
    /// </summary>
    public void ShowPanel(string name, object data = null)
    {
        if (mPanels.TryGetValue(name, out var panel))
        {
            panel.SetVisible(true);
            panel.OnOpen(data);
            Debug.Log($"<color=orange>叠加显示面板: {name}</color>");
        }
    }

    /// <summary>
    /// 隐藏指定面板
    /// </summary>
    public void HidePanel(string name)
    {
        if (mPanels.TryGetValue(name, out var panel))
        {
            panel.SetVisible(false);
        }
    }


    /// <summary>
    /// 手动关闭当前开启的面板
    /// </summary>
    public void CloseCurrent()
    {
        if (mCurrentPanel != null)
        {
            mCurrentPanel.SetVisible(false);
            mCurrentPanel = null;
        }
    }
}