using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    // ======================
    // 基础属性
    // ======================

    public int maxHP = 100;
    public int currentHP = 100;

    public int maxEnergy = 100;
    public int currentEnergy = 100;

    // ======================
    // 战斗状态
    // ======================

    public bool isAlive => currentHP > 0;





    // 初始化 满血满能量
    public void Init()
    {
        currentHP = maxHP;
        currentEnergy = maxEnergy;
    }

    // ======================
    // 生命值相关
    // ======================

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        currentHP -= damage;
        if (currentHP < 0)
            currentHP = 0;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    // ======================
    // 能量相关
    // ======================

    public bool HasEnoughEnergy(int cost)
    {
        return currentEnergy >= cost;
    }

    public bool ConsumeEnergy(int cost)
    {
        if (cost <= 0) return true;

        if (currentEnergy < cost)
            return false;

        currentEnergy -= cost;
        return true;
    }

    public void RecoverEnergy(int amount)
    {
        if (amount <= 0) return;

        currentEnergy += amount;
        if (currentEnergy > maxEnergy)
            currentEnergy = maxEnergy;
    }

    public void RecoverEnergyToFull()
    {
        currentEnergy = maxEnergy;
    }
}
