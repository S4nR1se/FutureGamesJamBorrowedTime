using System.Collections.Generic;
using UnityEngine;
using static SoundManager;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Scriptable Objects/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [SerializeField] private List<SoundDefinition> sounds = new();

    private Dictionary<string, SoundDefinition> _lookup;

    public void Initialize()
    {
        _lookup = new Dictionary<string, SoundDefinition>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var sound in sounds)
        {
            if (!string.IsNullOrWhiteSpace(sound.name) && sound.clip != null)
            {
                _lookup[sound.name] = sound;
            }
        }
    }

    public bool TryGetSound(string name, out SoundDefinition sound)
    {
        if (_lookup == null) Initialize();
        return _lookup.TryGetValue(name, out sound);
    }

#if UNITY_EDITOR
    // (Optional) Validate data automatically in editor
    private void OnValidate()
    {
        Initialize();
    }
#endif
}
