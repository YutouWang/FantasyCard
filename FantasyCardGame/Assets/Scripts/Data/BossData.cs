using UnityEngine;

public class BossData
{
    public LevelType stage = LevelType.Level1; // 关卡 1 / 2 / 3
    public int maxHp = 100;
    public int currentHp = 100;

    //构造函数初始化关卡
    public BossData(LevelType stage)
    {
        this.stage = stage;
    }

    //获取不同关卡的攻击力
    public int GetAttackDamage()
    {
        switch (stage)
        {
            case LevelType.Level1:
                return 10;
            case LevelType.Level2:
                return 20;
            case LevelType.Level3:
                return 30;
            default:
                return 10;
        }

    }

    
    /// <summary>
    /// 返回 Boss 本回合技能,0 = 攻击玩家,1 = 夺取卡牌
    /// </summary>
    /// <returns></returns>
    public int GetRandomSkill()
    {
        return UnityEngine.Random.Range(0, 2);
    }

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

    
}
