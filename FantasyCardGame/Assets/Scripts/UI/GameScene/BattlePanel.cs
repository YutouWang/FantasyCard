using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePanel : BasePanel
{
    [Header("拖拽引用")]
    public Transform handRoot;          //  手牌容器 用于实例化的时候 提供生成所需的位置信息
    public Transform playedZone;        //  手牌打出去的区域
    public GameObject cardPrefab;       //  从外部拖进来 CardPrefab

    [Header("Resources 路径")]
    public string cardDataInitPath = "Card/CardDataInit";

    public StatBar playerHpBar;
    public StatBar playerEnergyBar;
    public StatBar bossHpBar;


    public PlayerData player = new PlayerData();
    private BossData boss;


    public override void Init()
    {
        //因为上面这些动态加载的部分需要用到mono 所以写在battlepanel里面了

        #region 1.从 Resources 加载 CardDataInit
        GameObject initPrefab = Resources.Load<GameObject>(cardDataInitPath);
        if (initPrefab == null)
        {
            Debug.LogError(" 没有在 Resources/" + cardDataInitPath + "找到 CardDataInit prefab");
            return;
        }

        //实例化的预制体
        GameObject initObj = Instantiate(initPrefab);
        if (initObj == null)
        {
            print("initObj(carddatainit实例化不出来)");
        }
        //得到实例上挂载的脚本
        CardDataInit initScript = initObj.GetComponent<CardDataInit>();

        if (initScript == null)
        {
            Debug.LogError("没有在CardDataInit预制体上找到CardDataInit脚本");
            return;
        }
        #endregion

        // 2.创建基础牌库
        List<CardInstance> deck = initScript.CreateBaseDeck();

        #region 3. 将上面生成的Deck传进Cardmanager便于管理，随机抽5张牌入Hand列表 渲染Hand中的card（绑定UI数据）


        #region 原始测试初始化5张牌逻辑
        //for (int i = 0; i < 5; i++)
        //{
        //    GameObject aCard = Instantiate(cardPrefab, handRoot, false);
        //    Debug.Log(aCard == null ? "aCard == NULL" : $"aCard OK: {aCard.name}");

        //    //得到CardUI预制体上的脚本
        //    CardUI cardUIScript = aCard.GetComponent<CardUI>();

        //    if (cardUIScript == null)
        //    {
        //        Debug.LogError("CardPrefab 上没有 CardUI脚本 ");
        //        return;
        //    }
        //    if (i == 0)
        //        cardUIScript.BindInstance(deck[0]);
        //    if (i == 1)
        //        cardUIScript.BindInstance(deck[6]);
        //    if (i == 2)
        //        cardUIScript.BindInstance(deck[13]);
        //    if (i == 3)
        //        cardUIScript.BindInstance(deck[17]);
        //    if (i == 4)
        //        cardUIScript.BindInstance(deck[8]);
        //}
        #endregion

        //把牌库创建好 存到manager里便于后续管理牌库中的数据 （抽牌、弃牌回到牌库等操作）
        CardManager.Instance.Init(deck);

        //随机抽5张牌
        CardManager.Instance.DrawCardsToHand(5);

        RenderHand(CardManager.Instance.Hand);
        #endregion
    }

    //渲染手牌区域的手牌的函数
    private void RenderHand(List<CardInstance> hand)
    {
        // 先清空（比较保险 害怕层级中有别的gameObject）
        for (int i = handRoot.childCount - 1; i >= 0; i--)
            Destroy(handRoot.GetChild(i).gameObject);

        // 再生成
        foreach (CardInstance instance in hand)
        {
            GameObject card = Instantiate(cardPrefab, handRoot, false);
            card.GetComponent<CardUI>().BindInstance(instance,OnCardOnClicked);
        }
    }

    //渲染Played（Table）区域的手牌
    private void RenderPlayed(List<CardInstance> played)
    {
        //先清空
        for (int i = playedZone.childCount - 1; i >= 0; i--)
            Destroy(playedZone.GetChild(i).gameObject);

        foreach (CardInstance instance in played)
        {
            GameObject card = Instantiate(cardPrefab, playedZone, false);

            // 出列区牌的点击出牌逻辑 回调函数OnPlayedCardClicked 撤销这张出牌
            card.GetComponent<CardUI>().BindInstance(instance, OnPlayedCardClicked);
        }

    }

    //Hand区域button传入的回调函数
    private void OnCardOnClicked(CardInstance card)
    {
        print(card.cardTemplate.cardName + "被点击了一下，执行出牌逻辑");

        //出牌加判断
        if (CardManager.Instance.TryPlayToTable(card))
        {
            //两个牌列表都刷新渲染逻辑

            //出牌逻辑成功了再渲染在 played 区域
            RenderPlayed(CardManager.Instance.playedInThisTurn);

            //Hand 也要重新渲染 因为Hand移除卡牌了 重新渲染一次
            RenderHand(CardManager.Instance.Hand);
        }
            

    }
    //Played区域button传入的回调函数
    private void OnPlayedCardClicked(CardInstance card)
    {
        print(card.cardTemplate.cardName + "被点击了一下，撤销这张出牌");
        if (CardManager.Instance.TryUnplayToHand(card))
        {
            //在渲染一次两列表
            RenderHand(CardManager.Instance.Hand);
            RenderPlayed(CardManager.Instance.playedInThisTurn);
        }
        
    }

}
