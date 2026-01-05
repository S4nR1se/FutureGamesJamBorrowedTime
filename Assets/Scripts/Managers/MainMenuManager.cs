using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{



    private AudioSource _buttonMeow;
    private AudioSource _musicSource;
    private string[] _buttonSound = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13" };

    private void Start()
    {
        //SaveManager.save_instance.Load_Data();
        SetUpAudio();
    }

     void SetUpAudio()
     {
         _musicSource = SoundManager.Instance.PlaySound("MainMenuMusic");


         if (_musicSource == null)
         {
             Debug.LogWarning("Failed to create/play main menu music AudioSource.");
         }
         SoundManager.Instance.RegisterBackgroundMusic(_musicSource);

         SoundManager.Instance.SetMasterVolume(SettingsManager.Instance.GetMasterAudioSliderVolume());
         SoundManager.Instance.SetMusicVolume(SettingsManager.Instance.GetMusicAudioSliderVolume());
         SoundManager.Instance.SetSFXVolume(SettingsManager.Instance.GetSoundEffectsAudioSliderVolume());

          // SoundManager.Instance.PlayLoopingSound(_mainMenuAudioSource.clip, Vector3.one);

      }
     
    public void Start_Game()
    {
        
        Debug.Log("Start Game");
        SoundManager.Instance.FadeMusicOut(_musicSource, 1.5f);
       
        SceneManager.LoadSceneAsync("MAINSCENE");
        _musicSource = SoundManager.Instance.PlaySound("Play-Button");

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
        _buttonMeow = SoundManager.Instance.PlaySound(_buttonSound[UnityEngine.Random.Range(0, _buttonSound.Length)]);
        Application.Quit();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
