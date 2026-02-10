// 【热更层代码】
public interface ISceneLoader
{
    // 场景名称，对应 Unity 场景文件的名字
    string SceneName { get; }
    // 加载逻辑
    void OnLoad(ISceneSystem sceneSystem);
}
