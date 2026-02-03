using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using QFramework;

public class GameApp : Architecture<GameApp>
{
    protected override void Init()
    {  
        // 必须注册，Command 才能通过 GetSystem 获取到
        this.RegisterSystem<IAssetSystem>(new YooAssetSystem());
        this.RegisterSystem<IHotUpdateSystem>(new HotUpdateSystem());

    }
}


