using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public Button closeButton;

    public override void Init()
    {
        destroyOnHide = false;

        closeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.HidderPanel("SettingPanel");
        });
    }

}
