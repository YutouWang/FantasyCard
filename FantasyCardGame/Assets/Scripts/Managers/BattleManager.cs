using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public PlayerData playerData;
    public BossData bossData;

    public HandManager handManager;
    public DeckManager deckManager;

    void Start()
    {
        // 初始化数据
        playerData = new PlayerData();
        bossData = new BossData(stage: 1);

        Debug.Log("战斗开始，Boss HP = " + bossData.currentHp);
    }

    // 玩家回合结束时调用
    public void EndPlayerTurn()
    {
        Debug.Log("玩家回合结束，Boss 行动");
        BossTurn();
    }

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
    }

    void BossAttack()
    {
        int damage = bossData.GetAttackDamage();
        playerData.TakeDamage(damage);

        Debug.Log($"Boss 攻击玩家，造成 {damage} 伤害");
        Debug.Log($"玩家当前 HP：{playerData.currentHP}");
    }

    void BossStealCard()
    {
        Debug.Log("Boss 使用夺牌技能");

        if (handManager != null && deckManager != null)
        {
            // 以后你再补真正逻辑
            Debug.Log("（这里将来写夺牌逻辑）");
        }
    }
}
