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

//疑问：弃牌的方法也是在这里面提供的吧？
public class CardManager
{
    public static CardManager instance = new CardManager();

    //属性简化写法
    public static CardManager Instance => instance;

    private readonly System.Random _rng = new System.Random();

    private List<CardInstance> _baseDeck;
    private List<CardInstance> _rewardDeck;

    public readonly List<CardInstance> Hand = new List<CardInstance>();

    public bool IsInitialized { get; private set; }

    private CardManager() { }


    //向外部提供的方法

    /// <summary>
    /// 把 CardDataInit 生成出来的牌库交给管理器（只需要调用一次，或每局重置时调用）
    /// </summary>
    public void Init(List<CardInstance> baseDeck, List<CardInstance> rewardDeck = null, bool clearHand = true)
    {
        if (baseDeck == null) throw new ArgumentNullException(nameof(baseDeck));

        _baseDeck = baseDeck;
        _rewardDeck = rewardDeck ?? new List<CardInstance>();

        if (clearHand) Hand.Clear();

        IsInitialized = true;
    }


   
    /// <summary>
    /// 抽卡方法
    /// </summary>
    /// <param name="count"> 需要传入抽取的数量 </param>
    /// <returns> 会返回抽取的卡片信息（序号还是实例待定）</returns>
    public IList<CardInstance> DrawCardsToHand(int count, DeckType from = DeckType.BaseDeck)
    {
        //抽取n张卡的逻辑
        //随机n个卡序号 实例化这两张卡片 存入List容器并且返回
        //(抽卡的时候只提供随机的序号)
        EnsureInitialized();

        var deck = GetDeck(from);
        var drawn = new List<CardInstance>(count);

        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0) break;

            int idx = _rng.Next(deck.Count); // 0 ~ deck.Count-1
            var card = deck[idx];
            deck.RemoveAt(idx);

            Hand.Add(card);
            drawn.Add(card);
        }

        return drawn;
    }

    private List<CardInstance> GetDeck(DeckType type)
        => type == DeckType.BaseDeck ? _baseDeck : _rewardDeck;

    private void EnsureInitialized()
    {
        if (!IsInitialized || _baseDeck == null)
            throw new InvalidOperationException("CardManager 未初始化：先调用 CardManager.Instance.Init(baseDeck)");
    }

}
