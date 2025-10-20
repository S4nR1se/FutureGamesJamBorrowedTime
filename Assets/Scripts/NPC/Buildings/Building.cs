using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    public Vector2Int size { get; private set; } = new Vector2Int(1, 1);
    public int BuildTime { get; protected set; }
    public int MaterialCost { get; protected set; }
    protected Zone AssociatedZone { get; private set; }
    protected ResourceManager ResourceManager { get; private set; }

    public virtual void Initialize()
    {
        ResourceManager = GameManager.Instance?.GetManager<ResourceManager>();

        ZoneMarker marker = GetComponent<ZoneMarker>();
        if (marker != null)
        {
            AssociatedZone = marker.GetZone();
            if (AssociatedZone != null)
            {
                AssociatedZone.NPCEntered += OnNPCEnter;
                AssociatedZone.NPCExited += OnNPCExit;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (AssociatedZone != null)
        {
            AssociatedZone.NPCEntered -= OnNPCEnter;
            AssociatedZone.NPCExited -= OnNPCExit;
        }
    }

    protected abstract void OnNPCEnter(NPC npc);

    protected abstract void OnNPCExit(NPC npc);

    protected IEnumerable<NPC> GetNPCsInBuilding()
    {
        return AssociatedZone?.GetNPCsInZone() ?? new List<NPC>();
    }
}
