using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public void ResetDeck()
    {
        InitDeck();
    }


    // 牌库：按类型计数
    private Dictionary<CardType, int> deckCounts;

    const int TOTAL_CARD_COUNT = 20;
    const int EACH_TYPE_COUNT = 5;

    void Awake()
    {
        Instance = this;
        InitDeck();
    }

    void InitDeck()
    {
        deckCounts = new Dictionary<CardType, int>()
        {
            { CardType.Attack, EACH_TYPE_COUNT },
            { CardType.Defense, EACH_TYPE_COUNT },
            { CardType.Recall, EACH_TYPE_COUNT },
            { CardType.Recovery, EACH_TYPE_COUNT }
        };

        Debug.Log("牌库初始化完成（20 张）");
    }
    public CardType DrawRandomCard()
    {
        List<CardType> available = new List<CardType>();

        foreach (var kv in deckCounts)
        {
            for (int i = 0; i < kv.Value; i++)
            {
                available.Add(kv.Key);
            }
        }

        if (available.Count == 0)
        {
            Debug.LogError("牌库为空！");
            return CardType.Attack;
        }

        CardType drawn = available[Random.Range(0, available.Count)];
        deckCounts[drawn]--;

        Debug.Log($"抽到卡牌：{drawn}");
        return drawn;
    }
    public void ReturnCard(CardType type)
    {
        deckCounts[type]++;

        Debug.Log($"卡牌回收：{type}");

        if (GetTotalCardCount() > TOTAL_CARD_COUNT)
        {
            Debug.LogError("牌库数量超过上限，请检查回收逻辑！");
        }
    }

    public int GetTotalCardCount()
    {
        int total = 0;
        foreach (var kv in deckCounts)
        {
            total += kv.Value;
        }
        return total;
    }



}
