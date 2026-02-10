//using UnityEngine;

//public class ZombieItem : MonoBehaviour
//{
//    public float Speed = 20f; // 僵尸移动速度
//    public int HP = 100;
//    private bool mIsDead = false;


//    public void Init()
//    {
//        HP = 100;
//        mIsDead = false;
//        gameObject.SetActive(true);
//    }

//    private void Update()
//    {
//        if (mIsDead) return;

//        // 向左移动 (-X 方向)
//        transform.Translate(Vector3.left * Speed * Time.deltaTime);

//        // 如果走出了屏幕左侧（进屋了），游戏结束逻辑
//        if (transform.localPosition.x < -1000)
//        {
//            Debug.Log("僵尸吃掉了你的脑子！");
//            // 这里可以触发游戏失败事件
//        }
//    }

//    // 接收伤害的方法
//    public void TakeDamage(int damage)
//    {
//        HP -= damage;
//        if (HP <= 0 && !mIsDead)
//        {
//            Die();
//        }
//    }

//    private void Die()
//    {
//        mIsDead = true;
//        Debug.Log("僵尸倒下了");
//        // 回收进对象池
//        PoolManager.Instance.Recycle(gameObject);
//    }

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        // 这里的逻辑已经在 PeaBullet 里写过了，但为了稳妥，也可以在这里双向判定
//        if (other.CompareTag("PeaBullet"))
//        {
//            TakeDamage(20);
//        }
//    }
//}


using UnityEngine;

public class ZombieItem : MonoBehaviour
{
    public float Speed = 20f;
    public int HP = 200;
    private bool mIsDead = false;

    private bool mIsInsideScreen = false;
    private float mRightBoundaryX; // 动态计算的右边界

    public void Init()
    {
        HP = 200;
        mIsDead = false;
        mIsInsideScreen = false;
        gameObject.SetActive(true);

        // --- 核心改进：动态计算边界 ---
        // 假设僵尸是在一个全屏适配的 Canvas 下
        // transform.root 通常是 Canvas，或者直接用 CanvasScaler 的参考分辨率
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // 获取 Canvas 的矩形宽度。如果是 ScreenSpace-Overlay，等同于 Screen.width
            // 如果有缩放，rect.width 会给出匹配后的逻辑宽度
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            mRightBoundaryX = canvasRect.rect.width * 0.5f;
        }
        else
        {
            // 保底方案：如果没有找到 Canvas，用屏幕像素的一半
            mRightBoundaryX = Screen.width * 0.5f;
        }
    }

    private void Update()
    {
        if (mIsDead) return;

        // 向左移动
        transform.Translate(Vector3.left * Speed * Time.deltaTime);

        // 动态判定进入屏幕
        if (!mIsInsideScreen && transform.localPosition.x < mRightBoundaryX)
        {
            mIsInsideScreen = true;
            Debug.Log($"僵尸进入视野！当前边界判定点: {mRightBoundaryX}");
        }

        // 进屋判定也可以动态化
        if (transform.localPosition.x < -mRightBoundaryX - 100)
        {
            Debug.Log("僵尸吃掉了你的脑子！");
            // 这里建议发送一个全局失败事件
        }
    }

    public void TakeDamage(int damage)
    {
        // 只有进场了才能受损
        if (!mIsInsideScreen) return;

        HP -= damage;
        if (HP <= 0 && !mIsDead)
        {
            Die();
        }
    }

    private void Die()
    {
        mIsDead = true;
        PoolManager.Instance.Recycle(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mIsInsideScreen && other.CompareTag("PeaBullet"))
        {
            TakeDamage(20);
        }
    }
}