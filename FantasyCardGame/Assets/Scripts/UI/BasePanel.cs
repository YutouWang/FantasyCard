using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class BasePanel : MonoBehaviour
{
    //专门用于控制面板的透明度的组件
    private CanvasGroup canvasGroup;
    //控制面板淡入淡出的速度
    private float alphaSpeed = 10;
    //当前是隐藏还是显示状态
    public bool isShow = false;

    //当隐藏完毕后需要执行的逻辑
    private UnityAction hideCallBack = null;
    protected virtual void Awake()
    {

        //所有面板上都需要手动添加 CanvasGroup 组件
        canvasGroup = this.GetComponent<CanvasGroup>();

        //如果忘了 防止报错会自动加一次
        if (canvasGroup == null)
        {
            canvasGroup = this.AddComponent<CanvasGroup>();
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Init();
    }

    /// <summary>
    /// 注册控件事件的方法 所有的子面板都要 注册控件事件 
    /// abstract 必须重写
    /// </summary>
    public abstract void Init();

    //显示本面板的时候做的逻辑
    public virtual void ShowMe()
    {
        Debug.Log($"{name}ShowMe called");
        //淡入
        isShow = true;
        //开始让面板的透明度为0 在Update中逐渐增加alpha值 达到淡入的效果 淡出同理
        canvasGroup.alpha = 0;

    }

    //隐藏本面板的时候做的逻辑
    public virtual void HideMe(UnityAction callBack)
    {
        //淡出
        isShow = false;
        canvasGroup.alpha = 1;

        hideCallBack = callBack;
    }
    // Update is called once per frame
    void Update()
    {
        if(Time.frameCount%60 == 0)
        {
            Debug.Log($"{name} Update running, isShow={isShow}, alpha={canvasGroup.alpha}");
        }

        //淡入：要显示自己 开始增加透明度
        if (isShow && canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += alphaSpeed * Time.deltaTime;

            //透明度加到1就停止增加透明度
            if (canvasGroup.alpha > 1)
                canvasGroup.alpha = 1;
        }
        //淡出
        else if (!isShow && canvasGroup.alpha != 0)
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;

            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;

                //淡出结束 面板隐藏 执行面板结束后的逻辑
                hideCallBack.Invoke();
            }

        }


    }
}
