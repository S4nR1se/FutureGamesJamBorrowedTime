using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    [System.Serializable]
    public struct ParticleDefinition
    {
        public string name;
        public ParticleSystem prefab;
        public float lifetimeOverride;
    }
    public static ParticleSystemManager Instance { get; private set; }

    [Header("Particle Library")]
    [SerializeField] private ParticleLibrary _particleLibrary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_particleLibrary != null)
                _particleLibrary.Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ParticleSystem Spawn(string name, Vector3 position, Quaternion rotation = default)
    {
        if (_particleLibrary == null)
        {
            Debug.LogWarning("No ParticleLibrary assigned to ParticleSystemManager.");
            return null;
        }

        if (!_particleLibrary.TryGetParticle(name, out ParticleDefinition definition))
        {
            Debug.LogWarning($"Particle '{name}' not found in ParticleLibrary.");
            return null;
        }

        if (definition.prefab == null) return null;

        ParticleSystem instance = Instantiate(definition.prefab, position, rotation);
        float lifetime = definition.lifetimeOverride > 0 ? definition.lifetimeOverride : instance.main.duration;
        StartCoroutine(CleanupAfterDuration(instance, lifetime));

        return instance;
    }

    private IEnumerator CleanupAfterDuration(ParticleSystem ps, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (ps != null)
            Destroy(ps.gameObject);
    }
}
