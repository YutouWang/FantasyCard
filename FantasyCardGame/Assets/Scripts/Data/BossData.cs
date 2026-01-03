using UnityEngine;

public class BossData
{
    public LevelType levelType = LevelType.Level1; // 关卡 1 / 2 / 3
    public int maxHp = 100;
    public int currentHp = 100;

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp < 0)
            currentHp = 0;
    }

    public bool IsDead()
    {
        return currentHp <= 0;
    }

    public BossData(int stage)
    {
        this.stage = stage;
    }

    // 返回 Boss 本回合技能
    // 0 = 攻击玩家
    // 1 = 夺取卡牌
    public int GetRandomSkill()
    {
        return Random.Range(0, 2);
    }

    public int GetAttackDamage()
    {
        if (stage == 1) return 10;
        if (stage == 2) return 20;
        if (stage == 3) return 30;
        return 10;
    }
}
