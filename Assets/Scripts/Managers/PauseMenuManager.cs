using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            this.gameObject.SetActive(true);
            //switch state to pause state
        }
    }

    public void ContinueGame()
   {
        this.gameObject.SetActive(false);
        //switch state to playing state
    }

    public void OpenSettingsMenu()
    {
        SettingsManager.SettingsMInstance.gameObject.SetActive(true);
    }

   public void ExitGame()
   {
       SceneManager.LoadSceneAsync("MainMenu");
   }
}
