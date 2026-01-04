using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//关卡枚举 确保只有三关
public enum LevelType
{
    Level1,
    Level2,
    Level3
}

//天赋枚举 一共三种
public enum TalentType
{
    Scarf,
    Med,
    Bear
}
/// <summary>
/// 存储游戏全局数据 需要继承到下一关的数据 
/// 例如玩家san值 玩家体温值 关卡进度，天赋类型
/// </summary>
public class GameProgressData
{
    //默认一开始是关卡1
    public LevelType currentLevel = LevelType.Level1;

    //这里的数值是每关结束后继承的san 和 temperature
    public float playerSan = 100;
    public float playerTemp = 100;

    //天赋默认选择围巾吧
    public TalentType selectedTalent = TalentType.Scarf;

    //手牌上限
    public int maxHandSize = 5;

    //玩家的手牌（当前拥有的牌情况,下一关会继承本关卡的手牌）
    public List<CardInstance> playerHandCards = new List<CardInstance>();

    //大牌库 baseDeck
    public List<CardInstance> baseDeck = new List<CardInstance>();


}
