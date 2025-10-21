using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager SettingsMInstance { get; private set; }

    [SerializeField] private Slider _masterVolumeSlider = null;
    [SerializeField] private Slider _musicVolumeSlider = null;
    [SerializeField] private Slider _soundEffectsVolumeSlider = null;

    [SerializeField] private GameObject _audioCanvas = null;
    [SerializeField] private GameObject _controlsCanvas = null;
    [SerializeField] private GameObject _resolutionsCanvas = null;

    [SerializeField] private GameObject _uiCanvas = null;
    [SerializeField] private GameObject _cameraCanvas = null;

    [SerializeField] private List<Vector2> _resolutions = new List<Vector2>();
    [SerializeField] private TMP_Text _resolutionsText = null;

    private int _selectedResolution = 0;
    public Vector2 _resolution = new();

    private void Awake()
    {
        if (SettingsMInstance == null)
        {
            SettingsMInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void Initialize() { }

    private void OnDestroy()
    {
        if (SettingsMInstance == this)
        {
            SettingsMInstance = null;
        }
    }

    private void Start()
    {
        Screen.fullScreen = true;
        this.gameObject.SetActive(false);
        _resolutionsCanvas.SetActive(false);
        _audioCanvas.SetActive(false);
        _cameraCanvas.SetActive(false);
        _controlsCanvas.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = true;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Screen.fullScreen = false;
        }
    }

    public void UpdateMasterAudioVolume()
    {
        SoundManager.Instance.SetMasterVolume(_masterVolumeSlider.value);
    }

    public void UpdateMusicAudioVolume()
    {
        SoundManager.Instance.SetMusicVolume(_musicVolumeSlider.value);
    }

    public void UpdateSoundEffectsAudioVolume()
    {
        //SoundManager.Instance.SetSFXVolume(_soundEffectsVolumeSlider.value);
    }

    public void OpenAudioPanel()
    {
        _audioCanvas.SetActive(true);

        _controlsCanvas.SetActive(false);
        _resolutionsCanvas.SetActive(false);
    }

    public void OpenResolutionsPanel()
    {
        _resolutionsCanvas.SetActive(true);

        _audioCanvas.SetActive(false);
        _controlsCanvas.SetActive(false);
    }

    public void OpenControlsPanel()
    {
        _controlsCanvas.SetActive(true);
        
        _resolutionsCanvas.SetActive(false);
        _audioCanvas.SetActive(false);
    }

    public void Res_Left_Arrow()
    {
        _selectedResolution--;
        if (_selectedResolution < 0)
        {
            _selectedResolution = 0;
        }

        Update_Resolutions_Text();
    }

    public void Res_Right_Arrow()
    {
        _selectedResolution++;
        if (_selectedResolution > _resolutions.Count - 1)
        {
            _selectedResolution = _resolutions.Count - 1;
        }

        Update_Resolutions_Text();
    }

    public void Update_Resolutions_Text()
    {
        _resolutionsText.text = _resolutions[_selectedResolution].x.ToString() + " x " + _resolutions[_selectedResolution].y.ToString();
    }

    public void Apply_Changes()
    {
        Screen.SetResolution((int)_resolutions[_selectedResolution].x, (int)_resolutions[_selectedResolution].y, true);
    }

    public float GetMasterAudioSliderVolume()
    {
        return _masterVolumeSlider.value;
    }

    public float GetMusicAudioSliderVolume()
    {
        return _musicVolumeSlider.value;
    }

    public float GetSoundEffectsAudioSliderVolume()
    {
        return _soundEffectsVolumeSlider.value;
    }

    public void CloseSettings()
    {
        if (this.gameObject.activeInHierarchy)
        {
            this.gameObject.SetActive(false);
        }
    }
}
