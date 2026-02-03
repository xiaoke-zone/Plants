using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 定义游戏初始化完成的事件
public struct GameInitedEvent { }

public struct AssetDownloadUpdateEvent
{
    public float Progress; // 0-1
    public string DownloadSpeed;
}

public struct AssetUpdateErrorEvent
{
    public string Error;
}