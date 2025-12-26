using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理游戏全局数据类 全局可获得
/// </summary>
public class GameDataManager
{
    private static GameDataManager instance = new GameDataManager();
    public static GameDataManager Instance => instance;

    public SettingData settingData;

    private GameDataManager()
    {
        settingData = PlayerPrefsDataManager.Instance.LoadData(typeof(SettingData),"SettingData") as SettingData;
        
        //第一次加载出来的是playerprefs中的默认值  而不是我们想要的setting的默认值
        if(!settingData.notFirst)
        {
            //默认toggle打开
            settingData.musicOpen = true;
            settingData.soundOpen = true;

            //默认音量大小
            settingData.musicValue = 0.3f;
            settingData.soundValue = 0.3f;

            //默认语言
            settingData.language = LanguageType.English;

            //改完再存一次
            PlayerPrefsDataManager.Instance.SaveData(settingData, "SettingData");

        }
    }

    public void SaveSettingData()
    {
        PlayerPrefsDataManager.Instance.SaveData(settingData, "SettingData");
    }
}
