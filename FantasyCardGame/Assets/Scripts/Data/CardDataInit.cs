using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// CardData 是定义卡牌数据结构的框架（有哪些属性）
/// 而 CardDataInit 是初始化各个类型的卡牌数据的
/// 继承 mono 挂载空 gameobject做成预制体 后续动态加载
/// </summary>
public class CardDataInit : MonoBehaviour
{
    //外部拖入对应的卡牌牌面 初始化牌面sprite数据
    public Sprite attackSprite;
    public Sprite defenseSprite;
    public Sprite recallSprite;
    public Sprite recoverySprite;


    //四种牌类型 需要四个CardData
    public CardData attackCard;
    public CardData defenseCard;
    public CardData recallCard;
    public CardData recoveryCard;

    //继承mono 所以在awake中初始化
    private void Awake()
    {
        attackCard = new CardData
        {
            cardID = 1,
            cardName = "锋芒",// ????动态生成名字吗 那这里好像应该填英文
            cardType = CardType.Attack,

            cardDescription = "我是描述锋芒牌的作用的",
            value = 20,
            //默认初始卡牌都不是奖励牌 如果需要奖励牌自行更改
            isRewardCard = false,
            cardSprite = attackSprite,
        };

        defenseCard = new CardData
        {
            cardID = 2,
            cardName = "深潜",
            cardType = CardType.Defense,

            cardDescription = "我是描述深潜牌的作用的",
            /// <summary> 一次防御的机会 这个value代表次数 </summary>
            value = 1,
            isRewardCard = false,
            cardSprite = defenseSprite,
        };
        
        recallCard = new CardData
        {
            cardID = 3,
            cardName = "余温",
            cardType = CardType.Recall,

            cardDescription = "我是描述余温牌的作用的",
            /// <summary> 回收牌的作用是一个行为 牌本身没有数值 </summary>
            value = 0,
            isRewardCard = false,
            cardSprite = recallSprite,
        };

        recoveryCard = new CardData
        {
            cardID = 4,
            cardName = "和解",
            cardType = CardType.Recovery,

            cardDescription = "我是描述和解牌的作用的",
            choiceEffects = new ChoiceEffect[]{new ChoiceEffect { effectName = "san", effectValue = 30 },
            new ChoiceEffect { effectName = "temperature", effectValue = 30 } },
            cardSprite = recoverySprite,
        };

    }

    //初始化牌库逻辑
}
