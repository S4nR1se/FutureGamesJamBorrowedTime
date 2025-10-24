using System.Collections.Generic;
using UnityEngine;
using static ParticleSystemManager;

[CreateAssetMenu(fileName = "ParticleLibrary", menuName = "Scriptable Objects/ParticleLibrary")]
public class ParticleLibrary : ScriptableObject
{
    [SerializeField] private List<ParticleDefinition> particles = new();

    private Dictionary<string, ParticleDefinition> _lookup;

    public void Initialize()
    {
        _lookup = new Dictionary<string, ParticleDefinition>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var p in particles)
        {
            if (!string.IsNullOrWhiteSpace(p.name) && p.prefab != null)
            {
                _lookup[p.name] = p;
            }
        }
    }

    public bool TryGetParticle(string name, out ParticleDefinition particle)
    {
        if (_lookup == null) Initialize();
        return _lookup.TryGetValue(name, out particle);
    }

#if UNITY_EDITOR
    private void OnValidate() => Initialize();
#endif
}
