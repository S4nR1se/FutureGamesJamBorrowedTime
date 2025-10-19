using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] GameObject save_manager;

    private void Awake()
    {
        //if (SettingsMenu.sm_instance == null)
        //{
        //    Instantiate(settings_menu);
        //}

        if (SaveManager.save_instance == null)
        {
            Instantiate(save_manager);
        }
    }
}
