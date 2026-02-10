using UnityEngine;
using QFramework;

// 植物基类
public abstract class PlantBase : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameApp.Interface;

    // 初始化方法，子类可以重写
    public virtual void Init() { }
}