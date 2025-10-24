using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private void Awake()
    {
        //SaveManager.save_instance.Load_Data();
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
