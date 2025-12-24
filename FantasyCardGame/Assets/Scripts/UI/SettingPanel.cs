using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public Button closeButton;

    public override void Init()
    {
        
        closeButton.onClick.AddListener(() => {
            UIManager.Instance.HiddenPanel("SettingPanel");
        });
    }
}
