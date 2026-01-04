using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartStoryPanel : BasePanel
{
    public Button continueBtn;
    public VideoPlayer storyVideo;
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

    // 面板显示时播放
    private bool videoStarted = false;

    public override void Update()
    {
        base.Update();

        // 面板显示且视频没播放过
        if (isShow && !videoStarted)
        {
            if (storyVideo != null) storyVideo.Play();
            videoStarted = true;
        }

        // 面板隐藏时停止视频
        if (!isShow && videoStarted)
        {
            if (storyVideo != null) storyVideo.Stop();
            videoStarted = false;
        }
    }

  
}
