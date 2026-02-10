using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using QFramework;

public class PlantsCard : MonoBehaviour, IController
{
    // UI 引用
    public Image PlantIcon;       // 正常颜色图片
    public Button CardButton;
    public TMP_Text CostText;
    public Image CooldownMask;    // 冷却遮罩

    // 数据
    private string mPlantName;
    private int mCost;
    private float mCooldownTime;

    private bool mIsCooldown = false;
    private bool mIsSunEnough = false;

    // 缓存 Sprite，避免重复加载
    private Sprite mNormalSprite;
    private Sprite mGraySprite;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    public void BindComponents()
    {
        // 如果已经绑定过就不再重复获取
        if (PlantIcon != null) return;

        PlantIcon = transform.Find("PlantIcon").GetComponent<Image>();
        CardButton = GetComponent<Button>();
        CostText = transform.Find("CostText").GetComponent<TMP_Text>();
        CooldownMask = transform.Find("CooldownMask")?.GetComponent<Image>();


    }

    public void Init(string plantName, int cost, float cooldown)
    {
        BindComponents();
        mPlantName = plantName;
        mCost = cost;
        mCooldownTime = cooldown;

        // 强制初始冷却进度为 0
        if (CooldownMask != null) CooldownMask.fillAmount = 0;

        CostText.text = mCost.ToString();

        // 1. 预加载两种状态的图片
        PreloadSprites();

        // 2. 初始化阳光状态（假设初始阳光是 GlobalData 里的值）
        OnSunChanged(GlobalData.CurrentSun);

        // 3. 绑定点击
        CardButton.onClick.RemoveAllListeners();
        CardButton.onClick.AddListener(OnCardClick);
    }

    private void PreloadSprites()
    {
        var package = YooAsset.YooAssets.GetPackage("DefaultPackage");

        // 加载正常图片
        var handleNormal = package.LoadAssetAsync<Sprite>(mPlantName);
        handleNormal.Completed += (op) => {
            if (op.Status == YooAsset.EOperationStatus.Succeed)
            {
                mNormalSprite = op.AssetObject as Sprite;
                RefreshVisual(); // 加载完刷新一次
            }
        };

        // 加载灰色图片 (带 G)
        var handleGray = package.LoadAssetAsync<Sprite>(mPlantName + "G");
        handleGray.Completed += (op) => {
            if (op.Status == YooAsset.EOperationStatus.Succeed)
            {
                mGraySprite = op.AssetObject as Sprite;
                RefreshVisual(); // 加载完刷新一次
            }
        };
    }

    public void OnSunChanged(int currentSun)
    {
        mIsSunEnough = currentSun >= mCost;
        RefreshVisual();
    }

    //private void RefreshVisual()
    //{
    //    // 判断条件：阳光充足 且 不在冷却中 才是“可互动状态”
    //    bool canInteract = mIsSunEnough && !mIsCooldown;
    //    CardButton.interactable = canInteract;

    //    // 根据状态切换图片
    //    if (canInteract)
    //    {
    //        if (mNormalSprite != null) PlantIcon.sprite = mNormalSprite;
    //        //PlantIcon.enabled = true;           // 显示正常图标
    //        //CooldownMask.enabled = false;       // 彻底关掉灰色/冷却层
    //    }
    //    else
    //    {
    //        if (mGraySprite != null) PlantIcon.sprite = mGraySprite;
    //        //PlantIcon.enabled = false;          // 关掉正常图标
    //        //CooldownMask.enabled = true;        // 开启灰色/冷却层

    //        //// 注意：如果是因为阳光不足而变灰，填充量应该是 0（全显灰色）
    //        //// 如果是因为正在冷却，FillAmount 会由协程控制
    //        //if (!mIsCooldown)
    //        //{
    //        //    CooldownMask.fillAmount = 1; // 阳光不足时，显示完整的灰色图
    //        //}
    //    }

    //    // 如果阳光不足，通常 UI 上也会把文字变红或者加个半透明效果
    //    CostText.color = mIsSunEnough ? Color.green : Color.red;
    //}


    private void RefreshVisual()
    {
        bool canInteract = mIsSunEnough && !mIsCooldown;
        CardButton.interactable = canInteract;

        // 1. 处理图标亮暗 (阳光逻辑)
        if (mIsSunEnough)
        {
            if (mNormalSprite != null) PlantIcon.sprite = mNormalSprite;
        }
        else
        {
            // 阳光不够，显示灰色图
            if (mGraySprite != null) PlantIcon.sprite = mGraySprite;
        }

        // 2. 冷却遮罩逻辑 (独立逻辑)
        // 只有在冷却中，且遮罩没有被销毁时，才确保它能被看到
        if (CooldownMask != null)
        {
            // 如果不在冷却中，确保遮罩是空的
            if (!mIsCooldown)
            {
                CooldownMask.fillAmount = 0;
            }
        }

        CostText.color = mIsSunEnough ? Color.green : Color.red;
    }

    private void OnCardClick()
    {
        // 双重校验
        if (mIsCooldown || !mIsSunEnough) return;

        // 【TODO: 种植逻辑点这里】
        // 这里应该先让鼠标拿上这个植物
        Debug.Log($"准备种植: {mPlantName}");

        // 发送事件：通知 BattlePanel 准备种植
        TypeEventSystem.Global.Send(new OnCardSelectedEvent { PlantName = mPlantName });

        // 进入冷却
        StartCoroutine(StartCooldown());
    }

   

    private IEnumerator StartCooldown()
    {
        mIsCooldown = true;
        RefreshVisual(); // 切换到灰色图片

        float timer = 0;
        if (CooldownMask != null) CooldownMask.fillAmount = 1;

        while (timer < mCooldownTime)
        {
            timer += Time.deltaTime;
            if (CooldownMask != null)
            {
                // 冷却遮罩：从 1 慢慢消失到 0
                CooldownMask.fillAmount = 1 - (timer / mCooldownTime);
            }
            yield return null;
        }

        if (CooldownMask != null) CooldownMask.fillAmount = 0;
        mIsCooldown = false;

        // 冷却结束，刷新视觉状态（如果此时阳光够，会变回彩色）
        RefreshVisual();
    }
}