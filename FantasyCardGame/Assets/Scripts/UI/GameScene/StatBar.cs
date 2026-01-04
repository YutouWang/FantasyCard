using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider slider;       // 拖入对应的Slider（玩家HP、玩家能量、Boss HP）

    [Header("Settings")]
    public float smoothSpeed = 5f; // 血量/能量变化动画速度

    private float displayedValue;
    private int maxValue;
    private int currentValue;

    // 初始化数值 血条最大值 当前值
    public void Initialize(int max, int current)
    {
        maxValue = max;
        currentValue = current;
        displayedValue = current;

        if (slider != null)
        {
            slider.maxValue = maxValue;
            slider.value = displayedValue;
        }
            

        slider.value = displayedValue;
    }

    // 更新数值（其他脚本调用）
    public void UpdateValue(int newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0, maxValue);
    }

    void Update()
    {
        //插值处理血条变化情况
        if (displayedValue != currentValue)
        {
            displayedValue = Mathf.Lerp(displayedValue, currentValue, Time.deltaTime * smoothSpeed);
            slider.value = displayedValue;
        }
    }
}
