using System.Resources;
using UnityEngine;

public class Zombie : Undead
{
    [Header("Audio Clips")]
    public AudioClip[] ZombieSpawnSFX;
    public AudioClip ZombieBiteSFX;

    private AudioSource _audioSource;
    //private SoundManager _soundManager;

    public override void Initialize(Zone startZone, string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night, SoundManager soundManager = null)
    {
        _soundManager = soundManager;
        base.Initialize(startZone, "Zombie", 3, movementSpeed, occupation, restZoneType, activeCycle);

        // Setup audio
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        PlayRandomSpawnSFX();
    }

    private void PlayRandomSpawnSFX()
    {
        if (ZombieSpawnSFX != null && ZombieSpawnSFX.Length > 0)
        {
            AudioClip clip = ZombieSpawnSFX[Random.Range(0, ZombieSpawnSFX.Length)];
            _audioSource.PlayOneShot(clip);
        }
    }

    private void Start()
    {
        // only for testing or standalone use
        if (ZombieSpawnSFX != null && ZombieSpawnSFX.Length > 0)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            PlayRandomSpawnSFX();
        }
    }
}
