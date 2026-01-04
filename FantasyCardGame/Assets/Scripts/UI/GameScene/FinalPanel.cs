using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalPanel : BasePanel
{
    [Header("遮罩（必须是不透明全屏 Image，用来盖住 BattlePanel）")]
    public Image maskBg;

    [Header("Victory Settings")]
    public Image victoryImage;
    public Sprite[] victorySlides;
    public float slideInterval = 1.5f;
    public float fadeDuration = 0.5f;

    [Header("Fail Settings")]
    public Image failImage;
    public Sprite failSlide;
    public float failDisplayTime = 2f;

    [Header("回到开始界面时要显示的面板名（Resources/UI/xxx）")]
    public string startPanelName = "StartPanel";

    private CanvasGroup victoryCanvasGroup;
    private CanvasGroup failCanvasGroup;
    private bool isPlaying = false;
    private bool _inited = false;

    protected override void Awake()
    {
        base.Awake();
        EnsureInit();
    }

    public override void Init()
    {
        // BasePanel.Start 会调用一次，这里做成幂等
        EnsureInit();
    }

    private void EnsureInit()
    {
        if (_inited) return;
        _inited = true;

        // 确保本 Panel 自己就是完全可交互、完全不透明（不走 BasePanel 的淡入）
        var rootCg = GetComponent<CanvasGroup>();
        if (rootCg == null) rootCg = gameObject.AddComponent<CanvasGroup>();
        rootCg.alpha = 1f;
        rootCg.interactable = true;
        rootCg.blocksRaycasts = true;

        if (maskBg != null)
        {
            maskBg.gameObject.SetActive(true);
            // 关键：遮罩要盖住底下
            var c = maskBg.color;
            c.a = 1f;
            maskBg.color = c;
        }

        if (victoryImage != null)
        {
            victoryCanvasGroup = victoryImage.GetComponent<CanvasGroup>();
            if (victoryCanvasGroup == null) victoryCanvasGroup = victoryImage.gameObject.AddComponent<CanvasGroup>();
            victoryImage.gameObject.SetActive(false);
            victoryCanvasGroup.alpha = 0f;
        }

        if (failImage != null)
        {
            failCanvasGroup = failImage.GetComponent<CanvasGroup>();
            if (failCanvasGroup == null) failCanvasGroup = failImage.gameObject.AddComponent<CanvasGroup>();
            failImage.gameObject.SetActive(false);
            failCanvasGroup.alpha = 0f;
        }
    }

    // 不要走 BasePanel 的淡入
    public override void ShowMe()
    {
        EnsureInit();
        isShow = true;
        var cg = GetComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    // 不要走 BasePanel 的淡出
    public override void HideMe(UnityAction callBack)
    {
        EnsureInit();
        isShow = false;

        var cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        callBack?.Invoke();
    }

    private void OnEnable()
    {
        // 每次被 ShowPanel 重新启用时都重置状态
        StopAllCoroutines();
        isPlaying = false;

        EnsureInit();

        if (victoryImage != null) victoryImage.gameObject.SetActive(false);
        if (failImage != null) failImage.gameObject.SetActive(false);

        if (victoryCanvasGroup != null) victoryCanvasGroup.alpha = 0f;
        if (failCanvasGroup != null) failCanvasGroup.alpha = 0f;
    }

    public void PlayVictory()
    {
        EnsureInit();

        if (isPlaying) return;
        if (victorySlides == null || victorySlides.Length == 0)
        {
            Debug.LogError("FinalPanel: victorySlides 没有拖任何图片！");
            return;
        }
        if (victoryImage == null)
        {
            Debug.LogError("FinalPanel: victoryImage 没有绑定！");
            return;
        }

        isPlaying = true;
        ShowMe();

        if (failImage != null) failImage.gameObject.SetActive(false);

        victoryImage.gameObject.SetActive(true);
        victoryCanvasGroup.alpha = 0f;

        StopAllCoroutines();
        StartCoroutine(PlayVictoryCoroutine());
    }

    private IEnumerator PlayVictoryCoroutine()
    {
        for (int i = 0; i < victorySlides.Length; i++)
        {
            victoryImage.sprite = victorySlides[i];

            yield return Fade(victoryCanvasGroup, 0f, 1f, fadeDuration);
            yield return new WaitForSeconds(slideInterval);
            yield return Fade(victoryCanvasGroup, 1f, 0f, fadeDuration);
        }

        EndPanel();
    }

    public void PlayFail()
    {
        EnsureInit();

        if (isPlaying) return;
        if (failSlide == null)
        {
            Debug.LogError("FinalPanel: failSlide 没有拖图片！");
            return;
        }
        if (failImage == null)
        {
            Debug.LogError("FinalPanel: failImage 没有绑定！");
            return;
        }

        isPlaying = true;
        ShowMe();

        if (victoryImage != null) victoryImage.gameObject.SetActive(false);

        failImage.gameObject.SetActive(true);
        failImage.sprite = failSlide;
        failCanvasGroup.alpha = 0f;

        StopAllCoroutines();
        StartCoroutine(PlayFailCoroutine());
    }

    private IEnumerator PlayFailCoroutine()
    {
        yield return Fade(failCanvasGroup, 0f, 1f, fadeDuration);
        yield return new WaitForSeconds(failDisplayTime);
        yield return Fade(failCanvasGroup, 1f, 0f, fadeDuration);

        EndPanel();
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

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

    private void EndPanel()
    {
        isPlaying = false;

        // ✅ 用 UIManager.HiddenPanel 来销毁自己并从字典移除（否则下次 ShowPanel 会复用旧对象）
        UIManager.Instance.HiddenPanel("FinalPanel");

        // 切回开始场景
        SceneManager.LoadScene("BeginScene");

        // 切回去一定要把开始 UI show 出来，否则就是白屏
        //UIManager.Instance.ShowPanel("BeginPanel");
    }
}
