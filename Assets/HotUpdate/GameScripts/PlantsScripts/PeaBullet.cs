//using UnityEngine;

//public class PeaBullet : MonoBehaviour
//{
//    private float mSpeed = 500f; // UI 坐标系下的速度
//    private float mLifeTime = 5f; // 5秒后自动销毁，防止内存泄漏

//    public void Init()
//    {
//        Destroy(gameObject, mLifeTime);
//    }

//    private void Update()
//    {
//        // 每一帧往右移动
//        transform.Translate(Vector3.right * mSpeed * Time.deltaTime);

//        // 如果飞出屏幕右侧也销毁
//        if (transform.position.x > Screen.width + 100)
//        {
//            Destroy(gameObject);
//        }
//    }

//    // 当子弹碰到僵尸（后续需要给僵尸加 Collider）
//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Zombie"))
//        {
//            // 给僵尸扣血逻辑...
//            Debug.Log("击中僵尸！");
//            Destroy(gameObject); // 命中后销毁子弹
//        }
//    }
//}

using UnityEngine;

public class PeaBullet : MonoBehaviour
{
    private float mSpeed = 500f;
    private float mTimer = 0f;
    private float mMaxLifeTime = 5f; // 代替 Destroy(go, time)

    public void Init()
    {
        mTimer = 0f; // 重置计时器
    }

    private void Update()
    {
        transform.Translate(Vector3.right * mSpeed * Time.deltaTime);

        // 计时回收
        mTimer += Time.deltaTime;
        if (mTimer >= mMaxLifeTime || transform.position.x > Screen.width + 100)
        {
            PoolManager.Instance.Recycle(gameObject);
        }
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Zombie"))
    //    {
    //        Debug.Log("击中僵尸！");
    //        // 命中即回收
    //        PoolManager.Instance.Recycle(gameObject);
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只检查僵尸
        if (other.CompareTag("Zombie"))
        {
            // 尝试获取僵尸脚本并造成伤害
            var zombie = other.GetComponent<ZombieItem>();
            if (zombie != null)
            {
                zombie.TakeDamage(20);
                Debug.Log("击中僵尸，造成20点伤害");
            }

            // 子弹完成使命，回收
            PoolManager.Instance.Recycle(gameObject);
        }
    }
}