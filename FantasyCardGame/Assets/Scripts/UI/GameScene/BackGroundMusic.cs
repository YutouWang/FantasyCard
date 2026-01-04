using UnityEngine;



public class BackGroundMusic : MonoBehaviour
{
    [Header("挂一个 AudioSource 就够")]
    public AudioSource audioSource;

    [Header("每个关卡对应的BGM")]
    public AudioClip level1BGM;
    public AudioClip level2BGM;
    public AudioClip level3BGM;

    private LevelType currentLevel;
    private AudioClip currentClip;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("Panel3BGMController 需要一个 AudioSource 组件！");
            }
        }

        // -----------------------------
        // 安全绑定 SettingPanel 的 sliderMusic 和 toggleMusic
        // -----------------------------
        BasePanel basePanel = UIManager.Instance.GetPanel("SettingPanel");
        SettingPanel settingPanel = basePanel as SettingPanel;

        if (settingPanel != null)
        {
            // 绑定回调
            settingPanel.sliderMusic.onValueChanged.AddListener(OnMusicValueChanged);
            settingPanel.toggleMusic.onValueChanged.AddListener(OnMusicToggleChanged);

            // 初始化当前值
            OnMusicValueChanged(settingPanel.sliderMusic.value);
            OnMusicToggleChanged(settingPanel.toggleMusic.isOn);
        }
        else
        {
            Debug.LogWarning("SettingPanel 没找到或者类型不对！");
            // 默认音量和开关
            audioSource.volume = 1f;
            audioSource.mute = false;
        }
    }

    #region 音量和开关控制
    public void OnMusicValueChanged(float value)
    {
        audioSource.volume = value;
    }

    public void OnMusicToggleChanged(bool isOn)
    {
        audioSource.mute = !isOn;
    }
    #endregion

    #region 关卡音乐控制
    public void StartLevel(LevelType level)
    {
        currentLevel = level;
        PlayBGMForLevel(level);
    }

    public void WinLevel()
    {
        switch (currentLevel)
        {
            case LevelType.Level1: currentLevel = LevelType.Level2; break;
            case LevelType.Level2: currentLevel = LevelType.Level3; break;
            case LevelType.Level3:
                Debug.Log("已经是最后一关");
                break;
        }
        PlayBGMForLevel(currentLevel);
    }

    public void LoseLevel()
    {
        switch (currentLevel)
        {
            case LevelType.Level3: currentLevel = LevelType.Level2; break;
            case LevelType.Level2: currentLevel = LevelType.Level1; break;
            case LevelType.Level1:
                Debug.Log("已经是第一关，无法回退");
                break;
        }
        PlayBGMForLevel(currentLevel);
    }

    private void PlayBGMForLevel(LevelType level)
    {
        AudioClip targetClip = GetClipForLevel(level);
        if (targetClip == null) return;
        if (currentClip == targetClip) return; // 音乐没变就不重复播放

        currentClip = targetClip;
        audioSource.clip = currentClip;
        audioSource.loop = true; // 战斗音乐循环
        audioSource.Play();
    }

    private AudioClip GetClipForLevel(LevelType level)
    {
        switch (level)
        {
            case LevelType.Level1: return level1BGM;
            case LevelType.Level2: return level2BGM;
            case LevelType.Level3: return level3BGM;
            default: return null;
        }
    }
    #endregion
}
