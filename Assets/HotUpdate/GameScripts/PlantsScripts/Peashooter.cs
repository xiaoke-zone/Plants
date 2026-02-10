
using UnityEngine;
using QFramework;

public class Peashooter : PlantBase
{
    private float mTimer = 0f;
    private float mAttackInterval = 0.5f;

    private Transform mFirePoint;
    private GameObject mBulletTemplate; // 子弹模板

    public override void Init()
    {
        Debug.Log("豌豆射手已就位，准备战斗！");

        // 1. 寻找发射点
        mFirePoint = transform.Find("FirePoint");

        // 2. 寻找子弹模板（假设它叫 Bullet）
        Transform bulletTrans = transform.Find("Bullet");
        if (bulletTrans != null)
        {
            mBulletTemplate = bulletTrans.gameObject;
            mBulletTemplate.SetActive(false); // 初始一定要隐藏模板
        }
        else
        {
            Debug.LogError("在 Peashooter 下没找到名为 'Bullet' 的子物体！");
        }
    }

    private void Update()
    {
        // 只有当场上有僵尸时才射击（这里先预留逻辑，目前先一直射）
        mTimer += Time.deltaTime;
        if (mTimer >= mAttackInterval)
        {
            mTimer = 0f;
            Shoot();
        }
    }

    //private void Shoot()
    //{
    //    if (mBulletTemplate == null || mFirePoint == null) return;

    //    Debug.Log("噗！发射一颗豌豆");

    //    // 3. 克隆子弹
    //    // 我们把克隆出来的子弹放在当前植物的父物体（即地块）或者 Canvas 下，
    //    // 这样植物被铲除时，已经飞出去的子弹不会跟着消失
    //    GameObject newBullet = Instantiate(mBulletTemplate, mFirePoint.position, Quaternion.identity, transform.parent.parent);

    //    newBullet.SetActive(true); // 显示克隆出来的子弹

    //    // 4. 给子弹挂载飞行逻辑
    //    // 我们需要单独写一个 PeaBullet 脚本来控制它往前飞
    //    var  bulletScript = newBullet.AddComponent<PeaBullet>();

    //    bulletScript.Init();
    //}


    private void Shoot()
    {
        if (mBulletTemplate == null || mFirePoint == null) return;

        // 使用对象池分配子弹
        GameObject newBullet = PoolManager.Instance.Allocate(mBulletTemplate, transform.parent.parent);
        newBullet.transform.position = mFirePoint.position;

        // 如果子弹上没脚本就挂一个，有的话直接初始化
        // 重点：尝试获取脚本，如果没有才添加
        var bulletScript = newBullet.GetComponent<PeaBullet>();
        if (bulletScript == null)
        {
            bulletScript = newBullet.AddComponent<PeaBullet>();
        }

        bulletScript.Init();
    }
}