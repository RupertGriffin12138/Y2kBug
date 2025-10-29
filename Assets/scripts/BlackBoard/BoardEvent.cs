using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardEvent : MonoBehaviour
{
    public void PlayAudio()
    {
        AudioClipHelper.Instance.Play_WipeTheBlackboard();
    }
    public void StopAudio()
    {
        AudioClipHelper.Instance.Stop_WipeTheBlackboard();
    }
}
