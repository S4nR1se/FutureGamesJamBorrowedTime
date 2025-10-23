using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    public static ParticleSystemManager Instance { get; private set; }

    [Header("Pooling Settings")]
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private bool _usePooling = true;

    private Queue<ParticleSystem> _availableParticles = new Queue<ParticleSystem>();
    private GameObject _particleParent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _particleParent = new GameObject("ParticleSystemPool");
        _particleParent.transform.SetParent(transform);

        if (_usePooling)
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreatePooledParticle();
            }
        }
    }

    private ParticleSystem CreatePooledParticle()
    {
        GameObject go = new GameObject("PooledParticle");
        go.transform.SetParent(_particleParent.transform);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.playOnAwake = false;

        go.SetActive(false);
        _availableParticles.Enqueue(ps);
        return ps;
    }

    private ParticleSystem GetParticleFromPool()
    {
        if (_availableParticles.Count > 0)
        {
            ParticleSystem ps = _availableParticles.Dequeue();
            if (ps != null)
                return ps;
        }

        return _usePooling ? CreatePooledParticle() : null;
    }
    public ParticleSystem SpawnParticle(ParticleSystem prefab, Vector3 position, Quaternion rotation = default, float durationOverride = -1f)
    {
        if (prefab == null) return null;

        ParticleSystem psInstance;

        if (_usePooling)
        {
            psInstance = GetParticleFromPool();
            if (psInstance == null) return null;

            var main = psInstance.main;
            main = prefab.main;
        }
        else
        {
            psInstance = Instantiate(prefab, position, rotation);
        }

        psInstance.gameObject.SetActive(true);
        psInstance.transform.position = position;
        psInstance.transform.rotation = rotation;
        psInstance.Play();

        float duration = durationOverride > 0 ? durationOverride : psInstance.main.duration;
        StartCoroutine(CleanupAfterDuration(psInstance, duration));

        return psInstance;
    }

    private IEnumerator CleanupAfterDuration(ParticleSystem ps, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_usePooling)
        {
            ps.Stop();
            ps.Clear();
            ps.gameObject.SetActive(false);
            _availableParticles.Enqueue(ps);
        }
        else
        {
            Destroy(ps.gameObject);
        }
    }
}
