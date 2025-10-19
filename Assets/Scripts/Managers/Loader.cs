using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] GameObject _saveManager;

    private void Awake()
    {
        //if (SettingsMenu.sm_instance == null)
        //{
        //    Instantiate(settings_menu);
        //}

        if (SaveManager.SaveInstance == null)
        {
            Instantiate(_saveManager);
        }
    }
}
