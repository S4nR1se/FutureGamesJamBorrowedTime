using UnityEngine;

public class GameManager2 : StateMachine
{
    [SerializeField] private AudioClip _inGameAudioClip;
    AudioSource _inGameAudioSource = null;

    private void Awake()
    {
        RegisterState(new PlayingState());
        SwitchState<PlayingState>();
    }

    private void SetAudio()
    {
        SoundManager.Instance.Initialize();

        _inGameAudioSource = GetComponent<AudioSource>();
        _inGameAudioSource.clip = _inGameAudioClip;
        SoundManager.Instance.RegisterBackgroundMusic(_inGameAudioSource);

        SoundManager.Instance.SetMasterVolume(SettingsManager.SettingsMInstance.GetMasterAudioSliderVolume());
        SoundManager.Instance.SetMusicVolume(SettingsManager.SettingsMInstance.GetMusicAudioSliderVolume());
        //SoundManager.Instance.SetSFXVolume(SettingsManager.SettingsMInstance.GetSoundEffectsAudioSliderVolume());

        SoundManager.Instance.PlayLoopingSound(_inGameAudioSource.clip, Vector3.one);
    }

    private void Start()
    {
        SetAudio();
    }
}
