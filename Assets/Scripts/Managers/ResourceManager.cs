using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Manager
{
    public event Action<Dictionary<Resources, int>> OnResourceChange;

    [System.Serializable]
    public class ResourceEntry
    {
        public Resources ResourceType;
        public int Amount;
    }

    [SerializeField]
    private List<ResourceEntry> _resourceList = new List<ResourceEntry>();

    private Dictionary<Resources, int> _resources = new Dictionary<Resources, int>();

    public override void Initialize()
    {
        if (_resources == null)
            _resources = new Dictionary<Resources, int>();

        _resources.Clear();

        foreach (Resources resource in Enum.GetValues(typeof(Resources)))
        {
            int existingAmount = 0;
            var entry = _resourceList.Find(r => r.ResourceType == resource);
            if (entry == null)
            {
                entry = new ResourceEntry { ResourceType = resource, Amount = 100 };
                _resourceList.Add(entry);
            }
            else
            {
                existingAmount = entry.Amount;
            }

            _resources[resource] = existingAmount;
        }

        InitializeResources();
    }
    private void InitializeResources()
    {
        //Code to set initial values
    }
    public void UpdateValue(Resources resource, int amount)
    {
        if (_resources.ContainsKey(resource))
        {
            int updatedValue = _resources[resource] + amount;
            _resources[resource] = Mathf.Max(0, updatedValue);

            OnResourceChange?.Invoke(_resources);

            ResourceEntry entry = _resourceList.Find(r => r.ResourceType == resource);
            if (entry != null)
            {
                entry.Amount = _resources[resource];
            }
        }
        else
        {
            Debug.LogWarning($"Resource {resource} not found in the dictionary.");
        }
    }

    public int GetValue(Resources resource)
    {
        if (_resources.ContainsKey(resource))
        {
            return _resources[resource];
        }
        return 0;
    }
    private void OnValidate()
    {
        foreach (Resources resource in Enum.GetValues(typeof(Resources)))
        {
            if (!_resourceList.Exists(r => r.ResourceType == resource))
            {
                _resourceList.Add(new ResourceEntry { ResourceType = resource, Amount = 0 });
            }
        }
        _resources.Clear();
        foreach (var entry in _resourceList)
        {
            _resources[entry.ResourceType] = entry.Amount;
        }
    }
}

public enum Resources
{
    Materials,
    FoodStock,
    Graves,
    Purr,
    Dread
}