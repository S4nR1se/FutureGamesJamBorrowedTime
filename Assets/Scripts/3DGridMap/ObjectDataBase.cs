using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ObjectData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public Vector2Int Size { get; private set; } = Vector2Int.one;
    [field: SerializeField] public GameObject Prefab { get; private set; }

}

[CreateAssetMenu(fileName = "ObjectDataBase", menuName = "Scriptable Objects/ObjectDataBase")]
public class ObjectDataBase : ScriptableObject
{
    public List<ObjectData> ObjectsData;   
    
    public List<ObjectData> Get_Objects()
    {
        return ObjectsData;
    }
}
