

using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;

public class SunItem : MonoBehaviour, IPointerClickHandler
{
    private int mValue = 25;
    private bool mIsCollected = false;

    private float mLifeTimer = 0f;
    private const float MAX_LIFE_TIME = 10f; // 10秒不点自动回收

    public void Init(int value)
    {
        mValue = value;
        mIsCollected = false; // 必须重置，否则池子里取出来的永远是已采集状态
        mLifeTimer = 0f;      // 重置寿命计时器
        gameObject.SetActive(true);
    }

    private void Update()
    {
        // 手动处理生命周期计时
        mLifeTimer += Time.deltaTime;
        if (mLifeTimer >= MAX_LIFE_TIME)
        {
            RecycleSelf();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (mIsCollected) return;
        mIsCollected = true;


        Debug.Log($"[SunItem] 点击阳光，增加数值: {mValue}");

       


        GlobalData.ChangeSun(mValue);

        // 播放个音效或简单的飞向UI动画（可选）

        RecycleSelf();
    }

    private void RecycleSelf()
    {
        // 调用我们定义的 PoolManager 回收
        PoolManager.Instance.Recycle(this.gameObject);
    }
}