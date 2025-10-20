using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
   public void ContinueGame()
   {
        this.gameObject.SetActive(false);
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
