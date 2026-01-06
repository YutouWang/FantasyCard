using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TalentPanel : BasePanel
{
    public Button scarfBtn;
    public Button medBtn;
    public Button bearBtn;

    [Header("Tooltip")]
    public GameObject tooltipRoot;   // 一个小面板，默认隐藏
    public TMP_Text tooltipText;     // 面板里的文字

    public override void Init()
    {
        // 默认隐藏提示
        if (tooltipRoot != null) tooltipRoot.SetActive(false);

        // ====== 点击选择 ======
        scarfBtn.onClick.RemoveAllListeners();
        scarfBtn.onClick.AddListener(() =>
        {
            ProgressManager.Instance.Data.selectedTalent = TalentType.Scarf;
            UIManager.Instance.ShowPanel("BattlePanel");
            UIManager.Instance.HiddenPanel("TalentPanel");
        });

        medBtn.onClick.RemoveAllListeners();
        medBtn.onClick.AddListener(() =>
        {
            ProgressManager.Instance.Data.selectedTalent = TalentType.Med;
            UIManager.Instance.ShowPanel("BattlePanel");
            UIManager.Instance.HiddenPanel("TalentPanel");
        });

        bearBtn.onClick.RemoveAllListeners();
        bearBtn.onClick.AddListener(() =>
        {
            ProgressManager.Instance.Data.selectedTalent = TalentType.Bear;
            UIManager.Instance.ShowPanel("BattlePanel");
            UIManager.Instance.HiddenPanel("TalentPanel");
        });

        // ====== 悬停提示（写死三条文案）======
        AddHoverTooltip(scarfBtn.gameObject,
            "妈妈织的围巾\n每回合体温流失降低至 5 点。");

        AddHoverTooltip(medBtn.gameObject,
            "氟西汀\n每次【深潜】成功生效，回复 SAN 5 点。");

        AddHoverTooltip(bearBtn.gameObject,
            "被缝补过的小熊\n玩家回合行动前，多抽 1 张牌。");
    }

    // 给某个按钮添加 hover 事件：进入显示，离开隐藏
    private void AddHoverTooltip(GameObject target, string content)
    {
        if (target == null) return;

        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();
        trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();

        // PointerEnter
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((data) =>
        {
            ShowTooltip(content);
        });

        // PointerExit
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((data) =>
        {
            HideTooltip();
        });

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    private void ShowTooltip(string content)
    {
        if (tooltipText != null) tooltipText.text = content;
        if (tooltipRoot != null) tooltipRoot.SetActive(true);
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }
}
