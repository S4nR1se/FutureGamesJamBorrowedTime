using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    AudioSource _mainMenuAudioSource;
    [SerializeField] AudioClip _mainMenuMusic;
    private void Start()
    {
        //SaveManager.save_instance.Load_Data();
        SetUpAudio();
    }

    void SetUpAudio()
    {
        // AudioSource musicSource = SoundManager.Instance.PlaySound("MainMenuMusic");

        _mainMenuAudioSource = GetComponent<AudioSource>();
        if(_mainMenuAudioSource == null)
        {            
            _mainMenuAudioSource = gameObject.AddComponent<AudioSource>();
        }
        SoundManager.SoundDefinition sound;
        if (!SoundManager.Instance._soundLibrary.TryGetSound("MainMenuMusic", out sound))
        {
            Debug.LogError("MainMenuMusic sound not found in SoundLibrary.");
            return;
        }
        ;
        AudioSource musicSource = SoundManager.Instance.PlayLoopingSound(
        sound.clip, Vector3.zero, sound.volume, sound.is2D, sound.pitch
    );
        // _mainMenuAudioSource.clip = _mainMenuMusic;
        // _mainMenuAudioSource.loop = true;
        // _mainMenuAudioSource.Play();
        if (musicSource != null)
        {
            _mainMenuAudioSource = musicSource;
            SoundManager.Instance.RegisterBackgroundMusic(_mainMenuAudioSource);
        }
        else
        {
            Debug.LogWarning("Failed to create/play main menu music AudioSource.");
        }

        // SoundManager.Instance.RegisterBackgroundMusic(_mainMenuAudioSource);

        SoundManager.Instance.SetMasterVolume(SettingsManager.Instance.GetMasterAudioSliderVolume());
        SoundManager.Instance.SetMusicVolume(SettingsManager.Instance.GetMusicAudioSliderVolume());
        SoundManager.Instance.SetSFXVolume(SettingsManager.Instance.GetSoundEffectsAudioSliderVolume());

        // SoundManager.Instance.PlayLoopingSound(_mainMenuAudioSource.clip, Vector3.one);
    }

    public void Start_Game()
    {
        SceneManager.LoadSceneAsync("MAINSCENE");
        Debug.Log("Start Game");
        if (Input.GetKeyDown(KeyCode.Escape))
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
