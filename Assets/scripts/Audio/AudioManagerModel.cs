using System;

public class AudioManagerModel
{
    public Action<bool> OnBGMSwtich = delegate { };

    public bool IsBgmOpen
    {
        get => _isBGMOpen;
        set
        {
            if (_isBGMOpen != value)
            {
                _isBGMOpen = value;
                if (!value)
                {
                    //StaticModule.SwitchMusicOff();
                }

                //CommonDataStorage.SetBGMOpen(value);
                OnBGMSwtich.Invoke(value);
            }
        }
    }

    public bool IsSoundOpen
    {
        get => _isSoundOpen;
        set
        {
            if (_isSoundOpen != value)
            {
                SetDoozySound(value);
                _isSoundOpen = value;
                if (!value)
                {
                    //StaticModule.SwitchSoundOff();
                }

                //CommonDataStorage.SetSoundOpen(value);
            }
        }
    }

    public bool IsVibrateOpen
    {
        get => _isVibrateOpen;
        set
        {
            if (_isVibrateOpen != value)
            {
                _isVibrateOpen = value;
                if (!value)
                {
                    //StaticModule.SwitchVibrateOff();
                }

                //CommonDataStorage.SetVibrateOpen(value);
            }
        }
    }

    private void SetDoozySound(bool isOpen)
    {

    }

    public AudioManagerModel()
    {
        //_isSoundOpen = CommonDataStorage.GetSoundOpen();
        //_isBGMOpen = CommonDataStorage.GetBGMOpen();
        //_isVibrateOpen = CommonDataStorage.GetVibrateOpen();
        SetDoozySound(_isSoundOpen);
    }

    private bool _isSoundOpen = true;
    private bool _isBGMOpen = true;
    private bool _isVibrateOpen = true;
}