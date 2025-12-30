using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例模式
/// 管理卡牌的创建、牌库的初始化、抽牌等行为
/// 像 UIManager（统一管理所以面板的显隐） 一样统一管理卡片的行为
/// </summary>
/// 

//疑问：弃牌的方法也是在这里面提供的吧？
public class CardManager 
{
    public static CardManager instance = new CardManager();

    //属性简化写法
    public static CardManager Instance => instance;

    //构造方法 后续看看需不需要什么初始化的数据再填
    public CardManager()
    {

    } 

    //向外部提供的方法

    /// <summary>
    /// 抽卡方法
    /// </summary>
    /// <param name="count"> 需要传入抽取的数量 </param>
    /// <returns> 会返回抽取的卡片信息（序号还是实例待定）</returns>
    public IList<CardData> DrawCards(int count)
    {
        //抽取两张卡的逻辑
        //随机两个卡序号 实例化这两张卡片 存入List容器并且返回
        //(需要确定实例化卡片的动作是在每次抽卡的时候 还是单独写一个方法 抽卡的时候只提供随机的序号)
        return null;
    }
    
}
