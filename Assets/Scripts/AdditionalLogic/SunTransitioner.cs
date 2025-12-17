using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class SunTransitioner : MonoBehaviour
{
    private Light sunLight;

    [SerializeField] private Texture2D daySprite;
    [SerializeField] private Texture2D nightSprite;
    [SerializeField] private RawImage dayUI;

    private Vector3 dayRotation = new Vector3(30f, 70f, 0f);
    private Vector3 nightRotation = new Vector3(-10f, 70f, 0f);

    private const float DAYINTENSITY = 1.2f;
    private const float NIGHTINTENSITY = 0.05f;
    private const float TRANSITIONDURATION = 5f;

    private Coroutine _transitionCoroutine;

    private AudioSource _musicSource;

    [SerializeField] private string dayMusicName = "DayMusic";
    [SerializeField] private string nightMusicName = "NightMusic";

    public void InitializeLighting(DayCycle initialCycle)
    {
        sunLight = FindFirstObjectByType(typeof(Light)) as Light;
        if (sunLight == null)
        {
            return;
        }

        if (initialCycle == DayCycle.Day)
        {
            sunLight.transform.rotation = Quaternion.Euler(dayRotation);
            sunLight.intensity = DAYINTENSITY;
            //_musicSource = SoundManager.Instance.PlaySound("DayMusic");
            //SoundManager.Instance.RegisterBackgroundMusic(_musicSource);

        }
        else
        {
            sunLight.transform.rotation = Quaternion.Euler(nightRotation);
            sunLight.intensity = NIGHTINTENSITY;
            //_musicSource = SoundManager.Instance.PlaySound("NightMusic");
            //SoundManager.Instance.RegisterBackgroundMusic(_musicSource);

        }
    }

    /*public void PlayMusicOnCycle(DayCycle cycle)
    {
        string musicToPlay = cycle == DayCycle.Day ? dayMusicName : nightMusicName;
        if (_musicSource != null)
        {
            SoundManager.Instance.FadeMusicOut(_musicSource, 1.5f);
        }
        
        _musicSource = SoundManager.Instance.PlaySound(musicToPlay);
        
        if (_musicSource != null)
        {
            SoundManager.Instance.RegisterBackgroundMusic(_musicSource);
        }
        else
        {
            Debug.LogWarning("Failed to create/play day/night cycle music AudioSource.");
        }

   }
     */
    public void TransitionToCycle(DayCycle targetCycle)
    {
        if(targetCycle  == DayCycle.Day)
        {
            dayUI.texture = daySprite;
        }
        else
        {
            dayUI.texture = nightSprite;
        }
        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        _transitionCoroutine = StartCoroutine(LerpLightTransition(targetCycle));
    }

    private IEnumerator LerpLightTransition(DayCycle targetCycle)
    {
        Quaternion startRot = sunLight.transform.rotation;
        Quaternion endRot = Quaternion.Euler(targetCycle == DayCycle.Day ? dayRotation : nightRotation);
        float startIntensity = sunLight.intensity;
        float endIntensity = targetCycle == DayCycle.Day ? DAYINTENSITY : NIGHTINTENSITY;

        float elapsed = 0f;
        while (elapsed < TRANSITIONDURATION)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / TRANSITIONDURATION);

            sunLight.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            sunLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);

            yield return null;
        }

        sunLight.transform.rotation = endRot;
        sunLight.intensity = endIntensity;
    }
}
