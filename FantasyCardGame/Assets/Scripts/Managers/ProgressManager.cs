using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameProgressData 的静态实例封装（存储容器）
/// </summary>

public class ProgressManager
{
    public static ProgressManager Instance { get; } = new ProgressManager();
    private ProgressManager() { }

    public GameProgressData Data { get; } = new GameProgressData();

    // 每关一个 checkpoint（用于 Level2 输了回到 Level2 开局）
    private readonly Dictionary<LevelType, Checkpoint> _checkpoints = new Dictionary<LevelType, Checkpoint>();

    public void SaveCheckpoint(LevelType level, PlayerData player, CardManager cm)
    {
        _checkpoints[level] = new Checkpoint
        {
            hp = player.currentHP,
            energy = player.currentEnergy,
            deck = cm.GetBaseDeckSnapshot(),
            hand = cm.GetHandSnapshot()
        };
    }

    public bool TryLoadCheckpoint(LevelType level, PlayerData player, CardManager cm)
    {
        if (!_checkpoints.TryGetValue(level, out var cp)) return false;

        player.currentHP = cp.hp;
        player.currentEnergy = cp.energy;
        cm.SetDeckAndHand(cp.deck, cp.hand);
        return true;
    }

    private class Checkpoint
    {
        public int hp;
        public int energy;
        public List<CardInstance> deck;
        public List<CardInstance> hand;
    }
}
