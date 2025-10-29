using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioShowFirework : MonoBehaviour
{
    public SpriteRenderer sprite;
    private bool wasAboveThreshold = false; // 记录上一帧的透明度状态

    private void OnEnable()
    {
        // 初始状态检查
        CheckAlphaAndControlAudio();
    }

    private void Update()
    {
        // 每帧检查透明度并控制音效
        CheckAlphaAndControlAudio();
    }

    private void CheckAlphaAndControlAudio()
    {
        if (sprite == null) return;

        // 获取当前精灵的透明度 [6](@ref)
        float currentAlpha = sprite.color.a;

        // 透明度大于50%且之前不在这个状态
        if (currentAlpha > 0.5f && !wasAboveThreshold)
        {
            AudioClipHelper.Instance.Play_Burning();
            wasAboveThreshold = true;
        }
        // 透明度小于等于50%且之前在这个状态之上
        else if (currentAlpha <= 0.5f && wasAboveThreshold)
        {
            AudioClipHelper.Instance.Stop_Burning();
            wasAboveThreshold = false;
        }
    }

    private void OnDisable()
    {
        // 确保对象禁用时停止音效
        if (wasAboveThreshold)
        {
            AudioClipHelper.Instance.Stop_Burning();
            wasAboveThreshold = false;
        }
    }
}