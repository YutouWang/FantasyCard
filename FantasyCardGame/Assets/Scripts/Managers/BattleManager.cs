using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("角色数据")]
    public PlayerData playerData;
    public BossData bossData;

    [Header("UI StatBars")]
    public StatBar playerHPBar;      // 玩家血量条（Slider）
    public StatBar playerEnergyBar;  // 玩家能量条（Slider）
    public StatBar bossHPBar;        // Boss血量条（Slider）

    [Header("卡牌/手牌管理")]
    public HandManager handManager;
    public DeckManager deckManager;

    void Start()
    {
        // 初始化数据
        playerData = new PlayerData();
        playerData.Init();

        bossData = new BossData(stage: 1);

        // 初始化StatBar
        playerHPBar.Initialize(playerData.maxHP, playerData.currentHP);
        playerEnergyBar.Initialize(playerData.maxEnergy, playerData.currentEnergy);
        bossHPBar.Initialize(bossData.maxHp, bossData.currentHp);

        Debug.Log("战斗开始，Boss HP = " + bossData.currentHp);

        // 开始玩家回合
        StartPlayerTurn();
    }

    // ------------------------------
    // 玩家回合开始
    // ------------------------------
    public void StartPlayerTurn()
    {
        // 每回合消耗10点能量
        playerData.ConsumeEnergy(10);
        playerEnergyBar.UpdateValue(playerData.currentEnergy);

        Debug.Log("玩家回合开始，消耗10点能量，剩余能量：" + playerData.currentEnergy);

        // 这里可以显示玩家手牌UI，等待玩家操作
    }

    // ------------------------------
    // 玩家出牌攻击Boss
    // ------------------------------
    public void PlayerAttackBoss(int damage)
    {
        // Boss受到伤害
        bossData.TakeDamage(damage);
        bossHPBar.UpdateValue(bossData.currentHp);

        Debug.Log($"玩家出牌，Boss受到 {damage} 伤害，Boss当前 HP：{bossData.currentHp}");
    }

    // ------------------------------
    // 玩家使用回血或能量回复卡牌
    // ------------------------------
    public void PlayerHeal(int amount)
    {
        playerData.Heal(amount);
        playerHPBar.UpdateValue(playerData.currentHP);

        Debug.Log($"玩家回血 {amount}，当前 HP：{playerData.currentHP}");
    }

    public void PlayerRecoverEnergy(int amount)
    {
        playerData.RecoverEnergy(amount);
        playerEnergyBar.UpdateValue(playerData.currentEnergy);

        Debug.Log($"玩家恢复能量 {amount}，当前能量 {playerData.currentEnergy}");
    }

    // ------------------------------
    // 玩家回合结束 → Boss行动
    // ------------------------------
    public void EndPlayerTurn()
    {
        Debug.Log("玩家回合结束，Boss行动");
        BossTurn();
    }

    // ------------------------------
    // Boss回合逻辑
    // ------------------------------
    void BossTurn()
    {
        int skill = bossData.GetRandomSkill();

        if (skill == 0)
        {
            BossAttack();
        }
        else
        {
            BossStealCard();
        }

        // Boss回合结束后，如果玩家受到伤害，StatBar已经更新在 BossAttack 里
        // 可以在这里继续结算回合结束逻辑，例如刷新手牌等
    }

    void BossAttack()
    {
        int damage = bossData.GetAttackDamage();
        playerData.TakeDamage(damage);

        // 更新玩家血量条
        playerHPBar.UpdateValue(playerData.currentHP);

        Debug.Log($"Boss攻击玩家，造成 {damage} 伤害，玩家当前 HP：{playerData.currentHP}");
    }

    void BossStealCard()
    {
        Debug.Log("Boss 使用夺牌技能");

        if (handManager != null && deckManager != null)
        {
            // 以后补真正逻辑
            Debug.Log("（这里将来写夺牌逻辑）");
        }
    }
}
