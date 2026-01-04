using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BattlePanel : BasePanel
{
    [Header("区域引用")]
    public Transform handRoot;
    public Transform playedZone;
    public GameObject cardPrefab;

    [Header("Boss Roots 素材 (每关)")]
    public GameObject bossLv1Root;
    public GameObject bossLv2Root;
    public GameObject bossLv3Root;

    [Header("按钮 / 文本")]
    public Button endTurnBtn;
    public TMP_Text phaseHintText;

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
    public GameObject scratchFx;
    public float scratchShowTime = 1.5f;
    public float bossActDelay = 2f;
    public float afterPlayerResolveDelay = 2f;
    public float afterBossResolveDelay = 2f;
    public float playedVanishDelay = 0.15f;
    public float stealShowTime = 1.5f;

    // 数据
    public PlayerData player = new PlayerData();
    private BossData boss;

    // 回合数据
    private int turnIndex = 1;
    private int shieldCharges = 0;

    // Recall：上一回合最后一张出牌
    private CardInstance lastPlayedPrevTurn = null;

    // 阶段
    private enum PlayerPhase { Play, Discard }
    private PlayerPhase phase = PlayerPhase.Play;
    private bool pendingNextTurnDiscard = false;

    private bool _inited = false;

    [Header("规则")]
    public int maxHandSize = 5;
    public int startHandCount = 5;
    public int drawEachTurn = 2;
    public int energyCostPerTurn = 10;

    // 天赋（从 ProgressManager 读）
    private TalentType Talent => ProgressManager.Instance.Data.selectedTalent;
    private LevelType Level => ProgressManager.Instance.Data.currentLevel;

    public override void Init()
    {
        if (_inited) return;
        _inited = true;

        ApplyTalentRules();

        // 初始化牌库（只做一次）
        if (!CardManager.Instance.IsInitialized)
        {
            CardDataInit initScript = LoadCardDataInit();
            if (initScript == null) return;

            List<CardInstance> deck = initScript.CreateBaseDeck();
            CardManager.Instance.Init(deck);
            CardManager.Instance.DrawCardsToHand(startHandCount); // 初始5张只给一次
        }

        // 初始化 player / boss
        player.Init();
        SetupBossForLevel(Level);

        if (playerHpBar != null) playerHpBar.Initialize(player.maxHP, player.currentHP);
        if (playerEnergyBar != null) playerEnergyBar.Initialize(player.maxEnergy, player.currentEnergy);
        if (bossHpBar != null) bossHpBar.Initialize(boss.maxHp, boss.currentHp);

        // 绑定按钮
        if (endTurnBtn != null)
        {
            endTurnBtn.onClick.RemoveAllListeners();
            endTurnBtn.onClick.AddListener(OnEndTurnClicked);
        }

        if (drawBtn != null)
        {
            drawBtn.onClick.RemoveAllListeners();
            drawBtn.onClick.AddListener(OnDrawClicked);
        }

        if (scratchFx != null) scratchFx.SetActive(false);

        // 保存 checkpoint（你有就留，没有也不影响跑）
        ProgressManager.Instance.SaveCheckpoint(Level, player, CardManager.Instance);

        // 开始本关第1回合
        turnIndex = 1;
        shieldCharges = 0;
        pendingNextTurnDiscard = false;
        lastPlayedPrevTurn = null;

        StartPlayerTurn();
    }

    private void ApplyTalentRules()
    {
        energyCostPerTurn = 10;
        drawEachTurn = 2;

        if (Talent == TalentType.Scarf) energyCostPerTurn = 5;
        else if (Talent == TalentType.Bear) drawEachTurn = 3;
        // Med 在“Submerge 生效”时回血
    }

    private void SetupBossForLevel(LevelType level)
    {
        maxHandSize = 5; // Level1/2/3 都固定 5

        //  1) 生成 Boss 数据（每关重置）
        boss = new BossData(level);

        //  2) 给每关一个 maxHp（你可以按策划改）
        boss.maxHp = GetBossMaxHp(level);
        boss.currentHp = boss.maxHp;

        //  3) 换 Boss 视觉（最关键！）
        ApplyBossVisual(level);

        //  4) 刷血条（保险）
        if (bossHpBar != null) bossHpBar.Initialize(boss.maxHp, boss.currentHp);

        // ✅ 5) 提示：关卡 + 回合
        UpdateHint();
    }

    private void UpdateHint(string extra = null)
    {
        if (phaseHintText == null) return;

        string lv = $"Level {(int)Level + 1}";
        string turn = $"Turn {turnIndex}";
        string ph =
            phase == PlayerPhase.Discard ? "DISCARD" :
            (drewThisTurn ? "Player Turn" : "Click DRAW");

        if (!string.IsNullOrEmpty(extra))
            phaseHintText.text = $"{lv} - {turn} - {extra}";
        else
            phaseHintText.text = $"{lv} - {turn} - {ph}";
    }

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
        drewThisTurn = false;
        phase = PlayerPhase.Play;

        if (drawBtn != null) drawBtn.interactable = true;
        if (endTurnBtn != null) endTurnBtn.interactable = false;

        EnterPlayPhase();
    }

    private void EnterPlayPhase()
    {
        phase = PlayerPhase.Play;
        UpdateHint();

        if (playedZone != null && !playedZone.gameObject.activeSelf)
            playedZone.gameObject.SetActive(true);

        RenderHand(CardManager.Instance.Hand, OnHandCardClicked_Play);
        RenderPlayed(CardManager.Instance.playedInThisTurn, OnPlayedCardClicked_Undo);
    }

    private void EnterDiscardPhase()
    {
        phase = PlayerPhase.Discard;

        int needDiscard = Mathf.Max(0, CardManager.Instance.Hand.Count - maxHandSize);
        UpdateHint($"DISCARD {needDiscard}");

        RenderHand(CardManager.Instance.Hand, OnHandCardClicked_Discard);
        RenderPlayed(CardManager.Instance.playedInThisTurn, null);

        if (endTurnBtn != null) endTurnBtn.interactable = true;
        if (drawBtn != null) drawBtn.interactable = false;
    }

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

    private bool HasAttackInPlayedThisTurn()
    {
        List<CardInstance> played = CardManager.Instance.playedInThisTurn;
        for (int i = 0; i < played.Count; i++)
            if (played[i].cardTemplate.cardType == CardType.Attack) return true;
        return false;
    }

    private void OnHandCardClicked_Play(CardInstance card)
    {
        if (!drewThisTurn)
        {
            UpdateHint("Draw first!");
            return;
        }
        if (card == null) return;

        CardType type = card.cardTemplate.cardType;

        if (turnIndex == 1 && type == CardType.Recall)
        {
            UpdateHint("Recall can't be used in Turn 1");
            return;
        }

        if (type == CardType.Attack && HasAttackInPlayedThisTurn())
        {
            UpdateHint("Only 1 Attack per turn");
            return;
        }

        if (CardManager.Instance.TryPlayToTable(card))
            EnterPlayPhase();
    }

    private void OnPlayedCardClicked_Undo(CardInstance card)
    {
        if (CardManager.Instance.TryUnplayToHand(card))
            EnterPlayPhase();
    }

    private void OnHandCardClicked_Discard(CardInstance card)
    {
        if (!CardManager.Instance.DiscardFromHand(card)) return;

        if (CardManager.Instance.Hand.Count > maxHandSize)
        {
            EnterDiscardPhase();
            return;
        }

        if (pendingNextTurnDiscard)
        {
            pendingNextTurnDiscard = false;
            turnIndex++;
            StartPlayerTurn();
            return;
        }

        StartCoroutine(ResolveEndTurnRoutine());
    }

    private void OnDrawClicked()
    {
        if (drewThisTurn) return;

        CardManager.Instance.DrawCardsToHand(drawEachTurn);
        drewThisTurn = true;

        if (drawBtn != null) drawBtn.interactable = false;
        if (endTurnBtn != null) endTurnBtn.interactable = true;

        EnterPlayPhase();
    }

    private void OnEndTurnClicked()
    {
        if (phase == PlayerPhase.Discard)
        {
            if (CardManager.Instance.Hand.Count > maxHandSize)
            {
                EnterDiscardPhase();
                return;
            }

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

        if (CardManager.Instance.Hand.Count > maxHandSize)
        {
            EnterDiscardPhase();
            return;
        }

        StartCoroutine(ResolveEndTurnRoutine());
    }

    private IEnumerator ResolveEndTurnRoutine()
    {
        if (endTurnBtn != null) endTurnBtn.interactable = false;
        if (drawBtn != null) drawBtn.interactable = false;

        RenderHand(CardManager.Instance.Hand, null);
        RenderPlayed(CardManager.Instance.playedInThisTurn, null);

        UpdateHint("Resolving...");

        // 0) 扣体温/能量
        player.ConsumeEnergy(energyCostPerTurn);
        if (playerEnergyBar != null) playerEnergyBar.UpdateValue(player.currentEnergy);
        if (player.currentEnergy <= 0)
        {
            Lose("Energy depleted");
            yield break;
        }

        // 1) 出列区先隐藏
        if (playedZone != null) playedZone.gameObject.SetActive(false);
        if (playedVanishDelay > 0f) yield return new WaitForSeconds(playedVanishDelay);

        // 2) 记录本回合最后一张出牌
        List<CardInstance> played = CardManager.Instance.playedInThisTurn;
        CardInstance lastPlayedThisTurn = (played != null && played.Count > 0) ? played[played.Count - 1] : null;

        // 3) 玩家结算
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
                float hpRatio = (float)player.currentHP / player.maxHP;
                float enRatio = (float)player.currentEnergy / player.maxEnergy;
                if (hpRatio <= enRatio) player.Heal(12);
                else player.RecoverEnergy(8);
            }
            else if (type == CardType.Recall)
            {
                // 你现在的设计：本回合出几张 Recall 都只触发一次
                if (!recallTriggered)
                {
                    recallTriggered = true;
                    if (lastPlayedPrevTurn != null)
                        CardManager.Instance.TryRecallSpecificToHand(lastPlayedPrevTurn);
                }
            }
        }

        if (bossHpBar != null) bossHpBar.UpdateValue(boss.currentHp);
        if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);
        if (playerEnergyBar != null) playerEnergyBar.UpdateValue(player.currentEnergy);

        if (afterPlayerResolveDelay > 0f)
        {
            UpdateHint("Player effects resolved");
            yield return new WaitForSeconds(afterPlayerResolveDelay);
        }

        // 4) 判胜 → 处理换关
        if (boss.IsDead())
        {
            yield return StartCoroutine(HandleBossDefeatedRoutine());
            yield break;
        }

        // 5) Boss 行动
        UpdateHint("Boss Turn...");
        if (bossActDelay > 0f) yield return new WaitForSeconds(bossActDelay);

        int skill = boss.GetRandomSkill();
        if (skill == 0) yield return StartCoroutine(BossAttackRoutine());
        else yield return StartCoroutine(BossStealRoutine());

        if (afterBossResolveDelay > 0f)
        {
            UpdateHint("Boss action resolved");
            yield return new WaitForSeconds(afterBossResolveDelay);
        }

        // 6) 判负
        if (!player.isAlive || player.currentEnergy <= 0)
        {
            Lose("HP/Energy depleted");
            yield break;
        }

        // 7) 更新 Recall 记录
        lastPlayedPrevTurn = lastPlayedThisTurn;

        // 8) 回库
        CardManager.Instance.ReturnPlayedToBaseDeck();

        // 9) 结算后弃牌
        if (CardManager.Instance.Hand.Count > maxHandSize)
        {
            pendingNextTurnDiscard = true;
            EnterDiscardPhase();
            yield break;
        }

        // 10) 下一回合
        turnIndex++;
        StartPlayerTurn();
    }

    private IEnumerator BossAttackRoutine()
    {
        UpdateHint("Boss attacks!");

        if (scratchFx != null) scratchFx.SetActive(true);
        if (scratchShowTime > 0f) yield return new WaitForSeconds(scratchShowTime);
        if (scratchFx != null) scratchFx.SetActive(false);

        int damage = boss.GetAttackDamage();

        if (shieldCharges > 0)
        {
            shieldCharges--;
            damage = 0;

            if (Talent == TalentType.Med)
            {
                player.Heal(5);
                if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);
            }

            UpdateHint("Submerge blocked the attack!");
        }

        player.TakeDamage(damage);
        if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);

        yield return null;
    }

    private IEnumerator BossStealRoutine()
    {
        // ✅ 手牌空了就不要偷了，直接改成攻击（你刚说的）
        if (CardManager.Instance.Hand.Count <= 0)
        {
            yield return StartCoroutine(BossAttackRoutine());
            yield break;
        }

        UpdateHint("Boss steals a card!");

        if (shieldCharges > 0)
        {
            shieldCharges--;

            if (Talent == TalentType.Med)
            {
                player.Heal(5);
                if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);
            }

            UpdateHint("Submerge blocked the steal!");
            if (stealShowTime > 0f) yield return new WaitForSeconds(stealShowTime);
            yield break;
        }

        int idx = Random.Range(0, CardManager.Instance.Hand.Count);
        CardInstance stolen = CardManager.Instance.Hand[idx];
        CardManager.Instance.Hand.RemoveAt(idx);
        CardManager.Instance.ReturnCardToBaseDeck(stolen);

        RenderHand(CardManager.Instance.Hand, null);

        if (stealShowTime > 0f) yield return new WaitForSeconds(stealShowTime);
    }

    private IEnumerator HandleBossDefeatedRoutine()
    {
        LevelType lv = ProgressManager.Instance.Data.currentLevel;

        // 最后一关才真 WIN
        if (lv == LevelType.Level3)
        {
            Win();
            yield break;
        }

        UpdateHint("STAGE CLEAR!");
        yield return new WaitForSeconds(1.0f);

        // 关卡结束恢复（按你策划：san+10 temp+30；你 san=hp temp=energy）
        player.Heal(10);
        player.RecoverEnergy(30);
        if (playerHpBar != null) playerHpBar.UpdateValue(player.currentHP);
        if (playerEnergyBar != null) playerEnergyBar.UpdateValue(player.currentEnergy);

        // 下一关
        LevelType next = NextLevel(lv);
        ProgressManager.Instance.Data.currentLevel = next;

        // 出列回库
        CardManager.Instance.ReturnPlayedToBaseDeck();

        // 清理本关的临时状态，防止带到下一关
        pendingNextTurnDiscard = false;
        lastPlayedPrevTurn = null;
        drewThisTurn = false;
        shieldCharges = 0;

        // 清空出列区显示（你现在可能只回库但UI没彻底清）
        if (playedZone != null)
        {
            playedZone.gameObject.SetActive(true);
            for (int i = playedZone.childCount - 1; i >= 0; i--)
                Destroy(playedZone.GetChild(i).gameObject);
        }

        // 换 Boss（数据 + 视觉）
        SetupBossForLevel(next);

        // 保存 checkpoint（新关开局）
        ProgressManager.Instance.SaveCheckpoint(next, player, CardManager.Instance);

        // 新关从 Turn1 开始
        turnIndex = 1;

        // 切关后强制刷新弃牌阶段判断
        if (CardManager.Instance.Hand.Count > maxHandSize)
        {
            pendingNextTurnDiscard = true;
            EnterDiscardPhase();
            yield break; // 让玩家先弃牌再开始下一回合
        }

        StartPlayerTurn();
    }


    private void PlayFinalVictory()
    {
        BasePanel p = UIManager.Instance.ShowPanel("FinalPanel");
        FinalPanel fp = p as FinalPanel;
        if (fp != null) fp.PlayVictory();
        else Debug.LogError("ShowPanel('FinalPanel') 返回的不是 FinalPanel，请检查预制体上挂的脚本类型。");
    }

    private void PlayFinalFail()
    {
        BasePanel p = UIManager.Instance.ShowPanel("FinalPanel");
        FinalPanel fp = p as FinalPanel;
        if (fp != null) fp.PlayFail();
        else Debug.LogError("ShowPanel('FinalPanel') 返回的不是 FinalPanel，请检查预制体上挂的脚本类型。");
    }

    private void Win()
    {
        if (endTurnBtn != null) endTurnBtn.interactable = false;
        if (drawBtn != null) drawBtn.interactable = false;

        //  第三关胜利：播胜利动画面板
        if (ProgressManager.Instance.Data.currentLevel == LevelType.Level3)
        {
            PlayFinalVictory();
            return;
        }

        // 否则：走你“切下一关”的逻辑（HandleBossDefeatedRoutine 里应该会处理）
        if (phaseHintText != null) phaseHintText.text = "WIN!";
    }

    private void Lose(string reason)
    {
        Debug.Log("LOSE: " + reason);

        // 先锁按钮，防止还在操作
        if (endTurnBtn != null) endTurnBtn.interactable = false;
        if (drawBtn != null) drawBtn.interactable = false;

        //  Level3：失败直接播结尾失败动画（不回档）
        if (Level == LevelType.Level3)
        {
            PlayFinalFail();
            return;
        }

        //  Level2：回到 Level2 开局 checkpoint
        if (Level == LevelType.Level2)
        {
            bool ok = ProgressManager.Instance.TryLoadCheckpoint(LevelType.Level2, player, CardManager.Instance);
            if (ok)
            {
                shieldCharges = 0;
                lastPlayedPrevTurn = null;
                pendingNextTurnDiscard = false;
                drewThisTurn = false;

                SetupBossForLevel(LevelType.Level2);

                if (playerHpBar != null) playerHpBar.Initialize(player.maxHP, player.currentHP);
                if (playerEnergyBar != null) playerEnergyBar.Initialize(player.maxEnergy, player.currentEnergy);
                if (bossHpBar != null) bossHpBar.Initialize(boss.maxHp, boss.currentHp);

                turnIndex = 1;
                StartPlayerTurn();
                return;
            }
        }

        //  其他关：先直接结束（你要不要加“回到开始界面”都行，之后再说）
        if (phaseHintText != null) phaseHintText.text = "LOSE!";
    }


    private LevelType NextLevel(LevelType lv)
    {
        if (lv == LevelType.Level1) return LevelType.Level2;
        if (lv == LevelType.Level2) return LevelType.Level3;
        return LevelType.Level3;
    }

    private int GetBossMaxHp(LevelType lv)
    {
        if (lv == LevelType.Level1) return 100;
        if (lv == LevelType.Level2) return 140;
        return 180;
    }

    private void ApplyBossVisual(LevelType lv)
    {
        if (bossLv1Root != null) bossLv1Root.SetActive(lv == LevelType.Level1);
        if (bossLv2Root != null) bossLv2Root.SetActive(lv == LevelType.Level2);
        if (bossLv3Root != null) bossLv3Root.SetActive(lv == LevelType.Level3);
    }
}
