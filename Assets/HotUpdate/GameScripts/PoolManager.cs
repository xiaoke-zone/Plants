using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class PoolManager : MonoSingleton<PoolManager>
{
    // 存储每种预制体对应的池子
    private Dictionary<string, Stack<GameObject>> mPoolDict = new Dictionary<string, Stack<GameObject>>();

    /// <summary>
    /// 从池中获取物体
    /// </summary>
    public GameObject Allocate(GameObject template, Transform parent)
    {
        string key = template.name;

        if (!mPoolDict.ContainsKey(key))
        {
            mPoolDict[key] = new Stack<GameObject>();
        }

        GameObject go;
        if (mPoolDict[key].Count > 0)
        {
            go = mPoolDict[key].Pop();
        }
        else
        {
            // 池子里没货了，才真的实例化
            go = Instantiate(template);
            //go.name = key; // 保持名字一致，方便回收
                           // 在 PoolManager.cs 的 Allocate 中
            go.name = template.name; // 强制保持名字一致，作为池子的 ID
        }

        go.transform.SetParent(parent);
        go.SetActive(true);
        return go;
    }

    /// <summary>
    /// 回收物体到池中
    /// </summary>
    public void Recycle(GameObject go)
    {
        string key = go.name;
        go.SetActive(false);

        if (!mPoolDict.ContainsKey(key))
        {
            mPoolDict[key] = new Stack<GameObject>();
        }

        mPoolDict[key].Push(go);
    }
}