using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    //private void Awake()
    //{
    //    SaveManager.save_instance.Load_Data();
    //}

    public void Play_Game()
    {
        SceneManager.LoadSceneAsync("SampleScene");
    }

    public void Open_Settings_Menu()
    {
        //SettingsMenu.sm_instance.gameObject.SetActive(true);
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
