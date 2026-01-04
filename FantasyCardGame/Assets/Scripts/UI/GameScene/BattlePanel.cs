using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BattlePanel : BasePanel
{
    [Header("基础实例化信息")]
    public Transform handRoot;          //  手牌容器 用于实例化的时候 提供生成所需的位置信息
    public Transform playedZone;        //  手牌打出去的区域
    public GameObject cardPrefab;       //  从外部拖进来 CardPrefab

    [Header("按钮/文本控件")]
    public Button endTurnBtn;
    public TMP_Text phaseHintText;

    [Header("Resources 路径")]
    public string cardDataInitPath = "Card/CardDataInit";

    [Header("血条")]
    public StatBar playerHpBar;
    public StatBar playerEnergyBar;
    public StatBar bossHpBar;

    //boss 和 player的申明
    public PlayerData player = new PlayerData();
    private BossData boss;

    //回合/阶段数据
    private enum PlayerPhase { Play, Discard }
    private PlayerPhase phase = PlayerPhase.Play;

    private int maxHandSize = 5;     // 目前测试第一关 为5
    private int shieldCharges = 0;   // 防御牌抵消 boss的 攻击次数



    public override void Init()
    {
        //因为上面这些动态加载的部分需要用到mono 所以写在battlepanel里面了

        #region 1.从 Resources 加载 CardDataInit 并生成牌库Deck
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

        // 创建基础牌库
        List<CardInstance> deck = initScript.CreateBaseDeck();
        #endregion



        #region 2. 将上面生成的Deck传进Cardmanager便于管理，随机抽5张牌入Hand列表 渲染Hand中的card（绑定UI数据）


        //把牌库创建好 存到manager里便于后续管理牌库中的数据 （抽牌、弃牌回到牌库等操作）
        CardManager.Instance.Init(deck);

        //随机抽5张牌
        CardManager.Instance.DrawCardsToHand(5);

        
        #endregion

        #region 血条相关的初始化

        player.Init();
        boss = new BossData(LevelType.Level1);
        boss.maxHp = 100;
        boss.currentHp = boss.maxHp;

        playerHpBar.Initialize(player.maxHP, player.currentHP);
        playerEnergyBar.Initialize(player.maxEnergy, player.currentEnergy);
        bossHpBar.Initialize(boss.maxHp, boss.currentHp);

        #endregion

        #region 4. 绑定 EndTurn

        if (endTurnBtn != null)
        {
            endTurnBtn.onClick.RemoveAllListeners();
            endTurnBtn.onClick.AddListener(OnEndTurnClicked);
        }
        else
        {
            Debug.LogWarning("endTurnBtn 没有绑定（你需要在面板上放一个按钮并拖进来）");
        }
        #endregion

        // 5) 进入出牌阶段（会渲染 hand/played）
        EnterPlayPhase();
    }

    // 阶段切换 出牌阶段
    private void EnterPlayPhase()
    {
        phase = PlayerPhase.Play;
        if (phaseHintText != null) phaseHintText.text = "Player Turn";

        RenderHand(CardManager.Instance.Hand, OnHandCardClicked_Play);
        RenderPlayed(CardManager.Instance.playedInThisTurn, OnPlayedCardClicked_Undo);
    }

    //阶段切换 弃牌阶段
    private void EnterDiscardPhase()
    {
        phase = PlayerPhase.Discard;
        int needDiscard = Mathf.Max(0, CardManager.Instance.Hand.Count - maxHandSize);
        if (phaseHintText != null) phaseHintText.text = $"Discard {needDiscard}";

        // 弃牌阶段：手牌点击=弃牌；出列区禁止撤回（传 null）
        RenderHand(CardManager.Instance.Hand, OnHandCardClicked_Discard);
        RenderPlayed(CardManager.Instance.playedInThisTurn, null);
    }

    //渲染手牌区域的手牌的函数
    private void RenderHand(List<CardInstance> hand, UnityAction<CardInstance> onClick)
    {
        // 先清空（比较保险 害怕层级中有别的gameObject）
        for (int i = handRoot.childCount - 1; i >= 0; i--)
            Destroy(handRoot.GetChild(i).gameObject);

        // 再生成
        foreach (CardInstance instance in hand)
        {
            GameObject card = Instantiate(cardPrefab, handRoot, false);
            card.GetComponent<CardUI>().BindInstance(instance, onClick);
        }
    }

    //渲染Played（Table）区域的手牌
    private void RenderPlayed(List<CardInstance> played, UnityAction<CardInstance> onClick)
    {
        //先清空
        for (int i = playedZone.childCount - 1; i >= 0; i--)
            Destroy(playedZone.GetChild(i).gameObject);

        foreach (CardInstance instance in played)
        {
            GameObject card = Instantiate(cardPrefab, playedZone, false);

            // 出列区牌的点击出牌逻辑 回调函数OnPlayedCardClicked 撤销这张出牌
            card.GetComponent<CardUI>().BindInstance(instance, onClick);
        }

    }

    #region  点击事件的回调  出牌/撤回/弃牌

    //出牌
    private void OnHandCardClicked_Play(CardInstance card)
    {
        if (CardManager.Instance.TryPlayToTable(card))
            EnterPlayPhase(); // 刷新 hand + played
    }

    //撤销出牌
    private void OnPlayedCardClicked_Undo(CardInstance card)
    {
        if (CardManager.Instance.TryUnplayToHand(card))
            EnterPlayPhase();
    }

    //弃牌
    private void OnHandCardClicked_Discard(CardInstance card)
    {
        if (CardManager.Instance.DiscardFromHand(card))
        {
            if (CardManager.Instance.Hand.Count > maxHandSize)
            {
                EnterDiscardPhase(); // 还没弃够继续
            }
            else
            {
                ResolveEndTurn(); // 弃够了，直接结算
            }
        }
    }
    #endregion



    #region EndTurn 按钮 逻辑
    private void OnEndTurnClicked()
    {
        // 如果正在弃牌阶段
        if (phase == PlayerPhase.Discard)
        {
            if (CardManager.Instance.Hand.Count > maxHandSize)
            {
                EnterDiscardPhase(); // 更新提示
                return;
            }
            ResolveEndTurn();
            return;
        }

        // 正常出牌阶段点击 EndTurn
        if (CardManager.Instance.Hand.Count > maxHandSize)
        {
            EnterDiscardPhase();
            return;
        }

        ResolveEndTurn();
    }

    #endregion

    //回合结算：出列牌效果 -> boss行动 -> 出列回库 -> 抽2 -> 刷新 
    private void ResolveEndTurn()
    {
        // 1) 结算玩家出列牌（今晚先做 3 张：Attack/Defense/Recovery；Recall 明天再补）
        List<CardInstance> played = CardManager.Instance.playedInThisTurn;

        for (int i = 0; i < played.Count; i++)
        {
            CardInstance c = played[i];
            CardType type = c.cardTemplate.cardType;

            if (type == CardType.Attack)
            {
                boss.TakeDamage(c.cardTemplate.value);
            }
            else if (type == CardType.Defense)
            {
                shieldCharges += 1; // 挡一次 boss 攻击
            }
            else if (type == CardType.Recovery)
            {
                // 先做一个“自动选择”：哪个更缺补哪个（不做弹窗也能玩）
                float hpRatio = (float)player.currentHP / player.maxHP;
                float enRatio = (float)player.currentEnergy / player.maxEnergy;

                // 这里的数值你之后可以换成 cardTemplate.choiceEffects
                if (hpRatio <= enRatio) player.Heal(12);
                else player.RecoverEnergy(8);
            }
        }

        // 2) 判胜利
        if (boss.IsDead())
        {
            bossHpBar.UpdateValue(boss.currentHp);
            if (phaseHintText != null) phaseHintText.text = "WIN!";
            Debug.Log("WIN!");
            return;
        }

        // 3) Boss 行动（今晚先固定攻击；偷牌明天再加）
        int damage = boss.GetAttackDamage();
        if (shieldCharges > 0)
        {
            shieldCharges--;
            damage = 0;
        }
        player.TakeDamage(damage);

        // 4) 判失败
        if (!player.isAlive)
        {
            playerHpBar.UpdateValue(player.currentHP);
            if (phaseHintText != null) phaseHintText.text = "LOSE!";
            Debug.Log("LOSE!");
            return;
        }

        // 5) 出列牌回库并清空
        CardManager.Instance.ReturnPlayedToBaseDeck();

        // 6) 回合开始抽 2
        CardManager.Instance.DrawCardsToHand(2);

        // 7) 刷新血条
        playerHpBar.UpdateValue(player.currentHP);
        playerEnergyBar.UpdateValue(player.currentEnergy);
        bossHpBar.UpdateValue(boss.currentHp);

        // 8) 回到出牌阶段（刷新 UI）
        EnterPlayPhase();
    }

}
