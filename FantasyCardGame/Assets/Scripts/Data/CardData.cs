using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌类型枚举
/// </summary>
public enum CardType
{
    /// <summary> 攻击（红色牌）</summary>
    Attack,

    /// <summary> 防御（紫色牌）</summary>
    Defense,

    /// <summary> 回收（黄色牌）</summary>
    Recall,

    /// <summary> 治疗（绿色牌）</summary>
    Recovery
}
/// <summary>
/// 恢复牌专用 用来选择到底是加san值还是加temperature
/// </summary>
public struct ChoiceEffect
{
    /// <summary> 作用效果的名称 可填“体温”“san值”</summary>
    public string effectName;

    /// <summary> 对应效果的作用数值 </summary>
    public int effectValue;

}

//卡牌数据结构 后续在CardDataBase 中初始化 申明4个CardData类型的变量 4中不同的初始化
public class CardData
{
    public int cardID;

    public string cardName;

    public CardType cardType;



    //卡牌的描述（鼠标点上去会出现功能描述）
    public string cardDescription;

    //其他三种卡牌作用的效果值
    public int value;

    public ChoiceEffect[] choiceEffects;

    //是否为奖励排
    public bool isRewardCard = false;

    public Sprite cardSprite;


}

//牌库枚举类型
public enum DeckType
{
    BaseDeck,
    RewardDeck
}

//封装carddata 作为场景中实例化出来的具体卡牌实例
//可以在牌库（看不见的地方）中管理 也可以在UI面板（看得见的地方）上管理

public class CardInstance
{
    public CardData cardTemplate;

    public DeckType deckType;

    //有参构造 
    public CardInstance(CardData template,DeckType type)
    {
        cardTemplate = template ;
        deckType = type ;
    }
}
