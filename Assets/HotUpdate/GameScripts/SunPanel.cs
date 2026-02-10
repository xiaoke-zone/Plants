using UnityEngine;
using QFramework;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// 继承 UIPanel，并显式实现 IController 接口
public class SunPanel : UIBase
{
    public TMP_Text sunNumText;
    public Button BtnShovel;
    public Button BtnBack;
      
    private Transform mSunPoint;
    public GameObject mSunTemplate;     // 太阳模板
    private bool mHasTemplate = false;

    private float mSunTimer = 0f;
    private const float SUN_DROP_INTERVAL = 5f; // 5秒间隔
    private const float MARGIN = 100f; // 边缘距离


    public override void BindComponents()
    {
        BtnShovel = transform.Find("BtnShovel")?.GetComponent<Button>();
        BtnBack = transform.Find("BtnBack")?.GetComponent<Button>();
        sunNumText = transform.Find("SunText")?.GetComponent<TMP_Text>();
         
        // 绑定返回按钮事件
        BtnBack?.onClick.AddListener(() =>
        {
            // 切换场景（SceneDirector 会自动处理 UI 的销毁和重新生成） 

            var sceneSystem = GameApp.Interface.GetSystem<ISceneSystem>(); 
            sceneSystem.LoadScene("MainScene");

        });

        // --- 在这里直接绑定铲子逻辑，省掉 ShovelBtn.cs ---
        BtnShovel?.onClick.AddListener(() =>
        {
            Debug.Log("点击了铲子，发送选中事件");
            // 发送给 BattlePanel 监听
            TypeEventSystem.Global.Send(new OnShovelSelectedEvent());
        });


        // 1. 寻找发射点
        mSunPoint = transform.Find("FirePoint");

        //// 2. 寻找子弹模板（假设它叫 SunPrefab）
        //Transform sunTrans = transform.Find("SunPrefab");
        //if (sunTrans != null)
        //{
        //    mSunTemplate = sunTrans.gameObject;
        //    mSunTemplate.SetActive(false); // 初始一定要隐藏模板
        //}
        //else
        //{
        //    Debug.LogError("在 SunPanel 下没找到名为 'SunPrefab' 的子物体！");
        //}

        Transform sunTrans = transform.Find("SunPrefab");
        if (sunTrans != null)
        {
            mSunTemplate = sunTrans.gameObject;
            mSunTemplate.SetActive(false);
            mHasTemplate = true; // 确认找到了
        }
        else
        {
            // 如果这里报错，请检查 Hierarchy 结构！
            Debug.LogError("致命错误：在 SunPanel 下找不到 SunPrefab，请确认物体名字准确且在 SunPanel 节点下！");
            mHasTemplate = false;
        }

    }




    public override void OnOpen(object data = null)
    {
        RefreshSunUI();
        // 在 SunPanel.cs 的 OnOpen 里
        TypeEventSystem.Global.Register<SunChangedEvent>(e =>
        {
            sunNumText.text = e.CurrentSun.ToString();
        }).UnRegisterWhenGameObjectDestroyed(gameObject);

        // 开启生成阳光的逻辑
        mSunTimer = 0f;

    }

    private void Update()
    { 
        // 如果没有模板，根本不要跑计时器
        if (!mHasTemplate) return;

        // 阳光掉落逻辑
        mSunTimer += Time.deltaTime;
        if (mSunTimer >= SUN_DROP_INTERVAL)
        {
            mSunTimer = 0f;
            SpawnSun();
        }
    } 

    private void SpawnSun()
    {  
        // 1. 增加 PoolManager 判空
        if (PoolManager.Instance == null)
        {
            Debug.LogError("PoolManager 实例未找到！");
            return;
        }

        // 2. 【最关键检查】检查模板是否丢失
        if (mSunTemplate == null)
        {
            // 尝试重新寻找一次（万一初始化时没找到）
            Transform sunTrans = transform.Find("SunPrefab");
            if (sunTrans != null)
            {
                mSunTemplate = sunTrans.gameObject;
                mSunTemplate.SetActive(false);
            }
            else
            {
                // 如果还是找不到，说明你没在 Hierarchy 里的 SunPanel 下放 SunPrefab
                Debug.LogError("SpawnSun 失败：未能在 SunPanel 节点下找到名为 'SunPrefab' 的子物体。请检查层级结构！");
                return;
            }
        }



        float randomX = Random.Range(MARGIN, Screen.width - MARGIN);
        float randomY = Random.Range(MARGIN, Screen.height - MARGIN);
        Vector3 spawnPos = new Vector3(randomX, randomY, 0);



        // 这样就不会传 null 进对象池了
        GameObject sunGo = PoolManager.Instance.Allocate(mSunTemplate, transform.parent.parent);

        if (sunGo != null)
        {
            sunGo.transform.position = spawnPos;
            sunGo.transform.localScale = Vector3.one;
            var sunScript = sunGo.GetComponent<SunItem>() ?? sunGo.AddComponent<SunItem>();
            sunScript.Init(25);
        }



        //GameObject sunGo = PoolManager.Instance.Allocate(mSunTemplate, transform.parent.parent);

        //// 【关键修改】检查对象池是否真的分出了物体
        //if (sunGo == null) return;

        //sunGo.transform.position = spawnPos;
        //sunGo.transform.localScale = Vector3.one;

        //var sunScript = sunGo.GetComponent<SunItem>() ?? sunGo.AddComponent<SunItem>();
        //sunScript.Init(25);
    }



    /// <summary>
    /// 刷新 UI 文字的方法
    /// </summary>
    public void RefreshSunUI()
    {
         
        if (sunNumText != null)
        {
            sunNumText.text = GlobalData.CurrentSun.ToString();
        }
        else
        {
            // 如果打印了这个，说明你的 BindComponents 寻找路径 SunText 不对
            Debug.LogError("SunPanel 找不到 sunNumText 引用！");
        }
    }
}