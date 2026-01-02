using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class BattlePanel : MonoBehaviour
{

}*/



public class BattlePanel : BasePanel
{
    [Header("拖拽引用")]
    public Transform handRoot;          // 手牌容器 用于实例化的时候 提供生成所需的位置信息
    public GameObject cardPrefab;       //  从外部拖进来 CardPrefab

    [Header("Resources 路径")]
    public string cardDataInitPath = "Card/CardDataInit";

    public override void Init()
    {
        //因为上面这些动态加载的部分需要用到mono 所以写在battlepanel里面了
        // 1.从 Resources 加载 CardDataInit
        GameObject initPrefab = Resources.Load<GameObject>(cardDataInitPath);
        if (initPrefab == null)
        {
            Debug.LogError(" 没有在 Resources/" + cardDataInitPath + "找到 CardDataInit prefab");
            return;
        }

        //实例化的预制体
        GameObject initObj = Instantiate(initPrefab);
        if (initObj == null)
        {
            print("initObj(carddatainit实例化不出来)");
        }
        //得到实例上挂载的脚本
        CardDataInit initScript = initObj.GetComponent<CardDataInit>();

        if (initScript == null)
        {
            Debug.LogError("没有在CardDataInit预制体上找到CardDataInit脚本");
            return;
        }

        // 2.创建基础牌库
        List<CardInstance> deck = initScript.CreateBaseDeck();

        // 3. 生成 n 张牌 UI 并绑定数据

        #region 原始测试初始化5张牌逻辑
        //for (int i = 0; i < 5; i++)
        //{
        //    GameObject aCard = Instantiate(cardPrefab, handRoot, false);
        //    Debug.Log(aCard == null ? "aCard == NULL" : $"aCard OK: {aCard.name}");

        //    //得到CardUI预制体上的脚本
        //    CardUI cardUIScript = aCard.GetComponent<CardUI>();

        //    if (cardUIScript == null)
        //    {
        //        Debug.LogError("CardPrefab 上没有 CardUI脚本 ");
        //        return;
        //    }
        //    if (i == 0)
        //        cardUIScript.BindInstance(deck[0]);
        //    if (i == 1)
        //        cardUIScript.BindInstance(deck[6]);
        //    if (i == 2)
        //        cardUIScript.BindInstance(deck[13]);
        //    if (i == 3)
        //        cardUIScript.BindInstance(deck[17]);
        //    if (i == 4)
        //        cardUIScript.BindInstance(deck[8]);
        //}
        #endregion

        //把牌库创建好 存到manager里便于后续管理
        CardManager.Instance.Init(deck);

        CardManager.Instance.DrawCardsToHand(5);

        RenderHand(CardManager.Instance.Hand);

    }

    private void RenderHand(List<CardInstance> hand)
    {
        // 先清空
        for (int i = handRoot.childCount - 1; i >= 0; i--)
            Destroy(handRoot.GetChild(i).gameObject);

        // 再生成
        foreach (var inst in hand)
        {
            var go = Instantiate(cardPrefab, handRoot, false);
            go.GetComponent<CardUI>().BindInstance(inst);
        }
    }

}
