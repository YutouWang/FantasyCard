using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public Button buttonClose;
    public Toggle toggleMusic;
    public Toggle toggleSound;
    public Slider sliderMusic;
    public Slider sliderSound;
    public TMP_Dropdown dropdownLanguage;

    public override void Init()
    {
        //根据本地存储的数据  初始化面板显示内容
        SettingData settingData = GameDataManager.Instance.settingData;

        //初始化toggle
        toggleMusic.isOn = settingData.musicOpen;
        toggleSound.isOn = settingData.soundOpen;

        //初始化slider
        sliderMusic.value = settingData.musicValue;
        sliderSound.value = settingData.soundValue;

        buttonClose.onClick.AddListener(() =>
        {
            //为了节约性能只有当设置面板更改以后 面板关闭的时候才会将本次更改的数据写入硬盘
            GameDataManager.Instance.SaveSettingData();
            //隐藏自己（设置面板）
            UIManager.Instance.HiddenPanel("SettingPanel");

        });

        toggleMusic.onValueChanged.AddListener((value) =>
        {
            //更改背景音乐toggle的开关状态 同时控制背景音乐的播放状态
            BackgroundMusic.Instance.SetIsOpen(value);

            //记录本次更改 开关的状态 记录在数据结构类中的 保存到本地的话GameMgr提供了SaveSettingData方法
            GameDataManager.Instance.settingData.musicOpen = value;
        });

        toggleSound.onValueChanged.AddListener((value) =>
        {
            //记录音效数据
            GameDataManager.Instance.settingData.soundOpen = value;
        });

        sliderMusic.onValueChanged.AddListener((value) =>
        {
            //让BackgroundMusic更改背景音乐的大小
            BackgroundMusic.Instance.ChangeMusicValue(value);

            //记录本次更改
            GameDataManager.Instance.settingData.musicValue = value;

        });

        sliderSound.onValueChanged.AddListener((value) =>
        {
            //记录本次更改
            GameDataManager.Instance.settingData.soundValue = value;

        });

        
    }

}
