//using System.Collections.Generic;

//public static class HotConfig
//{
//    private const string AOT_DIR = "Assets/Examples/AB/HotDll/Android/";



//    private static readonly string[] AOT_NAMES =
//    {
//        "mscorlib.dll.bytes",
//        "MainAOT.dll.bytes",
//        "Newtonsoft.Json.dll.bytes",
//        "System.dll.bytes",
//        "System.Core.dll.bytes",
//        "UnityEngine.CoreModule.dll.bytes",
//        "YooAsset.dll.bytes",
//    };

//    public static List<string> GetAOTList()
//    {
//        var paths = new List<string>();
//        foreach (var name in AOT_NAMES)
//        {
//            paths.Add(AOT_DIR + name);
//        }
//        return paths;
//    }
//}


using System.Collections.Generic;

public static class HotConfig
{
    private static readonly string[] AOT_NAMES =
    {
        "mscorlib.dll",
        "MainAOT.dll",
        "Newtonsoft.Json.dll",
        "System.dll",
        "System.Core.dll",
        "UnityEngine.CoreModule.dll",
        "YooAsset.dll",
    };

    public static List<string> GetAOTList()
    {
        // 直接返回文件名列表，不要拼接任何 AOT_DIR！
        return new List<string>(AOT_NAMES);
    }
}