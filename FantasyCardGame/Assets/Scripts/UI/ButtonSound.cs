using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public static ButtonSound Instance;

    public AudioSource audioSource;  // 挂在这个物体上的 AudioSource
    public AudioClip clickClip;      // 按钮点击音效

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void Play()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip);
        }
    }
}
