using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsManager : Manager
{
    public static SettingsManager Instance {get; private set;}

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        this.gameObject.SetActive(false);
        //fullscreen.isOn = Screen.fullScreen;
        //sound_volume.value = audio.volume;

        //if (QualitySettings.vSyncCount == 0)
        //{
        //    VSync.isOn = false;
        //}
        //else
        //{
        //    VSync.isOn = true;
        //}
    }

    public void Close_Settings()
    {
        if (this.gameObject.activeInHierarchy)
        {
            this.gameObject.SetActive(false);
        }
    }


    //public class SettingsMenu : MonoBehaviour
    //{
    //    public static SettingsMenu sm_instance = null;

    //    [SerializeField] Slider sound_volume = null;
    //    [SerializeField] AudioSource audio = null;
    //    [SerializeField] Toggle fullscreen = null;
    //    [SerializeField] Toggle VSync = null;
    //    [SerializeField] List<Vector2> resolutions = new List<Vector2>();
    //    [SerializeField] TMP_Text resolutions_text = null;

    //    int selected_resolution = 0;
    //    public Vector2 resolution = new();

    //    private void Awake()
    //    {
    //        if (sm_instance == null)
    //        {
    //            sm_instance = this;
    //        }
    //        else if (sm_instance != this)
    //        {
    //            Destroy(this.gameObject);
    //        }

    //        DontDestroyOnLoad(this.gameObject);
    //    }

    //    

    //    public float Get_Audio_Value()
    //    {
    //        return sound_volume.value;
    //    }

    //    public void Set_Audio_Source(AudioSource new_audio)
    //    {
    //        audio = new_audio;
    //    }

    //   

    //    public void Update_Sound_Volume()
    //    {
    //        audio.volume = sound_volume.value;
    //    }

    //    public void Res_Left_Arrow()
    //    {
    //        selected_resolution--;
    //        if (selected_resolution < 0)
    //        {
    //            selected_resolution = 0;
    //        }

    //        Update_Resolutions_Text();
    //    }

    //    public void Res_Right_Arrow()
    //    {
    //        selected_resolution++;
    //        if (selected_resolution > resolutions.Count - 1)
    //        {
    //            selected_resolution = resolutions.Count - 1;
    //        }

    //        Update_Resolutions_Text();
    //    }

    //    public void Update_Resolutions_Text()
    //    {
    //        resolutions_text.text = resolutions[selected_resolution].x.ToString() + " x " + resolutions[selected_resolution].y.ToString();
    //    }

    //    public void Apply_Changes()
    //    {
    //        if (VSync.isOn)
    //        {
    //            QualitySettings.vSyncCount = 1;
    //        }
    //        else
    //        {
    //            QualitySettings.vSyncCount = 0;
    //        }

    //        Screen.SetResolution((int)resolutions[selected_resolution].x, (int)resolutions[selected_resolution].y, fullscreen.isOn);
    //    }
    //}

}
