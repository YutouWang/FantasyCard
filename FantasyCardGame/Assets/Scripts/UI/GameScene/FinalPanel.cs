using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalPanel : BasePanel
{
    [Header("Victory Settings")]
    public Image victoryImage;         // 显示胜利 PPT
    public Sprite[] victorySlides;     // 胜利 PPT 图片数组
    public float slideInterval = 1.5f; // 每张图片显示时间
    public float fadeDuration = 0.5f;  // 每张图片淡入淡出时间

    [Header("Fail Settings")]
    public Image failImage;            // 显示失败图片
    public Sprite failSlide;
    public float failDisplayTime = 2f; // 失败图片停留时间

    private CanvasGroup victoryCanvasGroup;  // 用于胜利图片淡入淡出
    private CanvasGroup failCanvasGroup;     // 用于失败图片淡入淡出
    private bool isPlaying = false;

    public override void Init()
    {
        // 初始化 CanvasGroup
        victoryCanvasGroup = victoryImage.GetComponent<CanvasGroup>();
        if (victoryCanvasGroup == null)
            victoryCanvasGroup = victoryImage.gameObject.AddComponent<CanvasGroup>();

        failCanvasGroup = failImage.GetComponent<CanvasGroup>();
        if (failCanvasGroup == null)
            failCanvasGroup = failImage.gameObject.AddComponent<CanvasGroup>();

        // 隐藏图片
        victoryImage.gameObject.SetActive(false);
        failImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        isPlaying = false;
        victoryImage.gameObject.SetActive(false);
        failImage.gameObject.SetActive(false);
    }

    // 播放胜利 PPT
    public void PlayVictory()
    {
        if (isPlaying) return;
        isPlaying = true;

        ShowMe();
        failImage.gameObject.SetActive(false);
        victoryImage.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(PlayVictoryCoroutine());
    }

    private IEnumerator PlayVictoryCoroutine()
    {
        for (int i = 0; i < victorySlides.Length; i++)
        {
            victoryImage.sprite = victorySlides[i];

            // 图片淡入
            yield return StartCoroutine(FadeImage(victoryCanvasGroup, 0f, 1f, fadeDuration));

            // 等待显示时间
            yield return new WaitForSeconds(slideInterval);

            // 图片淡出
            yield return StartCoroutine(FadeImage(victoryCanvasGroup, 1f, 0f, fadeDuration));
        }

        EndPanel();
    }

    // 播放失败图片
    public void PlayFail()
    {
        if (isPlaying) return;
        isPlaying = true;

        ShowMe();
        victoryImage.gameObject.SetActive(false);
        failImage.gameObject.SetActive(true);
        failImage.sprite = failSlide;

        StopAllCoroutines();
        StartCoroutine(PlayFailCoroutine());
    }

    private IEnumerator PlayFailCoroutine()
    {
        // 图片淡入
        yield return StartCoroutine(FadeImage(failCanvasGroup, 0f, 1f, fadeDuration));

        // 等待显示时间
        yield return new WaitForSeconds(failDisplayTime);

        // 图片淡出
        yield return StartCoroutine(FadeImage(failCanvasGroup, 1f, 0f, fadeDuration));

        EndPanel();
    }

    // 图片淡入淡出协程
    private IEnumerator FadeImage(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    // 播放完毕后返回游戏开始界面
    private void EndPanel()
    {
        isPlaying = false;
        HideMe(() =>
        {
            // 回到游戏开始场景
            SceneManager.LoadScene("BeginScene");
        });
    }
}
