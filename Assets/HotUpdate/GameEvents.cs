


//  struct   改成   class

public class SunChangedEvent
{
    public int CurrentSun; // 携带当前的阳光总量
}
 
public class OnCardSelectedEvent
{
    public string PlantName; // 例如 "Peashooter"
}


public class OnShovelSelectedEvent { }

// 【热更层代码】
public class StartBattleEvent
{
    public int LevelID; // 告诉监听者玩家选了哪一关
}
