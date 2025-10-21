using System.Collections.Generic;
using UnityEngine;

public class NPCName
{
    [field: SerializeField] public string Name { get; private set; }
}


[CreateAssetMenu(fileName = "NPCNames", menuName = "Scriptable Objects/NPCNames")]
public class NPCNames
{
    public List<NPCName> NPCNamesList;

    public List<NPCName> Get_Objects()
    {
        return NPCNamesList;
    }
}
