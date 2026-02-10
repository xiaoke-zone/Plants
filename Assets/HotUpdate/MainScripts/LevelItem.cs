using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelItem : MonoBehaviour
{
    public TMP_Text LevelNameText;
    public Button SelfButton;
    public Image LockMask; // 锁定的遮罩图层

    private void BindComponents()
    {
        LevelNameText = transform.Find("LevelText")?.GetComponent<TMP_Text>();
        SelfButton = GetComponent<Button>(); 
        LockMask = transform.GetComponent<Image>(); 
    }
     

    public void Init(int id, string name, bool isUnlocked, System.Action<int> onClick)
    {
        // 关键：在赋值前，先手动确保组件已经找过了
        BindComponents();

        // 开始赋值
        if (LevelNameText != null) 
        { 
            LevelNameText.text = name;
        } 

        if (LockMask != null)
        { 
            LockMask.gameObject.SetActive(true);
        } 
         

        if (SelfButton != null)
        {
            SelfButton.interactable = isUnlocked;
            SelfButton.onClick.RemoveAllListeners();
            if (isUnlocked)
            {
                SelfButton.onClick.AddListener(() => onClick?.Invoke(id));
            }
        } 
    }
}