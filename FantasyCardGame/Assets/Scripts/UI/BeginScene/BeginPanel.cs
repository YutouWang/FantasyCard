using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeginPanel : BasePanel
{
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;

    public override void Init()
    {
        startButton.onClick.AddListener(() =>
        {
            Debug.Log("Start button clicked!");
            UIManager.Instance.HiddenPanel("BeginPanel");
            SceneManager.LoadScene("GameScene");
            UIManager.Instance.ShowPanel("StartStoryPanel");
        });

        settingsButton.onClick.AddListener(() =>
        {
            Debug.Log("设置按钮被点击了！");
            UIManager.Instance.ShowPanel("SettingPanel");
        });

        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
