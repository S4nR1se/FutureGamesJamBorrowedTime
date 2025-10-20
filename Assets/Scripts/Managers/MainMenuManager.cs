using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private void Awake()
    {
        SaveManager.SaveInstance.Load_Data();
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
