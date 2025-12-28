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
            //UIManager.Instance.ShowPanel("TalentPanel");

        });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
