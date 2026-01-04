using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //card整体操作的button
    [SerializeField] private Button cardBtn;

    //card的名字
    [SerializeField] private TMP_Text cardName;

    //提示的背景（用来控制整个提示框的显示和隐藏）
    [SerializeField] private Image descBK;
    //提示的文字描述 根据卡牌类型来适配赋值
    [SerializeField] private TMP_Text descText;

    //用一个CardInstance来提供卡牌数据
    private CardInstance cardInstance;

    private void Awake()
    {
        //默认提示文本不显示
        if (descBK != null)
            descBK.gameObject.SetActive(false);

        //说明框不档鼠标事件
        DisableRaycastOnDesc();
    }

    //主要是设置 raycast 的
    private void DisableRaycastOnDesc()
    {
        if (descBK == null) return;

        // 背景 Image
        var bg = descBK.GetComponent<Image>();
        if (bg != null) bg.raycastTarget = false;

        // 文字 TMP
        if (descText != null) descText.raycastTarget = false;
    }

    //一创建cardinstance实例就调用bind函数 进行UI层面的初始化
    //把点击的回调时间也传进来 绑定完了就知道点击按钮会做什么事（OnCardClicked）
    public void BindInstance(CardInstance instance,UnityAction<CardInstance> OnCardClicked)
    {
        //传进来的实例信息进行保存 后面逻辑判断会用得到
        cardInstance = instance;

        if (cardBtn!=null && cardBtn.image != null)
        {
            //直接从button身上拿sprite 因为image是button的一个组件
            cardBtn.image.sprite = cardInstance.cardTemplate.cardSprite;
        }

        if (cardName != null)
        {
            cardName.text = cardInstance.cardTemplate.cardName;
        }

        if (descText != null)
        {
            descText.text = cardInstance.cardTemplate.cardDescription;
        }

        //再关一次是为了保险
        if (descBK != null)
        {
            descBK.gameObject.SetActive(false);
        }

        cardBtn.onClick.AddListener(() =>
        {
            //这里不知道为啥不能写成判空处理(更保险) OnCardClicked?.Invoke(cardInstance);
            OnCardClicked.Invoke(cardInstance);
        });

        Debug.Log($"Bind: {instance.cardTemplate.cardName} type={instance.cardTemplate.cardType} sprite={instance.cardTemplate.cardSprite?.name} desc={instance.cardTemplate.cardDescription}");

    }

    //鼠标悬停在此处时执行的逻辑  这是悬停检测函数（类似于碰撞检测函数那种）
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (descBK != null)
        {
            descBK.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (descBK != null)
        {
            descBK.gameObject.SetActive(false);
        }
    }


}
