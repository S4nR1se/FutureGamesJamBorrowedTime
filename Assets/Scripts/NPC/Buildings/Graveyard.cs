using UnityEngine;

public class Graveyard : Building
{
    protected override Occupation AssociatedOccupation => new BuilderOccupation(); //PlaceHolder

    private NPCManager _npcManager;
    private ResourceManager _resourceManager;

    private const int SKELETONGRAVECOST = 1;
    private const int SKELETONPURRCOSTWITHGRAVE = 20;
    private const int SKELETONPURRCOSTWITHOUTGRAVE = 60;

    private const int ZOMBIEGRAVECOST = 3;
    private const int ZOMBIEPURRCOSTWITHGRAVE = 70;
    private const int ZOMBIEPURRCOSTWITHOUTGRAVE = 210;

    public override void Initialize()
    {
        _npcManager = GameManager.Instance.GetManager<NPCManager>();
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        base.Initialize();
    }
    protected override void OnNPCEnter(NPC npc)
    {

    }

    protected override void OnNPCExit(NPC npc)
    {

    }
    public override void OnSelect(PlayerInputManager playerInputManager)
    {

    }
    [ContextMenu("SpawnSkeleton")]
    public  void SpawnSkeleton()
    {
        int graves = _resourceManager.GetValue(Resources.Graves);
        int purr = _resourceManager.GetValue(Resources.Purr);

        if (graves >= SKELETONGRAVECOST)
        {
            if(purr > SKELETONPURRCOSTWITHGRAVE)
            {
                _resourceManager.UpdateValue(Resources.Graves, -SKELETONGRAVECOST);
                _resourceManager.UpdateValue(Resources.Purr, -SKELETONPURRCOSTWITHGRAVE);
                SpawnUndead(UndeadType.Skeleton);
            }
            else
            {
                Debug.Log("Not enough PURR");
            }
        }
        else
        {
            if(purr > SKELETONPURRCOSTWITHOUTGRAVE)
            {
                _resourceManager.UpdateValue(Resources.Purr, -SKELETONPURRCOSTWITHOUTGRAVE);
                SpawnUndead(UndeadType.Skeleton);
            }
            else
            {
                Debug.Log("Not enough graves & PURR");
            }
        }
    }

    [ContextMenu("SpawnZombie")]
    public void SpawnZombie()
    {
        int graves = _resourceManager.GetValue(Resources.Graves);
        int purr = _resourceManager.GetValue(Resources.Purr);

        if (graves >= ZOMBIEGRAVECOST)
        {
            if (purr > ZOMBIEPURRCOSTWITHGRAVE)
            {
                _resourceManager.UpdateValue(Resources.Graves, -ZOMBIEGRAVECOST);
                _resourceManager.UpdateValue(Resources.Purr, -ZOMBIEPURRCOSTWITHGRAVE);
                SpawnUndead(UndeadType.Zombie);
            }
            else
            {
                Debug.Log("Not enough PURR");
            }
        }
        else
        {
            if (purr > ZOMBIEPURRCOSTWITHOUTGRAVE)
            {
                _resourceManager.UpdateValue(Resources.Purr, -ZOMBIEPURRCOSTWITHOUTGRAVE);
                SpawnUndead(UndeadType.Zombie);
            }
            else
            {
                Debug.Log("Not enough graves & PURR");
            }
        }
    }
    private void SpawnUndead(UndeadType undeadType)
    {
        if(undeadType == UndeadType.Skeleton)
        {
            _npcManager.SpawnUndead(AssociatedZone, undeadType);
        }
        else
        {
            _npcManager.SpawnUndead(AssociatedZone, undeadType);
        }
    }
}
