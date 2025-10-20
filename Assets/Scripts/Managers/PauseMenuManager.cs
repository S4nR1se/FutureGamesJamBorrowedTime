using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu = null;

    private void Start()
    {
        _pauseMenu.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            _pauseMenu.gameObject.SetActive(true);
            //switch state to pause state
        }
    }

    public void ContinueGame()
   {
        _pauseMenu.gameObject.SetActive(false);
        //switch state to playing state
    }

    public void OpenSettingsMenu()
    {
        SettingsManager.SettingsMInstance.gameObject.SetActive(true);
    }

   public void ExitGame()
   {
        SoundManager.Instance.StopAllSounds();
        SceneManager.LoadSceneAsync("MainMenu");
   }
}
