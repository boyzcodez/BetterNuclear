using Godot;
using System.Collections.Generic;

public enum BulletPriority
{
    Trash,
    Normal,
    Important,
    Critical
}

public partial class BulletPool : Node2D
{
    public static BulletPool Instance { get; private set; }

    [Export] public PackedScene BulletScene;
    [Export] public int MaxBullets = 5000;
    [Export] public int Prewarm = 2000;

    // Free bullets ready to use
    private readonly Stack<ModularBullet> _free = new();


    public readonly List<ModularBullet> _activeAll = new();
    private readonly Dictionary<ModularBullet, int> _activeAllIndex = new();
    
    private readonly List<ModularBullet> _activeSnapshot = new();

    // Active bullets by priority (for fast culling)
    private readonly List<ModularBullet>[] _activeByPriority =
    {
        new List<ModularBullet>(256), // Trash
        new List<ModularBullet>(256), // Normal
        new List<ModularBullet>(256), // Important
        new List<ModularBullet>(256), // Critical
    };

    private readonly Dictionary<ModularBullet, int> _priorityIndex = new();

    private int _created;

    public override void _Ready()
    {
        Instance = this;
        AddToGroup("BulletPool");

        int toPrewarm = Mathf.Clamp(Prewarm, 0, MaxBullets);
        for (int i = 0; i < toPrewarm; i++)
            CreateOne();
    }

    public IReadOnlyList<ModularBullet> GetActiveSnapshot()
    {
        _activeSnapshot.Clear();
        _activeSnapshot.AddRange(_activeAll); // copy references only (cheap)
        return _activeSnapshot;
    }
    
    public static ModularBullet Spawn(
        Vector2 position,
        Vector2 velocity,
        IBulletData bulletData
    )
    {
        return Instance?.SpawnInternal(position, velocity, bulletData);
    }

    public ModularBullet SpawnInternal(
        Vector2 position,
        Vector2 velocity,
        IBulletData bulletData
    )
    {
        // Reclaim any bullets that have been Deactivated() by their own logic
        // (lifetime ended, hit something, etc.)
        ReclaimInactiveBullets();

        if (!EnsureFree(bulletData.priority))
            return null;

        var bullet = _free.Pop();

        // Track as active
        AddActive(bullet, bulletData.priority);

        // Activate with YOUR exact signature/fields
        bullet.Activate(
            position,
            velocity,
            bulletData
        );

        return bullet;
    }

    /// <summary>
    /// Manual release if you ever want to call it directly.
    /// (Not required for your current bullet, since we auto-reclaim.)
    /// </summary>
    public void Release(ModularBullet bullet)
    {
        if (!IsInstanceValid(bullet))
            return;

        if (!_activeAllIndex.ContainsKey(bullet))
            return; // already free / not tracked

        // Ensure it's visually/logic inactive
        bullet.Visible = false;
        bullet.Active = false; // your bullet uses a public bool

        RemoveActive(bullet);
        _free.Push(bullet);
    }

    public override void _Process(double delta)
    {
        // Continuous reclaim so you never leak active bullets even if they deactivate in their own Update().
        ReclaimInactiveBullets();
    }

    // ----------------------------
    // Internals
    // ----------------------------

    private void ReclaimInactiveBullets()
    {
        // Scan active list; swap-remove safe loop
        for (int i = _activeAll.Count - 1; i >= 0; i--)
        {
            var b = _activeAll[i];
            if (!IsInstanceValid(b) || !b.Active)
            {
                // If invalid or deactivated, return it to pool
                if (IsInstanceValid(b))
                {
                    b.Visible = false;
                    // optional: clear behaviors to avoid holding references
                    b.Behaviors.Clear();
                }

                RemoveActiveAt(i, b);
                if (IsInstanceValid(b))
                    _free.Push(b);
            }
        }
    }

    private bool EnsureFree(BulletPriority incomingPriority)
    {
        if (_free.Count > 0)
            return true;

        if (_created < MaxBullets)
        {
            CreateOne();
            return _free.Count > 0;
        }

        // Max reached: try to cull something lower than incoming
        if (CullLowerThan(incomingPriority))
            return _free.Count > 0;

        // No room possible (e.g., incoming Trash and everything active is Trash/Critical)
        return false;
    }

    private void CreateOne()
    {
        if (_created >= MaxBullets || BulletScene == null)
            return;

        var bullet = BulletScene.Instantiate<ModularBullet>();
        bullet.Visible = false;

        // Important: add under BulletPool so _Ready() runs and finds Main/BulletPool via group
        AddChild(bullet);

        _created++;
        _free.Push(bullet);
    }

    private bool CullLowerThan(BulletPriority incoming)
    {
        for (int p = 0; p < (int)incoming; p++)
        {
            var list = _activeByPriority[p];
            if (list.Count == 0)
                continue;

            // Take last for O(1)
            var b = list[list.Count - 1];

            // Deactivate it (your bullet will set Active=false)
            b.Deactivate();

            // Immediately reclaim it now (so Spawn can proceed without waiting a frame)
            // (RemoveActive will yank it out of all tracking lists)
            RemoveActive(b);
            _free.Push(b);

            return true;
        }

        return false;
    }

    private void AddActive(ModularBullet bullet, BulletPriority priority)
    {
        // activeAll
        _activeAllIndex[bullet] = _activeAll.Count;
        _activeAll.Add(bullet);

        // activeByPriority
        var plist = _activeByPriority[(int)priority];
        _priorityIndex[bullet] = plist.Count;
        plist.Add(bullet);

        // Track active bullet count
        Eventbus.activeBullets++;
    }

    private void RemoveActive(ModularBullet bullet)
    {
        if (!_activeAllIndex.TryGetValue(bullet, out int allIdx))
            return;

        RemoveActiveAt(allIdx, bullet);
    }

    private void RemoveActiveAt(int allIdx, ModularBullet bullet)
    {
        // Remove from priority bucket (swap-remove)
        int p = (int)bullet.Priority;
        var plist = _activeByPriority[p];

        if (_priorityIndex.TryGetValue(bullet, out int pIdx))
        {
            int pLast = plist.Count - 1;
            var pSwap = plist[pLast];
            plist[pIdx] = pSwap;
            _priorityIndex[pSwap] = pIdx;

            plist.RemoveAt(pLast);
            _priorityIndex.Remove(bullet);
        }

        // Remove from activeAll (swap-remove)
        int allLast = _activeAll.Count - 1;
        var allSwap = _activeAll[allLast];
        _activeAll[allIdx] = allSwap;
        _activeAllIndex[allSwap] = allIdx;

        _activeAll.RemoveAt(allLast);
        _activeAllIndex.Remove(bullet);

        // Track active bullet count
        Eventbus.activeBullets--;
    }
}


