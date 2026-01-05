using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BossBGM : MonoBehaviour
{
    [Header("Boss背景音乐")]
    public AudioSource audioSource;

    [Header("当前Boss关卡音乐")]
    public AudioClip bgmClip;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.clip = bgmClip;
        audioSource.loop = true;

        // 绑定全局 Setting 面板音量和开关
        BasePanel basePanel = UIManager.Instance.GetPanel("SettingPanel");
        SettingPanel settingPanel = basePanel as SettingPanel;

        if (settingPanel != null)
        {
            settingPanel.sliderMusic.onValueChanged.AddListener(OnMusicValueChanged);
            settingPanel.toggleMusic.onValueChanged.AddListener(OnMusicToggleChanged);

            // 初始化当前音量和开关状态
            OnMusicValueChanged(settingPanel.sliderMusic.value);
            OnMusicToggleChanged(settingPanel.toggleMusic.isOn);
        }
        else
        {
            // 如果找不到 SettingPanel，使用默认音量
            audioSource.volume = 1f;
            audioSource.mute = false;
        }
    }

    private void OnEnable()
    {
        // Boss激活时播放音乐
        if (audioSource.clip != null)
            audioSource.Play();
    }

    private void OnDisable()
    {
        // Boss失活时停止音乐
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    #region 音量与开关回调
    private void OnMusicValueChanged(float value)
    {
        if (audioSource != null)
            audioSource.volume = value;
    }

    private void OnMusicToggleChanged(bool isOn)
    {
        if (audioSource != null)
            audioSource.mute = !isOn;
    }
    #endregion
}
