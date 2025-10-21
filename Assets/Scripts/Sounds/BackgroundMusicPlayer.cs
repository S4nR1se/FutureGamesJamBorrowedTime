using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float fadeInTime = 2f;
    [SerializeField] private float targetVolume = 1f;

    private void Start()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }


        SoundManager.Instance.RegisterBackgroundMusic(musicSource);


        musicSource.volume = 0f;


        musicSource.Play();


        SoundManager.Instance.FadeMusicIn(musicSource, targetVolume, fadeInTime);
    }
}
