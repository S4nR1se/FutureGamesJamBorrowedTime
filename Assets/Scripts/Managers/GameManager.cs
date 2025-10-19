using System.Collections.Generic;
using UnityEngine;

public class GameManager : StateMachine
{
    public static GameManager Instance { get; private set;}

    private Dictionary<System.Type, Manager> _managers = new Dictionary<System.Type, Manager>(); 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RegisterManagers();
        InitializeManagers();

        RegisterState(new PlayingState());
        SwitchState<PlayingState>();
    }
    private void Update()
    {
        UpdateStateMachine();
        FixedUpdateStateMachine();
    }
    private void RegisterManagers()
    {
        Manager[] foundManagers = GetComponentsInChildren<Manager>();

        foreach(Manager manager in foundManagers)
        {
            System.Type managerType = manager.GetType();
            if (!_managers.ContainsKey(managerType))
            {
                _managers[managerType] = manager;
            }
        }
    }
    private void InitializeManagers()
    {
        foreach (Manager manager in _managers.Values)
        {
            manager.Initialize();
        }
    }
    public T GetManager<T>() where T : Manager
    {
        if(_managers.TryGetValue(typeof(T), out Manager manager))
        {
            return manager as T;
        }
        Debug.LogWarning($"Manager of type {typeof(T).Name} not found!");
        return null;
    }
    public bool HasManager<T>() where T : Manager
    {
        return _managers.ContainsKey(typeof(T));
    }
}
