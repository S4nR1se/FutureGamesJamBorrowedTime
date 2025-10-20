using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private AudioClip _mainMenuMusic;

    AudioSource _mainMenuAudioSource = null;
    private void Awake()
    {
        SaveManager.SaveInstance.Load_Data();
        SoundManager.Instance.Initialize();

        _mainMenuAudioSource = GetComponent<AudioSource>();
        _mainMenuAudioSource.clip = _mainMenuMusic;
        SoundManager.Instance.RegisterBackgroundMusic(_mainMenuAudioSource);
        
        SoundManager.Instance.SetMasterVolume(SettingsManager.SettingsMInstance.GetMasterAudioSliderVolume());
        SoundManager.Instance.SetMusicVolume(SettingsManager.SettingsMInstance.GetMusicAudioSliderVolume());
        //SoundManager.Instance.SetSFXVolume(SettingsManager.SettingsMInstance.GetSoundEffectsAudioSliderVolume());
        
        SoundManager.Instance.PlayLoopingSound(_mainMenuAudioSource.clip, Vector3.one);
    }

    public void Play_Game()
    {
        SceneManager.LoadSceneAsync("SampleScene");
    }

    public void Open_Settings_Menu()
    {
        SettingsManager.SettingsMInstance.gameObject.SetActive(true);
    }

    public void Open_Stats_Menu()
    {
        //stats_panel.SetActive(true);
    }

    public void Quit_Game()
    {
        Application.Quit();
    }
}
