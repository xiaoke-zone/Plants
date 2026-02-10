using UnityEngine;
using QFramework;

// 【热更层代码】
public abstract class UIBase : MonoBehaviour, IController
{
    // 必须要实现的接口，返回你的 AOT 层定义的单例
    public IArchitecture GetArchitecture() => GameApp.Interface;
     
    public abstract void BindComponents();
    public virtual void OnOpen(object data = null) { }

    public void SetVisible(bool visible) => gameObject.SetActive(visible);
}