using UnityEngine;

namespace Audio
{
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
            if (_UIHover)
            {
                if (AudioManager.Instance)
                    AudioManager.Instance.PlaySoundEffect(_UIHover, false);
            }
        }

        public void Play_UIClick()
        {
            if (_UIHover)
                AudioManager.Instance.PlaySoundEffect(_UIClick, false);
        }
        public void Play_Dialogue()
        {
            AudioManager.Instance.PlaySoundEffect(_Dialogue, false);
        }
        public void Play_Burning()
        {
            AudioManager.Instance.PlaySoundEffect(_Burning, true);
        }
        public void Stop_Burning()
        {
            AudioManager.Instance.StopSoundEffect(_Burning);
        }
        public void Play_PickUpItems()
        {
            AudioManager.Instance.PlaySoundEffect(_PickUpItems, false);
        }
        public void Play_PickUpPaper()
        {
            AudioManager.Instance.PlaySoundEffect(_PickUpPaper, false);
        }
        public void Play_ClassroomLocked()
        {
            AudioManager.Instance.PlaySoundEffect(_ClassroomLocked, false);
        }
        public void Play_WoodenDoorLocked()
        {
            AudioManager.Instance.PlaySoundEffect(_WoodenDoorLocked, false);
        }
        public void Play_SchoolGateLocked()
        {
            AudioManager.Instance.PlaySoundEffect(_SchoolGateLocked, false);
        }
        public void Play_Footsteps()
        {
            AudioManager.Instance.PlaySoundEffect(_Footsteps, true);
        }
        public void Stop_Footsteps()
        {
            AudioManager.Instance.StopSoundEffect(_Footsteps);
        }
        public void Play_ManWhisper()
        {
            AudioManager.Instance.PlaySoundEffect(_ManWhisper, false);
        }
        public void Play_WoodenDoorUnlock()
        {
            AudioManager.Instance.PlaySoundEffect(_WoodenDoorUnlock, false);
        }
        public void Play_SchoolGateUnlock()
        {
            AudioManager.Instance.PlaySoundEffect(_SchoolGateUnlock, false);
        }
        public void Play_WipeTheBlackboard()
        {
            AudioManager.Instance.PlaySoundEffect(_WipeTheBlackboard, false);
        }

        public void Play_ChalkWriting()
        {
            AudioManager.Instance.PlaySoundEffect(_ChalkWriting, false);
        }
        public void Play_SuanPan()
        {
            AudioManager.Instance.PlaySoundEffect(_SuanPan, false);
        }

        public void Play_WoodenStructure()
        {
            AudioManager.Instance.PlaySoundEffect(_WoodenStructure, false);
        }
        public void Play_SecondHand()
        {
            AudioManager.Instance.PlaySoundEffect(_SecondHand, false);
        }
        public void Play_IronCabinet()
        {
            AudioManager.Instance.PlaySoundEffect(_IronCabinet, false);
        }



        public void PlayMouseClick()
        {
            AudioManager.Instance.PlaySoundEffect(_UIHover, false);
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
}