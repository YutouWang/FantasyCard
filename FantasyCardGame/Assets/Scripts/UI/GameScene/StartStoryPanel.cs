using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartStoryPanel : BasePanel
{
    public Button continueBtn;
    public override void Init()
    {
        //按下继续按钮 会进入选天赋面板 
        continueBtn.onClick.AddListener(() =>
        {
            //先隐藏故事面板 不然影响天赋面板的显示
            UIManager.Instance.HiddenPanel("StartStoryPanel");
            print("进入选天赋面板");
            UIManager.Instance.ShowPanel("TalentPanel");

        });
    }


    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        print("开场分镜动画的逻辑");
        //此处应该写三个分镜一次按时间显示出来的逻辑
        
    }
}
