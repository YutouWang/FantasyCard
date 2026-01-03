using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例模式
/// 管理卡牌的创建、牌库的初始化、抽牌等行为
/// 像 UIManager（统一管理所以面板的显隐） 一样统一管理卡片的行为
/// </summary>
/// 

//疑问：弃牌的方法也是在这里面提供的吧？是的
public class CardManager
{
    public static CardManager instance = new CardManager();

    //属性简化写法
    public static CardManager Instance => instance;

    //readonly 防止在其他地方实例化 赋值（只允许在申明处和构造函数中处理）
    private readonly System.Random random = new System.Random();

    //两个牌库
    private List<CardInstance> _baseDeck;
    private List<CardInstance> _rewardDeck;

    //手牌列表 现在手中的牌
    public readonly List<CardInstance> Hand = new List<CardInstance>();

    //本回合打出的牌列表
    public List<CardInstance> playedInThisTurn = new List<CardInstance>();

    //记录打出的上一张牌
    public CardInstance lastPlayed = null;


    //私有构造 防止外部new 实例
    private CardManager() { }


    //向外部提供的方法

    /// <summary>
    /// 把 CardDataInit 生成出来的牌库交给管理器（只需要调用一次，或每局重置时调用）
    /// </summary>
    public void Init(List<CardInstance> baseDeck, List<CardInstance> rewardDeck = null, bool clearHand = true)
    {
        //防止空异常处理
        if (baseDeck == null) throw new ArgumentNullException(nameof(baseDeck));

        _baseDeck = baseDeck;

        //"??"是空合并运算符  如果rewardDeck不是空 就传进来的参数 如果是空的 就新new 一个List
        _rewardDeck = rewardDeck ?? new List<CardInstance>();

        //这是第一关初始化手牌数据 一般都是第一次进游戏场景 或者 失败了回退第一关 要清理手牌list  不然会和继承的手牌冲突
        if (clearHand) Hand.Clear();


    }


    /// <summary>
    /// 抽卡方法
    /// </summary>
    /// <param name="count"> 需要传入抽取的数量 </param>
    /// <returns> 会返回抽取的卡片信息（序号还是实例待定）</returns>
    public IList<CardInstance> DrawCardsToHand(int count, DeckType deckType = DeckType.BaseDeck)
    {
        //抽取n张卡的逻辑
        //随机n个卡序号 实例化这两张卡片 存入List容器并且返回
        //(抽卡的时候只提供随机的序号)

        //防止空异常
        if (_baseDeck == null)
            throw new InvalidOperationException("CardManager 未初始化：先调用 CardManager.Instance.Init(baseDeck)");

        //先确定从哪个牌库里取
        List<CardInstance> deck = deckType == DeckType.BaseDeck ? _baseDeck : _rewardDeck;

        //drawn 是副本 先备份一下 是Hand中的牌 后续只为场上的牌做UI动画的时候就会用到这个
        List<CardInstance> drawn = new List<CardInstance>(count);

        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0) break;

            int index = random.Next(deck.Count); // 抽取范围 0 ~ deck.Count-1
            //抽出来的牌先存一下
            CardInstance card = deck[index];

            //从牌库中移除一下 但是移除的是deck 容器 不是_baseDeck啊？
            deck.RemoveAt(index);

            Hand.Add(card);
            drawn.Add(card);
        }

        return drawn;
    }


    //点击cardprefab上的按钮 后触发的事件OnCardClicked中就是这个逻辑
    //在Battlepanel中 Bind 的时候一起传过去
    //(函数名字面意思  把牌从hand区域 打到 played区域去)（出牌）
    public bool TryPlayToTable(CardInstance card)
    {
        //得写全情况 排除不能出牌的情况
        if (card == null)
            return false;

        //也是才发现 List的Remove还会返回一个bool确认是否移除hhhh
        //Hand.Remove(card) == false 进分支 代表移除失败 card不在 Hand 中 不能出牌
        if (!Hand.Remove(card))
            return false;

        //上面分支判断的时候已经把card 从Hand中移除了 下面只需要添加到对应的去处即可
        playedInThisTurn.Add(card);
        //方便回收牌回收
        lastPlayed = card;
        return true;
    }

    //取消出牌逻辑 Played——>Hand 和上面 TryPlayToTable 对应的
    public bool TryUnplayToHand(CardInstance card)
    {
        if (card == null)
            return false;

        if (!playedInThisTurn.Remove(card))
            return false;

        // 现在会加到队尾 不会回到原来的位置 但是起码能取消出牌了
        Hand.Add(card);   
        return true;
    }

    /// <summary>
    /// 回牌库方法 例如使用掉了以后回牌库 弃掉的牌回牌库
    /// </summary>
    public void ReturnPlayedToBaseDeck()
    {
        // 把出列牌全塞回 baseDeck
        for (int i = 0; i < playedInThisTurn.Count; i++)
        {
            _baseDeck.Add(playedInThisTurn[i]);
        }
        playedInThisTurn.Clear();
    }



}
