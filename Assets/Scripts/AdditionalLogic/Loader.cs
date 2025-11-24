using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] GameObject _settingsMenu;
    private void Awake()
    {
        if (SettingsManager.Instance == null)
        {
            Instantiate(_settingsMenu);
        }

        //if (SaveManager.save_instance == null)
        //{
        //    Instantiate(save_manager);
        //}
    }
}
