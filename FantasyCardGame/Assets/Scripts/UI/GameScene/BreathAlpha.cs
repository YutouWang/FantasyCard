using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathAlpha : MonoBehaviour
{
    [SerializeField] CanvasGroup cg;
    [SerializeField] float minAlpha = 0.35f;
    [SerializeField] float maxAlpha = 1f;
    [SerializeField] float speed = 1.2f; // 越大呼吸越快

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if(cg == null )
        {
            cg = GetComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (!cg) return;
        float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f; // 0..1
        cg.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
    }
}
