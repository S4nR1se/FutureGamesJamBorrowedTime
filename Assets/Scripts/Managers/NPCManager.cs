using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class NPCManager : Manager
{
    [SerializeField] private NPCNames _nPCNames;
    public event Action<Dictionary<Type, List<NPC>>> OnNPCAmountChange;

    public AudioClip SpawnPeasantSFX;
    public AudioClip SpawnSkeletonSFX;
    public AudioClip SpawnZombieSFX;

    private ZoneManager _zoneManager;
    private SoundManager _soundManager;

    private List<NPC> _activeNPCs = new();
    private List<IWorker> _workers = new();
    private Dictionary<Type, List<NPC>> _npcsByType = new();

    private PeasantPool _peasantPool;
    private UndeadPool _undeadPool;

    private NPCScheduler _scheduler;

    public override void Initialize()
    {
        _zoneManager = GameManager.Instance.GetManager<ZoneManager>();
        _soundManager = GameManager.Instance.GetManager<SoundManager>();

        _scheduler = GetComponent<NPCScheduler>();
        if( _scheduler != null)
        {
            TimeManager timeManager = GameManager.Instance.GetManager<TimeManager>();
            _scheduler.Initialize(this, timeManager);
        }

        _peasantPool = GetComponent<PeasantPool>();
        _undeadPool = GetComponent<UndeadPool>();

        _peasantPool.Initialize();
        _undeadPool.Initialize();

        NPC[] existingNPCs = FindObjectsByType<NPC>(FindObjectsSortMode.None);
        foreach (NPC npc in existingNPCs)
        {
            if (npc.gameObject.activeInHierarchy)
            {
                RegisterNPC(npc);
            }
        }
    }

    public void RegisterNPC(NPC npc)
    {
        if (npc == null || _activeNPCs.Contains(npc))
        {
            return;
        }

        _activeNPCs.Add(npc);

        System.Type npcType = npc.GetType();
        if (!_npcsByType.ContainsKey(npcType))
        {
            _npcsByType[npcType] = new List<NPC>();
        }
        _npcsByType[npcType].Add(npc);

        OnNPCAmountChange?.Invoke(_npcsByType);

        if(npc is IWorker worker)
        {
            _workers.Add(worker);
        }
    }

    public void UnregisterNPC(NPC npc)
    {
        if (npc == null) return;

        if (_activeNPCs.Remove(npc))
        {
            System.Type npcType = npc.GetType();
            if (_npcsByType.ContainsKey(npcType))
            {
                _npcsByType[npcType].Remove(npc);
                OnNPCAmountChange?.Invoke(_npcsByType);
                if (_npcsByType[npcType].Count == 0)
                {
                    _npcsByType.Remove(npcType);
                }
            }

            if (npc is IWorker worker)
            {
                _workers.Remove(worker);
            }

            if (npc is Peasant peasant && _peasantPool != null)
            {
                peasant.ReturnToPool(_peasantPool);
            }
            else if (npc is Undead undead && _undeadPool != null)
            {
                undead.ReturnToPool(_undeadPool);
            }
        }
    }

    public Peasant SpawnPeasant()
    {
        IPoolable poolable = _peasantPool.Get();

        Peasant peasant = poolable.PoolableComponent.GetComponent<Peasant>();
        if (peasant != null)
        {
            Zone correctZone = _zoneManager.GetRandomAvailableZone(ZoneType.House);
            if (correctZone == null)
            {
                return null;
            }

            Vector3 position = correctZone.GetRandomPointInZone();
            peasant.transform.position = position;

            UnityEngine.AI.NavMeshAgent agent = peasant.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
            }

            string generatedName = GenerateName(peasant);
            peasant.Initialize(correctZone, generatedName);

            _soundManager.PlaySoundEffect(SpawnPeasantSFX, position);

            RegisterNPC(peasant);
            return peasant;
        }
        return null;
    }
    public Peasant SpawnPeasant(Zone correctZone)
    {
        IPoolable poolable = _peasantPool.Get();

        Peasant peasant = poolable.PoolableComponent.GetComponent<Peasant>();
        if (peasant != null)
        {
            Vector3 position = correctZone.GetRandomPointInZone();
            peasant.transform.position = position;

            UnityEngine.AI.NavMeshAgent agent = peasant.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
            }

            string generatedName = GenerateName(peasant);
            peasant.Initialize(correctZone, generatedName);

            RegisterNPC(peasant);
            return peasant;
        }
        return null;
    }
    public Undead SpawnUndead(Zone correctZone, UndeadType undeadType)
    {
        IPoolable poolable = _undeadPool.Get(undeadType);

        Undead undead = poolable.PoolableComponent.GetComponent<Undead>();
        if (undead != null)
        {
            Vector3 position = correctZone.GetRandomPointInZone();
            undead.transform.position = position;

            UnityEngine.AI.NavMeshAgent agent = undead.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
            }

            string generatedName = GenerateName(undead);
            undead.Initialize(correctZone, generatedName);

            if (undeadType == UndeadType.Skeleton)
                _soundManager.PlaySoundEffect(SpawnSkeletonSFX, position);
            else
                _soundManager.PlaySoundEffect(SpawnZombieSFX, position);

            RegisterNPC(undead);
            return undead;
        }

        return null;
    }
    public void DespawnPeasant(Peasant peasant)
    {
        if (peasant != null)
        {
            UnregisterNPC(peasant);
        }
    }

    public void DespawnUndead(Undead undead)
    {
        if (undead != null)
        {
            UnregisterNPC(undead);
        }
    }

    public List<T> GetNPCsOfType<T>() where T : NPC
    {
        System.Type type = typeof(T);
        if (_npcsByType.ContainsKey(type))
        {
            return _npcsByType[type].Cast<T>().ToList();
        }
        return new List<T>();
    }

    public int GetNPCCountOfType<T>() where T : NPC
    {
        return GetNPCsOfType<T>().Count;
    }

    public List<NPC> GetNPCsInZone(Zone zone)
    {
        if (zone == null) return new List<NPC>();
        return _activeNPCs.Where(npc => npc.GetCurrentZone() == zone).ToList();
    }
    public List<IWorker> GetAllWorkers()
    {
        return new List<IWorker>(_workers);
    }
    public List<NPC> GetAllActiveNPC()
    {
        return _activeNPCs;
    }
    public int GetActivePeasantCount()
    {
        return _peasantPool != null ? _peasantPool.ActiveCount : 0;
    }

    public int GetActiveUndeadCount()
    {
        return _undeadPool != null ? _undeadPool.ActiveCount : 0;
    }

    [ContextMenu("SpawnPeasant")]
    public void TestSpawn()
    {
        Peasant peasant = SpawnPeasant();
    }
    [ContextMenu("KillPeasant")]
    public void TestDespawn()
    {
        List<Peasant> peasants = GetNPCsOfType<Peasant>();

        if (peasants.Count > 0)
        {
            Peasant randomPeasant = peasants[UnityEngine.Random.Range(0, peasants.Count)];
            DespawnPeasant(randomPeasant);
        }
    }
    private string GenerateName(NPC npc)
    {
        if(_nPCNames == null || _nPCNames.GetNPCNameListSize() == 0)
        {
            return "NPC";
        }  

        int NameIndex = UnityEngine.Random.Range(0, _nPCNames.GetNPCNameListSize() - 1);
        string newName =  _nPCNames.Get_Objects()[NameIndex].Name;

        return newName;
    }
}
