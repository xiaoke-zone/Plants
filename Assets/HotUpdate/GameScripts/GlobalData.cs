using System.Collections.Generic;

using QFramework;
using UnityEngine;

public static class GlobalData
{
    public static int SelectedLevelID = 1;
    public static int CurrentSun = 200;

    // --- 配置中心 ---

    /// <summary>
    /// 获取植物消耗阳光的配置
    /// </summary>
    public static int GetPlantCost(string plantName)
    {
        return plantName switch
        {
            "Peashooter" => 100,
            "SunFlower" => 50,
            "WallNut" => 50,
            "PotatoMine" => 25,
            "SnowPea" => 175,
            _ => 100
        };
    }

    /// <summary>
    /// 获取植物冷却时间的配置
    /// </summary>
    public static float GetPlantCooldown(string plantName)
    {
        return plantName switch
        {
            "WallNut" => 20f,
            "PotatoMine" => 15f,
            _ => 7.5f // 大部分植物默认 7.5 秒
        };
    }

    /// <summary>
    /// 统一修改阳光的方法（增加或减少）
    /// </summary>
    /// <param name="amount">正数为增加，负数为扣除</param>
    public static void ChangeSun(int amount)
    {
        int oldSun = CurrentSun;
        CurrentSun += amount;
        if (CurrentSun < 0) CurrentSun = 0;

        Debug.Log($"[GlobalData] 阳光变更: {oldSun} -> {CurrentSun}");

        try
        {
            TypeEventSystem.Global.Send(new SunChangedEvent { CurrentSun = CurrentSun });
        }
        catch (System.Exception e)
        {
            // 如果这里报错，说明监听 SunChangedEvent 的地方逻辑写崩了
            Debug.LogError($"[GlobalData] 发送事件崩溃: {e.Message}");
        }
    }
}

 