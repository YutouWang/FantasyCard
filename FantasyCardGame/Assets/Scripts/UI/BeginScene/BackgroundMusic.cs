using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    public static BackgroundMusic Instance => instance;

    private AudioSource bkSource;

    //带有mono 的脚本的单例模式 初始化实例在其awake中 只要挂载了对象 就会产生对象实例
    private void Awake()
    {
        instance = this;
        bkSource = this.GetComponent<AudioSource>();

        //通过默认数据来设置音乐的大小 开关状态
        SettingData data = GameDataManager.Instance.settingData;
        SetIsOpen(data.musicOpen);
        ChangeMusicValue(data.musicValue);
    }

    //提供给外部 开关背景音乐的方法（toggle）
    public void SetIsOpen(bool isOpen)
    {
        //mute是静音开关 true 是静音  false 是开启
        bkSource.mute = !isOpen;
    }

    //提供给外部 调整背景音乐大小的方法（slider）
    public void ChangeMusicValue(float value)
    {
        bkSource.volume = value;
    }
}
