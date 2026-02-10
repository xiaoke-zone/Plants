using UnityEngine;
using UnityEngine.UI;
using QFramework;

public class MainUIPanel : UIBase
{
    public Button BtnStart;

    public override void BindComponents()
    {
        // 使用你的路径查找
        BtnStart = transform.Find("BtnStart")?.GetComponent<Button>();

        BtnStart?.onClick.RemoveAllListeners();
        BtnStart?.onClick.AddListener(() =>
        {  
            // 切换面板逻辑
            SimpleUIManager.Instance.OpenPanel("LevelSelectPanel");
        });

    }

    private void Start()
    {
        gameObject.SetActive(true);
    }


     
}