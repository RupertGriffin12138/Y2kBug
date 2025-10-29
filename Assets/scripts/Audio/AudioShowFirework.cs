using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioShowFirework : MonoBehaviour
{
    public SpriteRenderer sprite;
    private bool wasAboveThreshold = false; // ��¼��һ֡��͸����״̬

    private void OnEnable()
    {
        // ��ʼ״̬���
        CheckAlphaAndControlAudio();
    }

    private void Update()
    {
        // ÿ֡���͸���Ȳ�������Ч
        CheckAlphaAndControlAudio();
    }

    private void CheckAlphaAndControlAudio()
    {
        if (sprite == null) return;

        // ��ȡ��ǰ�����͸���� [6](@ref)
        float currentAlpha = sprite.color.a;

        // ͸���ȴ���50%��֮ǰ�������״̬
        if (currentAlpha > 0.5f && !wasAboveThreshold)
        {
            AudioClipHelper.Instance.Play_Burning();
            wasAboveThreshold = true;
        }
        // ͸����С�ڵ���50%��֮ǰ�����״̬֮��
        else if (currentAlpha <= 0.5f && wasAboveThreshold)
        {
            AudioClipHelper.Instance.Stop_Burning();
            wasAboveThreshold = false;
        }
    }

    private void OnDisable()
    {
        // ȷ���������ʱֹͣ��Ч
        if (wasAboveThreshold)
        {
            AudioClipHelper.Instance.Stop_Burning();
            wasAboveThreshold = false;
        }
    }
}