using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//UI面板管理器，管理所有的面板的显示和隐藏，给外部提供获取这个面板的方法
//管理器是单例模式 不依附对象 所以不继承 Mono
public class UIManager 
{
    private static UIManager instance = new UIManager();

    //属性 供外部获取唯一的单例
    public static UIManager Instance => (instance);

    //用于存储显示过的面板 每显示一个面板 就存储进来 后续取出去或者隐藏的时候可以快速找到 更方便
    //string 存面板的名字  BasePanel 里氏替换父类装子类 装载面板的内容
    private Dictionary<string,BasePanel> panelDic = new Dictionary<string,BasePanel>();

    //场景中最开始有的一个公共的Canvas  后续显示的东西在这上面
    private Transform canvasTransform;

    //构造函数
    private UIManager()
    {
        //将Cnavas设置成预设体  确保场景中自始至终都只有一个Canvas
        GameObject canvas = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
        canvasTransform = canvas.transform;

        //过场景的时候不移除Canvas
        GameObject.DontDestroyOnLoad(canvas);
    }
    //显示面板
    public BasePanel ShowPanel(string panelName)
    {
        //判断字典中是否显示过这个面板 有的话直接返回
        if(panelDic.ContainsKey(panelName))
                return panelDic[panelName];

        //如果panelDic没有面板的话 动态创建面板预设体实例
        GameObject panel_Prefab = GameObject.Instantiate(Resources.Load<GameObject>("UI/"+panelName));

        //将要显示的面板的父对象设置为Canvas 相当于将面板放到了Canvas下面
        panel_Prefab.transform.SetParent(canvasTransform, false);

        //执行显示逻辑 并将动态创建的面板挂载的脚本存储到 panelDic 以便下一次取用
        //GetComponent 支持通过父类查找子类
        BasePanel nowPanel = panel_Prefab.GetComponent<BasePanel>();
        panelDic.Add(panelName,nowPanel);

        nowPanel.ShowMe();

        return nowPanel;

    }
    //隐藏面板
    public  void HiddenPanel(string panelName)
    {
        //先淡出 再执行删除面板逻辑 这里的删除面板逻辑作为匿名函数传进 HideMe 的委托
        panelDic[panelName].HideMe(() => 
        {
            if (panelDic.ContainsKey(panelName))
            {
                //删除脚本依附的对象
                GameObject.Destroy(panelDic[panelName].gameObject);

                //删除脚本
                panelDic.Remove(panelName);
            }
        });
        
    } 

    //得到面板
    public BasePanel GetPanel(string panelName)
    {
        if(panelDic.ContainsKey(panelName))
            return panelDic[panelName];
        else
            return null;
    }


}
