using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LanguageType
{
    Chinese,
    English
}

/// <summary>
/// 存储音乐数据、语言数据
/// </summary>
public class SettingData
{
    //设置面板的一些默认数据
    public LanguageType language = LanguageType.English;
    public bool musicOpen = true;
    public bool soundOpen = true;


    public float musicValue = 0.3f;
    public float soundValue = 0.3f;

    //标识是不是第一次加载 默认是false （意为是第一次加载）
    public bool notFirst;
}
