using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalentPanel : BasePanel
{
    public Button scarfBtn;
    public Button medBtn;
    public Button bearBtn;

    public override void Init()
    {
        //防止重复绑定
        scarfBtn.onClick.RemoveAllListeners();
        medBtn.onClick.RemoveAllListeners();
        bearBtn.onClick.RemoveAllListeners();

        //按下围巾按钮 
        scarfBtn.onClick.AddListener(() =>
        {
            print("每回合体温值流失降低至5点");
            ProgressManager.Instance.Data.selectedTalent = TalentType.Scarf;
            UIManager.Instance.ShowPanel("BattlePanel");
            UIManager.Instance.HiddenPanel("TalentPanel");
        });

        //按下药按钮
        medBtn.onClick.AddListener(() =>
        {
            ProgressManager.Instance.Data.selectedTalent = TalentType.Med;
            print("每局成功使用深潜回5点san值");
            UIManager.Instance.ShowPanel("BattlePanel");
            UIManager.Instance.HiddenPanel("TalentPanel");
        });

        //按下小熊键
        bearBtn.onClick.AddListener(() =>
        {
            ProgressManager.Instance.Data.selectedTalent = TalentType.Bear;
            print("每回合行动前多抽取一张卡牌");
            UIManager.Instance.ShowPanel("BattlePanel");
            UIManager.Instance.HiddenPanel("TalentPanel");
        });
    }



    // Update is called once per frame
    public override void Update()
    {
        base.Update();

    }
}
