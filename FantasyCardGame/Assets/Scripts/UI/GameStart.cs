using UnityEngine;

public class GameStart : MonoBehaviour
{
    void Start()
    {
        // 通过 UIManager 显示 BeginPanel（从 Resources/UI 中加载）
        UIManager.Instance.ShowPanel("BeginPanel");
        Debug.Log("BeginPanel requested");
    }
}
