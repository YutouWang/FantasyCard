using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BattlePanel : BasePanel
{
    [Header("区域引用")]
    public Transform handRoot;          // 手牌容器
    public Transform playedZone;        // 出列区（本回合已打出的牌）
    public GameObject cardPrefab;       // CardPrefab（上面挂 CardUI）

    [Header("按钮 / 文本")]
    public Button endTurnBtn;           // 结束回合按钮
    public TMP_Text phaseHintText;      // 提示文本：Player Turn / Discard X

    [Header("Resources 路径")]
    public string cardDataInitPath = "Card/CardDataInit";

    [Header("血条")]
    public StatBar playerHpBar;
    public StatBar playerEnergyBar;
    public StatBar bossHpBar;

    // 数据
    public PlayerData player = new PlayerData();
    private BossData boss;

    // 阶段
    private enum PlayerPhase { Play, Discard }
    private PlayerPhase phase = PlayerPhase.Play;

    [Header("规则")]
    public int maxHandSize = 5;         // 手牌上限（1/2关 = 5）
    public int startHandCount = 5;      // 初始手牌
    public int drawEachTurn = 2;        // 每回合抽牌数
    public int energyCostPerTurn = 10;  // 每回合扣体温/能量
    private int shieldCharges = 0;      // 防御牌挡几次

    public override void Init()
    {
        // 1) 初始化牌库
        CardDataInit initScript = LoadCardDataInit();
        if (initScript == null) return;

        List<CardInstance> deck = initScript.CreateBaseDeck();
        CardManager.Instance.Init(deck);           // 把 deck 交给 CardManager 管
        CardManager.Instance.DrawCardsToHand(startHandCount);

        // 2) 初始化角色与血条
        player.Init();
        boss = new BossData(LevelType.Level1);
        boss.maxHp = 100;
        boss.currentHp = boss.maxHp;

        if (playerHpBar != null) playerHpBar.Initialize(player.maxHP, player.currentHP);
        if (playerEnergyBar != null) playerEnergyBar.Initialize(player.maxEnergy, player.currentEnergy);
        if (bossHpBar != null) bossHpBar.Initialize(boss.maxHp, boss.currentHp);

        // 3) 绑定 EndTurn 按钮
        if (endTurnBtn != null)
        {
            endTurnBtn.onClick.RemoveAllListeners();
            endTurnBtn.onClick.AddListener(OnEndTurnClicked);
        }
        else
        {
            Debug.LogWarning("endTurnBtn 没有绑定：请在 BattlePanel 预制体上拖入按钮");
        }

        // 4) 进入出牌阶段（刷新UI）
        EnterPlayPhase();
    }

    // -----------------------------
    // 初始化：加载 CardDataInit
    // -----------------------------
    private CardDataInit LoadCardDataInit()
    {
        GameObject initPrefab = Resources.Load<GameObject>(cardDataInitPath);
        if (initPrefab == null)
        {
            Debug.LogError("没有在 Resources/" + cardDataInitPath + " 找到 CardDataInit prefab");
            return null;
        }

        GameObject initObj = Instantiate(initPrefab);
        CardDataInit initScript = initObj.GetComponent<CardDataInit>();
        if (initScript == null)
        {
            Debug.LogError("CardDataInit 预制体上没有 CardDataInit 脚本");
            return null;
        }
        return initScript;
    }

    // -----------------------------
    // 阶段切换
    // -----------------------------
    private void EnterPlayPhase()
    {
        phase = PlayerPhase.Play;
        if (phaseHintText != null) phaseHintText.text = "Player Turn";

        RenderHand(CardManager.Instance.Hand, OnHandCardClicked_Play);
        RenderPlayed(CardManager.Instance.playedInThisTurn, OnPlayedCardClicked_Undo);
    }

    private void EnterDiscardPhase()
    {
        phase = PlayerPhase.Discard;
        int needDiscard = Mathf.Max(0, CardManager.Instance.Hand.Count - maxHandSize);
        if (phaseHintText != null) phaseHintText.text = $"Discard {needDiscard}";

        // 弃牌阶段：手牌点一下 = 弃牌；出列区不允许撤回
        RenderHand(CardManager.Instance.Hand, OnHandCardClicked_Discard);
        RenderPlayed(CardManager.Instance.playedInThisTurn, null);
    }

    // -----------------------------
    // 渲染
    // -----------------------------
    private void RenderHand(List<CardInstance> hand, UnityAction<CardInstance> onClick)
    {
        for (int i = handRoot.childCount - 1; i >= 0; i--)
            Destroy(handRoot.GetChild(i).gameObject);

        foreach (CardInstance instance in hand)
        {
            GameObject go = Instantiate(cardPrefab, handRoot, false);
            CardUI ui = go.GetComponent<CardUI>();
            ui.BindInstance(instance, onClick);
        }
    }

    private void RenderPlayed(List<CardInstance> played, UnityAction<CardInstance> onClick)
    {
        for (int i = playedZone.childCount - 1; i >= 0; i--)
            Destroy(playedZone.GetChild(i).gameObject);

        foreach (CardInstance instance in played)
        {
            GameObject go = Instantiate(cardPrefab, playedZone, false);
            CardUI ui = go.GetComponent<CardUI>();
            ui.BindInstance(instance, onClick);
        }
    }

    // -----------------------------
    // 点击回调：出牌 / 撤回 / 弃牌
    // -----------------------------
    private void OnHandCardClicked_Play(CardInstance card)
    {
        if (CardManager.Instance.TryPlayToTable(card))
            EnterPlayPhase(); // 刷新
    }

    private void OnPlayedCardClicked_Undo(CardInstance card)
    {
        if (CardManager.Instance.TryUnplayToHand(card))
            EnterPlayPhase();
    }

    private void OnHandCardClicked_Discard(CardInstance card)
    {
        // 弃一张
        if (CardManager.Instance.DiscardFromHand(card))
        {
            // 还超手牌上限则继续弃
            if (CardManager.Instance.Hand.Count > maxHandSize)
            {
                EnterDiscardPhase();
            }
            else
            {
                ResolveEndTurn();
            }
        }
    }

    // -----------------------------
    // EndTurn 按钮
    // -----------------------------
    private void OnEndTurnClicked()
    {
        if (phase == PlayerPhase.Discard)
        {
            // 弃牌阶段点 EndTurn：只有弃够才结算
            if (CardManager.Instance.Hand.Count > maxHandSize)
            {
                EnterDiscardPhase();
                return;
            }
            ResolveEndTurn();
            return;
        }

        // 出牌阶段点 EndTurn：若超上限则进入弃牌，否则直接结算
        if (CardManager.Instance.Hand.Count > maxHandSize)
        {
            EnterDiscardPhase();
            return;
        }

        ResolveEndTurn();
    }

    // -----------------------------
    // 回合结算：玩家牌效果 -> Boss行动 -> 回库 -> 抽牌 -> 刷新
    // -----------------------------
    private void ResolveEndTurn()
    {
        // A) 回合开始先扣体温/能量（你设定每回合 -10）
        player.ConsumeEnergy(energyCostPerTurn);

        // B) 结算玩家出列牌
        List<CardInstance> played = CardManager.Instance.playedInThisTurn;

        for (int i = 0; i < played.Count; i++)
        {
            CardInstance c = played[i];
            CardType type = c.cardTemplate.cardType;

            switch (type)
            {
                case CardType.Attack:
                    boss.TakeDamage(c.cardTemplate.value);
                    break;

                case CardType.Defense:
                    shieldCharges += 1; // 挡一次
                    break;

                case CardType.Recovery:
                    // 先用固定逻辑（省弹窗）：缺啥补啥
                    float hpRatio = (float)player.currentHP / player.maxHP;
                    float enRatio = (float)player.currentEnergy / player.maxEnergy;

                    if (hpRatio <= enRatio) player.Heal(12);
                    else player.RecoverEnergy(8);
                    break;

                case CardType.Recall:
                    // 你如果还没做回收，就先跳过（不影响闭环）
                    // TODO：明天补：把 lastPlayed 放回 Hand（或抽2张等）
                    break;
            }
        }

        // C) 同步 Boss 血条 + 判胜
        if (bossHpBar != null) bossHpBar.UpdateValue(boss.currentHp);
        if (boss.IsDead())
        {
            if (phaseHintText != null) phaseHintText.text = "WIN!";
            Debug.Log("WIN!");
            return;
        }

        // D) Boss 行动（先只做攻击，偷牌明天加）
        int damage = boss.GetAttackDamage();
        if (shieldCharges > 0)
        {
            shieldCharges--;
            damage = 0;
        }
        player.TakeDamage(damage);

        // E) 判负（HP 或 体温为0都算输）
        bool energyDead = player.currentEnergy <= 0;
        if (!player.isAlive || energyDead)
        {
            if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);
            if (playerEnergyBar != null) playerEnergyBar.UpdateValue(player.currentEnergy);

            if (phaseHintText != null) phaseHintText.text = "LOSE!";
            Debug.Log("LOSE!");
            return;
        }

        // F) 出列牌回库
        CardManager.Instance.ReturnPlayedToBaseDeck();

        // G) 抽 2
        CardManager.Instance.DrawCardsToHand(drawEachTurn);

        // H) 刷新血条
        if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);
        if (playerEnergyBar != null) playerEnergyBar.UpdateValue(player.currentEnergy);
        if (bossHpBar != null) bossHpBar.UpdateValue(boss.currentHp);

        // I) 回到出牌阶段
        EnterPlayPhase();
    }
}
