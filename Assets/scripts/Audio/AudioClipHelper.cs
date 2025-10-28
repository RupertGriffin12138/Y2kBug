using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public class AudioClipHelper : MonoSingleton<AudioClipHelper>
{
    [SerializeField] private AudioClip _UIHover = null;
    [SerializeField] private AudioClip _UIClick = null;
    [SerializeField] private AudioClip _Dialogue = null;
    [SerializeField] private AudioClip _Burning = null;
    [SerializeField] private AudioClip _PickUpItems = null;
    [SerializeField] private AudioClip _PickUpPaper = null;
    [SerializeField] private AudioClip _ClassroomLocked = null;
    [SerializeField] private AudioClip _WoodenDoorLocked = null;
    [SerializeField] private AudioClip _SchoolGateLocked = null;
    [SerializeField] private AudioClip _Footsteps = null;
    [SerializeField] private AudioClip _ManWhisper = null;
    [SerializeField] private AudioClip _WoodenDoorUnlock = null;
    [SerializeField] private AudioClip _SchoolGateUnlock = null;
    [SerializeField] private AudioClip _WipeTheBlackboard = null;
    [SerializeField] private AudioClip _ChalkWriting = null;
    [SerializeField] private AudioClip _SuanPan = null;
    [SerializeField] private AudioClip _WoodenStructure = null;
    [SerializeField] private AudioClip _SecondHand = null;
    [SerializeField] private AudioClip _IronCabinet = null;



    public void Play_UIHover()
    {
        //Debug.Log("PlayMouseClick");
        if (_UIHover!=null)
        {
            if (AudioManager.Instance!=null)
            AudioManager.Instance.PlaySoundEffect(_UIHover);
            else
            {
                Debug.LogWarning("AudioManager Instance is not available.");
            }
        }
    }

    public void Play_UIClick()
    {
        //Debug.Log("PlayMouseClick");
        if (_UIHover != null)
        {
            AudioManager.Instance.PlaySoundEffect(_UIClick);
        }
        else
        {
            Debug.Log("_UIClick is null");
        }
    }
    public void Play_Dialogue()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_Dialogue);
    }
    public void Play_Burning()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_Burning);
    }
    public void Play_PickUpItems()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_PickUpItems);
    }
    public void Play_PickUpPaper()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_PickUpPaper);
    }
    public void Play_ClassroomLocked()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_ClassroomLocked);
    }
    public void Play_WoodenDoorLocked()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_WoodenDoorLocked);
    }
    public void Play_SchoolGateLocked()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_SchoolGateLocked);
    }
    public void Play_Footsteps()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_Footsteps);
    }
    public void Play_ManWhisper()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_ManWhisper);
    }
    public void Play_WoodenDoorUnlock()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_WoodenDoorUnlock);
    }
    public void Play_SchoolGateUnlock()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_SchoolGateUnlock);
    }
    public void Play_WipeTheBlackboard()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_WipeTheBlackboard);
    }

    public void Play_ChalkWriting()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_ChalkWriting);
    }
    public void Play_SuanPan()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_SuanPan);
    }

    public void Play_WoodenStructure()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_WoodenStructure);
    }
    public void Play_SecondHand()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_SecondHand);
    }
    public void Play_IronCabinet()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_IronCabinet);
    }



    public void PlayMouseClick()
    {
        //Debug.Log("PlayMouseClick");
        AudioManager.Instance.PlaySoundEffect(_UIHover);
    }


    //public void PlayBGMLobby()
    //{
    //    Debug.Log("PlayBGMLobby");
    //    //AudioManager.Instance.PlayBGM(_clipBGMLobby);
    //}
    //public void PlayBGMLevel()
    //{
    //    Debug.Log("PlayBGMLevel");
    //    AudioManager.Instance.PlayBGM(_clipBGMLevel);
    //}
}