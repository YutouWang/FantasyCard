using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    //card整体操作的button
    [SerializeField] private Button cardBtn;
    [SerializeField] private Image cardSprite;
    //card的名字
    [SerializeField] private TMP_Text  cardName;

    //提示的背景（用来控制整个提示框的显示和隐藏）
    [SerializeField] private Image describeBK;
    //提示的文字描述 根据卡牌类型来适配赋值
    [SerializeField] private TMP_Text description;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
