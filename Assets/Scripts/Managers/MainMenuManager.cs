using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    AudioSource _mainMenuAudioSource;
    [SerializeField] AudioClip _mainMenuMusic;
    private void Awake()
    {
        //SaveManager.save_instance.Load_Data();
        SetUpAudio();
    }

    void SetUpAudio()
    {

        _mainMenuAudioSource = GetComponent<AudioSource>();
        _mainMenuAudioSource.clip = _mainMenuMusic;
        SoundManager.Instance.RegisterBackgroundMusic(_mainMenuAudioSource);

        SoundManager.Instance.SetMasterVolume(SettingsManager.Instance.GetMasterAudioSliderVolume());
        SoundManager.Instance.SetMusicVolume(SettingsManager.Instance.GetMusicAudioSliderVolume());
        //SoundManager.Instance.SetSFXVolume(SettingsManager.Instance.GetSoundEffectsAudioSliderVolume());

        SoundManager.Instance.PlayLoopingSound(_mainMenuAudioSource.clip, Vector3.one);
    }

    public void Start_Game()
    {
        SceneManager.LoadSceneAsync("MAINSCENE");

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void Open_Settings_Menu()
    {
        SettingsManager.Instance.gameObject.SetActive(true);
    }

    public void Open_Credits_Menu()
    {
        
    }

    public void Quit_Game()
    {
        Application.Quit();
    }
}
