using System.Collections;
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
    public TMP_Text phaseHintText;      // 提示文本

    [Header("抽牌按钮")]
    public Button drawBtn;
    private bool drewThisTurn = false;

    [Header("Resources 路径")]
    public string cardDataInitPath = "Card/CardDataInit";

    [Header("血条")]
    public StatBar playerHpBar;
    public StatBar playerEnergyBar;
    public StatBar bossHpBar;

    [Header("Boss 特效/节奏")]
    public GameObject scratchFx;            // 抓痕图（GameObject，带 Image）
    public float scratchShowTime = 1.5f;    // 抓痕显示时间
    public float bossActDelay = 0.8f;       // Boss 回合开始前停顿
    public float afterPlayerResolveDelay = 0.8f; // 玩家结算后停顿
    public float afterBossResolveDelay = 0.8f;   // Boss 行动后停顿
    public float playedVanishDelay = 0.15f;      // 出列牌“先消失”的停顿
    public float stealShowTime = 1.0f;           // 偷牌提示停留时间

    // 数据
    public PlayerData player = new PlayerData();
    private BossData boss;

    // 回合数据
    private int turnIndex = 1;
    private int attackUsedThisTurn = 0;
    private int shieldCharges = 0;      // 防御牌挡几次

    // 记住“上一回合最后一张打出的牌”（用于 Recall）
    private CardInstance lastPlayedPrevTurn = null;

    // 阶段
    private enum PlayerPhase { Play, Discard }
    private PlayerPhase phase = PlayerPhase.Play;
    private bool pendingNextTurnDiscard = false; // 结算后需要弃牌才能开下一回合


    [Header("规则")]
    public int maxHandSize = 5;         // 手牌上限
    public int startHandCount = 5;      // 初始手牌（进入战斗就给）
    public int drawEachTurn = 2;        // 每回合抽牌数（手动点 Draw 抽）
    public int energyCostPerTurn = 10;  // 每回合扣体温/能量

    public override void Init()
    {
        // 1) 初始化牌库
        CardDataInit initScript = LoadCardDataInit();
        if (initScript == null) return;

        List<CardInstance> deck = initScript.CreateBaseDeck();
        CardManager.Instance.Init(deck);
        CardManager.Instance.DrawCardsToHand(startHandCount);

        // 2) 初始化角色与血条
        player.Init();
        boss = new BossData(LevelType.Level1);
        boss.maxHp = 100;
        boss.currentHp = boss.maxHp;

        if (playerHpBar != null) playerHpBar.Initialize(player.maxHP, player.currentHP);
        if (playerEnergyBar != null) playerEnergyBar.Initialize(player.maxEnergy, player.currentEnergy);
        if (bossHpBar != null) bossHpBar.Initialize(boss.maxHp, boss.currentHp);

        // 3) 绑定按钮
        if (endTurnBtn != null)
        {
            endTurnBtn.onClick.RemoveAllListeners();
            endTurnBtn.onClick.AddListener(OnEndTurnClicked);
        }
        else
        {
            Debug.LogWarning("endTurnBtn 没有绑定：请在 BattlePanel 预制体上拖入按钮");
        }

        if (drawBtn != null)
        {
            drawBtn.onClick.RemoveAllListeners();
            drawBtn.onClick.AddListener(OnDrawClicked);
        }

        // 抓痕默认隐藏
        if (scratchFx != null) scratchFx.SetActive(false);

        // 进入第一回合
        StartPlayerTurn();
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

    private void StartPlayerTurn()
    {
        attackUsedThisTurn = 0;
        drewThisTurn = false;

        // Draw 可点，EndTurn 不可点（必须先抽牌）
        if (drawBtn != null) drawBtn.interactable = true;
        if (endTurnBtn != null) endTurnBtn.interactable = false;

        // 回到出牌阶段（但提示会根据 drewThisTurn 显示“Click DRAW”）
        EnterPlayPhase();
    }

    // -----------------------------
    // 阶段切换
    // -----------------------------
    private void EnterPlayPhase()
    {
        phase = PlayerPhase.Play;

        if (phaseHintText != null)
        {
            phaseHintText.text = drewThisTurn
                ? $"Turn {turnIndex} - Player Turn"
                : $"Turn {turnIndex} - Click DRAW";
        }

        // 出列区恢复显示
        if (playedZone != null && !playedZone.gameObject.activeSelf)
            playedZone.gameObject.SetActive(true);

        RenderHand(CardManager.Instance.Hand, OnHandCardClicked_Play);
        RenderPlayed(CardManager.Instance.playedInThisTurn, OnPlayedCardClicked_Undo);
    }

    private void EnterDiscardPhase()
    {
        phase = PlayerPhase.Discard;

        int needDiscard = Mathf.Max(0, CardManager.Instance.Hand.Count - maxHandSize);
        if (phaseHintText != null)
            phaseHintText.text = $"Turn {turnIndex} - DISCARD PHASE: discard {needDiscard} card(s)";

        // 弃牌阶段：手牌点一下=弃牌；出列区不允许撤回
        RenderHand(CardManager.Instance.Hand, OnHandCardClicked_Discard);
        RenderPlayed(CardManager.Instance.playedInThisTurn, null);

        // 弃牌阶段 EndTurn 可点（用于确认/继续）
        if (endTurnBtn != null) endTurnBtn.interactable = true;
        if (drawBtn != null) drawBtn.interactable = false;
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
    // 点击回调：出牌 / 撤回 / 弃牌 / 抽牌
    // -----------------------------
    private void OnHandCardClicked_Play(CardInstance card)
    {
        if (!drewThisTurn)
        {
            if (phaseHintText != null) phaseHintText.text = $"Turn {turnIndex} - Draw first!";
            return;
        }

        if (card == null) return;

        CardType type = card.cardTemplate.cardType;

        // 规则：第一回合不能出回收牌
        if (turnIndex == 1 && type == CardType.Recall)
        {
            if (phaseHintText != null) phaseHintText.text = $"Turn {turnIndex} - Recall can't be used in Turn 1";
            return;
        }

        // 规则：每回合只能出 1 张攻击牌
        if (type == CardType.Attack && attackUsedThisTurn >= 1)
        {
            if (phaseHintText != null) phaseHintText.text = $"Turn {turnIndex} - Only 1 Attack per turn";
            return;
        }

        if (CardManager.Instance.TryPlayToTable(card))
        {
            if (type == CardType.Attack) attackUsedThisTurn++;
            EnterPlayPhase();
        }
    }

    private void OnPlayedCardClicked_Undo(CardInstance card)
    {
        if (CardManager.Instance.TryUnplayToHand(card))
            EnterPlayPhase();
    }

    private void OnHandCardClicked_Discard(CardInstance card)
    {
        if (CardManager.Instance.DiscardFromHand(card))
        {
            if (CardManager.Instance.Hand.Count > maxHandSize)
            {
                EnterDiscardPhase();
            }
            else
            {
                if (pendingNextTurnDiscard)
                {
                    pendingNextTurnDiscard = false;
                    turnIndex++;
                    StartPlayerTurn();   // 直接进下一回合（不要再结算一次）
                }
                else
                {
                    StartCoroutine(ResolveEndTurnRoutine()); // 这是“结算前弃牌”才会走到这
                }
            }
        }
    }

    private void OnDrawClicked()
    {
        if (drewThisTurn) return;

        CardManager.Instance.DrawCardsToHand(drawEachTurn);
        drewThisTurn = true;

        if (drawBtn != null) drawBtn.interactable = false;
        if (endTurnBtn != null) endTurnBtn.interactable = true;

        EnterPlayPhase(); // 刷新提示/渲染
    }

    // -----------------------------
    // EndTurn
    // -----------------------------
    private void OnEndTurnClicked()
    {
        // 弃牌阶段
        if (phase == PlayerPhase.Discard)
        {
            if (CardManager.Instance.Hand.Count > maxHandSize)
            {
                EnterDiscardPhase();
                return;
            }

            // 如果是结算后弃牌：弃够了就直接开下一回合
            if (pendingNextTurnDiscard)
            {
                pendingNextTurnDiscard = false;
                turnIndex++;
                StartPlayerTurn();
                return;
            }

            StartCoroutine(ResolveEndTurnRoutine());
            return;
        }

        // 出牌阶段：若超上限进入弃牌，否则结算
        if (CardManager.Instance.Hand.Count > maxHandSize)
        {
            EnterDiscardPhase();
            return;
        }

        StartCoroutine(ResolveEndTurnRoutine());
    }

    // -----------------------------
    // 回合结算协程（你要改节奏就改这里）
    // -----------------------------
    private IEnumerator ResolveEndTurnRoutine()
    {
        // 锁按钮（防连点）
        if (endTurnBtn != null) endTurnBtn.interactable = false;
        if (drawBtn != null) drawBtn.interactable = false;

        // 先把手牌/出列都禁用点击（但仍显示）
        RenderHand(CardManager.Instance.Hand, null);
        RenderPlayed(CardManager.Instance.playedInThisTurn, null);

        if (phaseHintText != null) phaseHintText.text = $"Turn {turnIndex} - Resolving...";

        // 0) 本回合扣体温/能量（只扣一次）
        player.ConsumeEnergy(energyCostPerTurn);
        if (playerEnergyBar != null) playerEnergyBar.UpdateValue(player.currentEnergy);

        if (player.currentEnergy <= 0)
        {
            Lose("Energy depleted");
            yield break;
        }

        // 1) 出列牌先“消失”（视觉上）
        if (playedZone != null) playedZone.gameObject.SetActive(false);
        if (playedVanishDelay > 0f) yield return new WaitForSeconds(playedVanishDelay);

        // 2) 记录“本回合最后一张出牌”（用于下回合 Recall）
        List<CardInstance> played = CardManager.Instance.playedInThisTurn;
        CardInstance lastPlayedThisTurn = (played != null && played.Count > 0) ? played[played.Count - 1] : null;

        // 3) 结算玩家出列牌
        bool recallTriggered = false;

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
                shieldCharges += 1;
            }
            else if (type == CardType.Recovery)
            {
                // 临时策略：缺啥补啥（你之后换成弹窗选择）
                float hpRatio = (float)player.currentHP / player.maxHP;
                float enRatio = (float)player.currentEnergy / player.maxEnergy;
                if (hpRatio <= enRatio) player.Heal(12);
                else player.RecoverEnergy(8);
            }
            else if (type == CardType.Recall)
            {
                // Recall：把“上一回合最后一张出牌”回收到手牌（第一回合已被禁止出）
                if (!recallTriggered)
                {
                    recallTriggered = true;

                    if (lastPlayedPrevTurn != null)
                    {
                        CardManager.Instance.TryRecallSpecificToHand(lastPlayedPrevTurn);
                        // 需要你在 CardManager 加一个函数（我下面给你）
                    }
                }
            }
        }

        // 4) 刷条（玩家结算完 → Boss 掉血/回血清楚可见）
        if (bossHpBar != null) bossHpBar.UpdateValue(boss.currentHp);
        if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);
        if (playerEnergyBar != null) playerEnergyBar.UpdateValue(player.currentEnergy);

        if (afterPlayerResolveDelay > 0f)
        {
            if (phaseHintText != null) phaseHintText.text = $"Turn {turnIndex} - Player effects resolved";
            yield return new WaitForSeconds(afterPlayerResolveDelay);
        }

        // 5) 判胜
        if (boss.IsDead())
        {
            Win();
            yield break;
        }

        // 6) Boss 回合（延迟 + 行动）
        if (phaseHintText != null) phaseHintText.text = $"Turn {turnIndex} - Boss Turn...";
        if (bossActDelay > 0f) yield return new WaitForSeconds(bossActDelay);

        int skill = boss.GetRandomSkill();
        if (skill == 0)
        {
            yield return StartCoroutine(BossAttackRoutine());
        }
        else
        {
            yield return StartCoroutine(BossStealRoutine());
        }

        if (afterBossResolveDelay > 0f)
        {
            if (phaseHintText != null) phaseHintText.text = $"Turn {turnIndex} - Boss action resolved";
            yield return new WaitForSeconds(afterBossResolveDelay);
        }

        // 7) 判负
        if (!player.isAlive || player.currentEnergy <= 0)
        {
            Lose("HP/Energy depleted");
            yield break;
        }

        // 8) 更新“上一回合最后一张出牌”
        lastPlayedPrevTurn = lastPlayedThisTurn;

        // 9) 出列牌回库 & 清空出列
        CardManager.Instance.ReturnPlayedToBaseDeck();

        for (int i = playedZone.childCount - 1; i >= 0; i--)
            Destroy(playedZone.GetChild(i).gameObject);

        // 结算完如果超手牌上限：先弃牌，弃够了才进下一回合
        if (CardManager.Instance.Hand.Count > maxHandSize)
        {
            pendingNextTurnDiscard = true;

            EnterDiscardPhase();

            // 按钮状态：弃牌阶段一般不给 Draw
            if (drawBtn != null) drawBtn.interactable = false;
            if (endTurnBtn != null) endTurnBtn.interactable = true;

            yield break; //不进入下一回合
        }


        

        // 10) 下一回合
        turnIndex++;
        StartPlayerTurn();
    }

    // -----------------------------
    // Boss：攻击（抓痕 + 扣血）
    // -----------------------------
    private IEnumerator BossAttackRoutine()
    {
        if (phaseHintText != null) phaseHintText.text = "Boss attacks!";

        if (scratchFx != null) scratchFx.SetActive(true);
        if (scratchShowTime > 0f) yield return new WaitForSeconds(scratchShowTime);
        if (scratchFx != null) scratchFx.SetActive(false);

        int damage = boss.GetAttackDamage();
        if (shieldCharges > 0)
        {
            shieldCharges--;
            damage = 0;
        }

        player.TakeDamage(damage);
        if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);

        yield return null;
    }

    // -----------------------------
    // Boss：偷牌（简化版：随机偷 1 张回库）
    // -----------------------------
    private IEnumerator BossStealRoutine()
    {
        if (phaseHintText != null) phaseHintText.text = "Boss steals a card!";

        if (CardManager.Instance.Hand.Count > 0)
        {
            int idx = Random.Range(0, CardManager.Instance.Hand.Count);
            CardInstance stolen = CardManager.Instance.Hand[idx];

            CardManager.Instance.Hand.RemoveAt(idx);
            CardManager.Instance.ReturnCardToBaseDeck(stolen);

            // 刷一下手牌，让玩家看到少了一张（Boss 回合禁用点击）
            RenderHand(CardManager.Instance.Hand, null);
        }

        if (stealShowTime > 0f) yield return new WaitForSeconds(stealShowTime);
    }

    // -----------------------------
    // Win / Lose
    // -----------------------------
    private void Win()
    {
        if (bossHpBar != null) bossHpBar.UpdateValue(boss.currentHp);
        if (phaseHintText != null) phaseHintText.text = "WIN!";
        if (endTurnBtn != null) endTurnBtn.interactable = false;
        if (drawBtn != null) drawBtn.interactable = false;
        Debug.Log("WIN!");
    }

    private void Lose(string reason)
    {
        if (phaseHintText != null) phaseHintText.text = "LOSE!";
        if (endTurnBtn != null) endTurnBtn.interactable = false;
        if (drawBtn != null) drawBtn.interactable = false;
        Debug.Log("LOSE: " + reason);
    }
}
