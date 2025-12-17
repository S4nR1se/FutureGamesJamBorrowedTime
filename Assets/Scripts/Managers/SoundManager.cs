using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Manager
{
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public struct SoundDefinition
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        public bool is2D;
        public bool loop;
        public float pitch;

        public SoundDefinition(string name, AudioClip clip, float volume = 1f, bool is2D = true, bool loop = false, float pitch = 1f)
        {
            this.name = name;
            this.clip = clip;
            this.volume = volume;
            this.is2D = is2D;
            this.loop = loop;
            this.pitch = pitch;
        }
    }

    [Header("Sound Library")]
    [SerializeField] public SoundLibrary _soundLibrary;

    public float GetMasterVolume() => _masterVolume;
    public float GetSFXVolume() => _masterSFXVolume;
    public float GetMusicVolume() => _masterMusicVolume;

    [Header("Audio Source Pool Settings")]
    [SerializeField] private int _maxAudioSources = 10;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float _masterVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] private float _masterSFXVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] private float _masterMusicVolume = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private bool _enablePitchVariation = true;
    [SerializeField] private float _pitchVariationAmount = 0.1f;

    private GameObject _audioSourceParent;
    private Queue<AudioSource> _availableSources = new();
    private Dictionary<AudioSource, SoundInstance> _activeSounds = new();
    private HashSet<AudioSource> _backgroundMusicSources = new();

    private const int MAX_VOLUME_UI = 10;
    public int GetMasterVolumeUI() => Mathf.RoundToInt(_masterVolume * MAX_VOLUME_UI);
    public int GetSFXVolumeUI() => Mathf.RoundToInt(_masterSFXVolume * MAX_VOLUME_UI);
    public int GetMusicVolumeUI() => Mathf.RoundToInt(_masterMusicVolume * MAX_VOLUME_UI);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSourceParent = new GameObject("AudioSourceParent");
            _audioSourceParent.transform.SetParent(transform);

            for (int i = 0; i < _maxAudioSources; i++)
            {
                CreateAudioSource();
            }

            if (_soundLibrary != null)
            {
                _soundLibrary.Initialize();
            }
            else
            {
                Debug.LogWarning("SoundManager: No SoundLibrary assigned.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            StopAllSounds();
            Instance = null;
        }
    }
    public AudioSource PlaySound(string name, Vector3 position = default)
    {
        if (_soundLibrary == null)
        {
            Debug.LogWarning("No SoundLibrary assigned to SoundManager.");
            return null;
        }

        if (!_soundLibrary.TryGetSound(name, out SoundDefinition sound))
        {
            Debug.LogWarning($"Sound '{name}' not found in SoundLibrary.");
            return null;
        }

        if (sound.loop)
            return PlayLoopingSound(sound.clip, position, sound.volume, sound.is2D, sound.pitch);
        else
            return PlaySoundEffect(sound.clip, position, sound.volume, sound.is2D, sound.pitch);
    }

    public AudioSource PlaySoundEffect(AudioClip clip, Vector3 position, float volume = 0.25f, bool is2D = true, float pitch = 1f)
    {
        if (clip == null)
        {
            return null;
        }

        AudioSource source = GetOrCreateAudioSource();
        if (source == null)
        {
            return null;
        }

        
        ConfigureAudioSource(source, clip, position, volume, is2D, pitch, false);

        source.volume = Mathf.Clamp01(volume * _masterVolume * _masterSFXVolume);

        StartSoundPlayback(source, clip.length, volume, false);

        return source;
    }
    public AudioSource PlayLoopingSound(AudioClip clip, Vector3 position, float volume = 0.25f, bool is2D = true, float pitch = 1f)
    {
        if (clip == null)
        {
            return null;
        }

        AudioSource source = GetOrCreateAudioSource();
        if (source == null) return null;

        ConfigureAudioSource(source, clip, position, volume, is2D, pitch, true);

        source.volume = Mathf.Clamp01(volume * _masterVolume * _masterMusicVolume);
        source.Play();

        _activeSounds[source] = new SoundInstance(volume, null, true);

        return source;
    }

    public void StopSound(AudioSource source, float fadeOutDuration = 0f)
    {
        if (source == null || !_activeSounds.ContainsKey(source)) return;

        if (fadeOutDuration > 0f)
        {
            StartCoroutine(FadeOutAndStop(source, fadeOutDuration));
        }
        else
        {
            ReturnSourceToPool(source);
        }
    }

    public void StopAllSounds(float fadeOutDuration = 0f)
    {
        var activeSoundsCopy = new List<AudioSource>(_activeSounds.Keys);

        foreach (var source in activeSoundsCopy)
        {
            if (source != null)
            {
                StopSound(source, fadeOutDuration);
            }
        }
    }

    public void RegisterBackgroundMusic(AudioSource musicSource)
    {
        if (musicSource == null) return;
 
        if (_backgroundMusicSources.Add(musicSource))
        {
            if (!_activeSounds.ContainsKey(musicSource))
            {
                float product = _masterVolume * _masterMusicVolume;
                float original = musicSource.volume;
                if (product > 0f) original = musicSource.volume / product;

                _activeSounds[musicSource] = new SoundInstance(original, null, true);
            }

            UpdateMusicVolumes();
        }
    }

    public void UnregisterBackgroundMusic(AudioSource musicSource)
    {
        if (musicSource != null)
        {
            _backgroundMusicSources.Remove(musicSource);
        }
    }

    public void FadeMusicIn(AudioSource musicSource, float targetVolume, float duration)
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeMusic(musicSource, targetVolume, duration));
        }
    }

    public void FadeMusicOut(AudioSource musicSource, float duration, bool stopOnComplete = true)
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeMusic(musicSource, 0f, duration, stopOnComplete));
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        _masterSFXVolume = Mathf.Clamp01(volume);
        UpdateSFXVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        _masterMusicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolumes();
    }

    public void SetMasterVolumeFromUI(int volume)
    {
        SetMasterVolume(volume / (float)MAX_VOLUME_UI);
    }

    public void SetSFXVolumeFromUI(int volume)
    {
        SetSFXVolume(volume / (float)MAX_VOLUME_UI);
    }

    public void SetMusicVolumeFromUI(int volume)
    {
        SetMusicVolume(volume / (float)MAX_VOLUME_UI);
    }

    private void UpdateAllVolumes()
    {
        UpdateSFXVolumes();
        UpdateMusicVolumes();
    }

    private void UpdateSFXVolumes()
    {
        foreach (var kvp in _activeSounds)
        {
            AudioSource source = kvp.Key;
            SoundInstance instance = kvp.Value;

            if (source != null && source.isPlaying && !instance.IsLooping)
            {
                source.volume = Mathf.Clamp01(instance.OriginalVolume * _masterVolume * _masterSFXVolume);
            }
        }
    }

    private void UpdateMusicVolumes()
    {
        foreach (var musicSource in _backgroundMusicSources)
        {
            if (musicSource == null) continue;

            if (_activeSounds.TryGetValue(musicSource, out SoundInstance instance))
            {
                musicSource.volume = Mathf.Clamp01(instance.OriginalVolume * _masterVolume * _masterMusicVolume);
            }
            else
            {
                float product = _masterVolume * _masterMusicVolume;
                float normalized = musicSource.volume;
                if (product > 0f) normalized = musicSource.volume / product;
                musicSource.volume = Mathf.Clamp01(normalized * product);
            }
        }
    }

    private AudioSource CreateAudioSource()
    {
        GameObject soundObject = new GameObject($"SFX_AudioSource_{_availableSources.Count}");
        soundObject.transform.SetParent(_audioSourceParent.transform);

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        _availableSources.Enqueue(source);
        return source;
    }

    private AudioSource GetOrCreateAudioSource()
    {
        while (_availableSources.Count > 0)
        {
            AudioSource source = _availableSources.Dequeue();
            if (source != null && !source.isPlaying)
            {
                return source;
            }
        }

        foreach (var kvp in _activeSounds)
        {
            if (kvp.Key != null && !kvp.Key.isPlaying)
            {
                ReturnSourceToPool(kvp.Key);
                return GetOrCreateAudioSource();
            }
        }

        if (_availableSources.Count + _activeSounds.Count < _maxAudioSources * 2)
        {
            return CreateAudioSource();
        }
        return null;
    }

    private void ConfigureAudioSource(AudioSource source, AudioClip clip, Vector3 position, float volume, bool is2D, float pitch, bool loop)
    {
        source.transform.position = position;
        source.clip = clip;

        // store raw/original volume — do not apply master multipliers here
        source.volume = volume;

        source.spatialBlend = is2D ? 0f : 1f;
        source.loop = loop;

        if (_enablePitchVariation && !loop)
        {
            source.pitch = pitch + UnityEngine.Random.Range(-_pitchVariationAmount, _pitchVariationAmount);
        }
        else
        {
            source.pitch = pitch;
        }
    }

    // Start playback for transient SFX and store original volume
    private void StartSoundPlayback(AudioSource source, float duration, float originalVolume, bool isLooping)
    {
        source.Play();
        Coroutine cleanupCoroutine = isLooping ? null : StartCoroutine(CleanupAfterPlayback(source, duration));
        _activeSounds[source] = new SoundInstance(originalVolume, cleanupCoroutine, isLooping);
    }

    private void ReturnSourceToPool(AudioSource source)
    {
        if (source == null) return;

        if (_activeSounds.TryGetValue(source, out SoundInstance instance))
        {
            if (instance.CleanupCoroutine != null)
            {
                StopCoroutine(instance.CleanupCoroutine);
            }
            _activeSounds.Remove(source);
        }

        source.Stop();
        source.clip = null;
        source.loop = false;
        source.pitch = 1f;
        _availableSources.Enqueue(source);
    }

    private IEnumerator CleanupAfterPlayback(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        ReturnSourceToPool(source);
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        ReturnSourceToPool(source);
    }

    private IEnumerator FadeMusic(AudioSource musicSource, float targetVolume, float duration, bool stopOnComplete = false)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float normalizedTarget = targetVolume * _masterVolume * _masterMusicVolume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, normalizedTarget, elapsed / duration);
            yield return null;
        }

        musicSource.volume = normalizedTarget;

        if (stopOnComplete)
        {
            musicSource.Stop();
        }
    }

    private class SoundInstance
    {
        public float OriginalVolume { get; }
        public Coroutine CleanupCoroutine { get; }
        public bool IsLooping { get; }

        public SoundInstance(float originalVolume, Coroutine cleanupCoroutine, bool isLooping)
        {
            OriginalVolume = originalVolume;
            CleanupCoroutine = cleanupCoroutine;
            IsLooping = isLooping;
        }
    }
}