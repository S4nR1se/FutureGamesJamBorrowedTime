using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] GameObject _saveManager = null;
    [SerializeField] GameObject _settingsManager = null;
    [SerializeField] GameObject _soundManager = null;

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

        if (SoundManager.Instance == null)
        {
            Instantiate(_soundManager);
        }
    }
}
