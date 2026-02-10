using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

public class ShovelBtn : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameApp.Interface;

    public void Init()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            // 发送选中铲子事件
            TypeEventSystem.Global.Send(new OnShovelSelectedEvent());
        });
    }
}
