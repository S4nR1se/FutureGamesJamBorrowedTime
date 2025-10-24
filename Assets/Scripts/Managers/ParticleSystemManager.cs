using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    [System.Serializable]
    public struct ParticleDefinition
    {
        public string name;
        public GameObject prefab;
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
            return null;
        }

        if (!_particleLibrary.TryGetParticle(name, out ParticleDefinition definition))
        {
            return null;
        }

        if (definition.prefab == null) return null;

        GameObject instance = Instantiate(definition.prefab, position, rotation);
        ParticleSystem ps = instance.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            return null;
        }

        float lifetime = definition.lifetimeOverride > 0 ? definition.lifetimeOverride : ps.main.duration;
        StartCoroutine(CleanupAfterDuration(ps, lifetime));

        return ps;
    }

    private IEnumerator CleanupAfterDuration(ParticleSystem ps, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (ps != null)
            Destroy(ps.gameObject);
    }
}
