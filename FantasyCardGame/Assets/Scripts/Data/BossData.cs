using UnityEngine;

public class BossData
{
    public LevelType levelType;   // Level1 / Level2 / Level3
    public int maxHp;
    public int currentHp;

    public BossData(LevelType levelType)
    {
        this.levelType = levelType;

        maxHp = GetMaxHpByLevel(levelType);
        currentHp = maxHp;
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

    // 0 = ¹¥»÷Íæ¼Ò£¬1 = ¶áÅÆ
    public int GetRandomSkill()
    {
        return Random.Range(0, 2);
    }

    public int GetAttackDamage()
    {
        switch (levelType)
        {
            case LevelType.Level1: return 10;
            case LevelType.Level2: return 20;
            case LevelType.Level3: return 30;
            default: return 10;
        }
    }

    private int GetMaxHpByLevel(LevelType lvl)
    {
        switch (lvl)
        {
            case LevelType.Level1: return 80;
            case LevelType.Level2: return 120;
            case LevelType.Level3: return 160;
            default: return 100;
        }
    }
}
