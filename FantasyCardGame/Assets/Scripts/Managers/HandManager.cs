using UnityEngine;

public class HandManager : MonoBehaviour
{
    private bool endTurnRequested = false;

    public bool CanEndTurn()
    {
        // 现在先永远允许（后面加手牌上限）
        return endTurnRequested;
    }

    public void MarkEndTurn()
    {
        Debug.Log("玩家点击了【结束回合】");
        endTurnRequested = true;
    }

    public void ResetTurnState()
    {
        endTurnRequested = false;
    }
}
