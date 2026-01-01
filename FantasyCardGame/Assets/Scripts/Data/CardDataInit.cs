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

    //大牌库和小牌库的总数量 方便后续数据更改
    public int baseDeckNum = 20, rewardDeckNUm;

    //继承mono 所以在awake中初始化
    private void Awake()
    {
        attackCard = new CardData
        {
            cardID = 1,
            cardName = "Edge",// ????动态生成名字吗 那这里好像应该填英文
            cardType = CardType.Attack,

            cardDescription = "Using this card can reduce the San value of Boss 20.",
            value = 20,
            //默认初始卡牌都不是奖励牌 如果需要奖励牌自行更改
            isRewardCard = false,
            cardSprite = attackSprite,
        };

        defenseCard = new CardData
        {
            cardID = 2,
            cardName = "Immersion",
            cardType = CardType.Defense,

            cardDescription = "Using this card can block one attack from the boss",
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
    //如果回退到第一关需要重置牌库 所以游戏中不止一次会初始化20张牌库 所以写成函数而不是在awake中


    public List<CardInstance> CreateBaseDeck()
    {
        List<CardInstance> baseDeck = new List<CardInstance>();
        //每种牌的张数
        int count = baseDeckNum / 4;

        //五张攻击牌(0-4)
        for(int i = 0; i < count; i++)
        {
            baseDeck.Add(new CardInstance(attackCard, DeckType.BaseDeck));
        }

        //五张防御牌（5-9）
        for(int i = count; i < 2 * count; i++)
        {
            baseDeck.Add(new CardInstance(defenseCard, DeckType.BaseDeck));
        }

        //五张回收牌(10-14)
        for(int i = 2 * count; i < 3 * count; i++)
        {
            baseDeck.Add(new CardInstance(recallCard, DeckType.BaseDeck));
        }

        //五张恢复牌(15-19)
        for(int i = 3 * count; i < 4 * count; i++)
        {
            baseDeck.Add(new CardInstance(recoveryCard, DeckType.BaseDeck));
        }

        return baseDeck;

    }
}
