using System.Collections.Generic;
using UnityEngine;

public class NPCManager : Manager
{
    private List<NPC> _activeNPC = new();

    public override void Initialize()
    {
        _activeNPC.Clear();
    }
}
