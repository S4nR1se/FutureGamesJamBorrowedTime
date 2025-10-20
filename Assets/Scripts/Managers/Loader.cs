using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] GameObject _saveManager = null;
    [SerializeField] GameObject _settingsManager = null;

    private void Awake()
    {
        if (SettingsManager.SettingsMInstance == null)
        {
            Instantiate(_settingsManager);
        }

        if (SaveManager.SaveInstance == null)
        {
            Instantiate(_saveManager);
        }
    }
}
