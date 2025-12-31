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

//卡牌数据是四种拍写四个CardData 然后分别挂载在四个预制体上吗
public class CardData
{
    public int cardID;

    public string cardName;

    public CardType cardType;



    //卡牌的描述（鼠标垫上去会出现功能描述）
    public string cardDescription;

    //卡牌作用的效果值
    public int value;

    //是否为奖励排
    public bool isRewardCard = false;



    //卡牌的作用效果（没有确定）

}
