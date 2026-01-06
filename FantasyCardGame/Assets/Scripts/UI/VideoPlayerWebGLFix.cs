using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerWebGLFix : MonoBehaviour
{
    public VideoPlayer vp;

    // 桌面端用 VideoClip（可选）
    public VideoClip editorOrStandaloneClip;

    // WebGL 用 StreamingAssets 里的文件名
    public string webglFileName = "StartStory.mp4";

    // 建议：由按钮点击触发播放，别 PlayOnAwake
    public bool playOnStart = false;

    void Awake()
    {
        if (!vp) vp = GetComponent<VideoPlayer>();

#if UNITY_WEBGL && !UNITY_EDITOR
    vp.source = VideoSource.Url;
    vp.url = Application.streamingAssetsPath + "/" + webglFileName;

    vp.audioOutputMode = VideoAudioOutputMode.Direct; // 开声音
#else
        if (editorOrStandaloneClip != null)
        {
            vp.source = VideoSource.VideoClip;
            vp.clip = editorOrStandaloneClip;
        }
#endif

    }

    void Start()
    {
        if (playOnStart)
            PlayFromUserGestureSafe();
    }

    // 建议把这个绑到按钮 OnClick（最稳）
    public void PlayFromUserGestureSafe()
    {
        vp.Prepare();
        vp.prepareCompleted += OnPrepared;
    }

    void OnPrepared(VideoPlayer player)
    {
        player.prepareCompleted -= OnPrepared;

#if UNITY_WEBGL && !UNITY_EDITOR
    // 这里再启用音轨/音量，最稳
    player.EnableAudioTrack(0, true);
    player.SetDirectAudioMute(0, false);
    player.SetDirectAudioVolume(0, 1f);
#endif

        player.Play();
    }
}
