using UnityEngine;

public class BossData
{
    // 用枚举表示关卡/阶段
    public LevelType stage;

    public int maxHp = 100;
    public int currentHp = 100;

    public BossData(LevelType stage)
    {
        this.stage = stage;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;
    }

    public bool IsDead()
    {
        return currentHp <= 0;
    }

    // 0 = 攻击玩家, 1 = 夺取卡牌
    public int GetRandomSkill()
    {
        return Random.Range(0, 2); // 只会是 0 或 1
    }

    public int GetAttackDamage()
    {
        switch (stage)
        {
            case LevelType.Level1: return 10;
            case LevelType.Level2: return 20;
            case LevelType.Level3: return 30;
            default: return 10;
        }
    }
}
