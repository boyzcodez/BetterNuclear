using Godot;
using System;
using System.Collections.Generic;

public partial class DungeonGenerator : Node, IMapAlgorithm
{
    // Optional tuning (you can leave defaults)
    [Export] public int MaxPlacementAttempts = 40;
    [Export] public int MinCorridorLen = 2;
    [Export] public int MaxCorridorLen = 5;
    [Export] public bool RotateRooms = true;

    private readonly List<AsciiRoomTemplate> _templates = new();

    private sealed class PlacedRoom
    {
        public AsciiRoomTemplate T;
        public Vector2I Origin;
        public bool[] DoorUsed;

        public PlacedRoom(AsciiRoomTemplate t, Vector2I origin)
        {
            T = t;
            Origin = origin;
            DoorUsed = new bool[t.Doors.Count];
        }

        public Vector2I DoorWorldPos(int i) => Origin + T.Doors[i].Pos;
        public Vector2I DoorDir(int i) => T.Doors[i].Dir;
    }

    public MapGenResult Generate(MapData map)
    {
        LoadTemplates(map.AsciiRoomsFolder);

        // fallback if no templates
        if (_templates.Count == 0)
        {
            GD.PushError($"DungeonGenerator: no .txt rooms found in {map.AsciiRoomsFolder}. Returning empty map.");
            var fallback = new Rect2I(-8, -8, 17, 17);
            return new MapGenResult(new HashSet<Vector2I>(), fallback, fallback, Vector2I.Zero);
        }

        var floors = new HashSet<Vector2I>();
        var occupied = new HashSet<Vector2I>(); // floors + solids + corridors
        var blockedForPlacement = new HashSet<Vector2I>(); // occupied + padding (keeps rooms from touching too much)

        var rooms = new List<PlacedRoom>(map.RoomAmount);

        // ---- place first room at origin ----
        var first = RandomTemplateVariant();
        var firstRoom = new PlacedRoom(first, Vector2I.Zero);
        rooms.Add(firstRoom);

        ApplyRoom(firstRoom, floors, occupied, blockedForPlacement);

        Vector2I spawnTile = PickSpawn(firstRoom, floors);

        // ---- place more rooms ----
        for (int r = 1; r < map.RoomAmount; r++)
        {
            bool placed = TryPlaceNextRoom(
                map,
                rooms,
                floors,
                occupied,
                blockedForPlacement,
                linear: map.DungeonLinear
            );

            if (!placed)
            {
                GD.PushWarning($"DungeonGenerator: stopped early at {rooms.Count}/{map.RoomAmount} rooms (no valid placement).");
                break;
            }
        }

        // Bounds should include occupied (so template solids are inside bounds too)
        (Rect2I mapBounds, Rect2I destructionBounds) = ComputeBounds_Tight(map, occupied);

        return new MapGenResult(floors, mapBounds, destructionBounds, spawnTile);
    }

    private void LoadTemplates(string folder)
    {
        _templates.Clear();

        if (string.IsNullOrWhiteSpace(folder))
            return;

        // Godot 4: DirAccess.GetFilesAt works with res://
        string[] files;
        try
        {
            files = DirAccess.GetFilesAt(folder);
        }
        catch (Exception e)
        {
            GD.PushError($"DungeonGenerator: could not list {folder}. {e.Message}");
            return;
        }

        foreach (var file in files)
        {
            if (!file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                continue;

            string path = folder.TrimEnd('/') + "/" + file;

            try
            {
                var t = AsciiRoomTemplate.FromTextFile(path);
                if (t.Doors.Count == 0)
                {
                    GD.PushWarning($"Room '{path}' has no doors (^ v < >). It can still be used as the start room, but won't connect well.");
                }
                _templates.Add(t);
            }
            catch (Exception e)
            {
                GD.PushWarning($"DungeonGenerator: failed parsing '{path}': {e.Message}");
            }
        }
    }

    private AsciiRoomTemplate RandomTemplateVariant()
    {
        var baseT = _templates[GD.RandRange(0, _templates.Count - 1)];
        if (!RotateRooms) return baseT;
        int rot = GD.RandRange(0, 3);
        return baseT.RotatedCW(rot);
    }

    private static void AddToBlockedWithPadding(HashSet<Vector2I> blocked, Vector2I p)
    {
        // 4-neighbor padding keeps rooms from kissing too closely
        blocked.Add(p);
        blocked.Add(p + Vector2I.Up);
        blocked.Add(p + Vector2I.Down);
        blocked.Add(p + Vector2I.Left);
        blocked.Add(p + Vector2I.Right);
    }

    private void ApplyRoom(PlacedRoom room, HashSet<Vector2I> floors, HashSet<Vector2I> occupied, HashSet<Vector2I> blocked)
    {
        // floors
        foreach (var lf in room.T.Floor)
        {
            var wp = room.Origin + lf;
            floors.Add(wp);
            occupied.Add(wp);
        }

        // solids
        foreach (var ls in room.T.Solid)
        {
            var wp = room.Origin + ls;
            occupied.Add(wp);
        }

        // update blocked
        foreach (var wp in WorldOccupied(room))
            AddToBlockedWithPadding(blocked, wp);
    }

    private IEnumerable<Vector2I> WorldOccupied(PlacedRoom room)
    {
        foreach (var lp in room.T.OccupiedLocal())
            yield return room.Origin + lp;
    }

    private Vector2I PickSpawn(PlacedRoom firstRoom, HashSet<Vector2I> floors)
    {
        if (firstRoom.T.SpawnLocal.HasValue)
            return firstRoom.Origin + firstRoom.T.SpawnLocal.Value;

        // otherwise: pick any floor in the first room
        foreach (var lf in firstRoom.T.Floor)
            return firstRoom.Origin + lf;

        // ultra fallback
        foreach (var f in floors)
            return f;

        return Vector2I.Zero;
    }

    private bool TryPlaceNextRoom(
        MapData map,
        List<PlacedRoom> rooms,
        HashSet<Vector2I> floors,
        HashSet<Vector2I> occupied,
        HashSet<Vector2I> blockedForPlacement,
        bool linear
    )
    {
        // choose a parent room:
        // - linear: always the most recent room that still has an unused door
        // - non-linear: random among rooms with unused doors
        List<int> candidateParents = new();
        if (linear)
        {
            for (int i = rooms.Count - 1; i >= 0; i--)
            {
                if (HasFreeDoor(rooms[i]))
                {
                    candidateParents.Add(i);
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < rooms.Count; i++)
                if (HasFreeDoor(rooms[i]))
                    candidateParents.Add(i);
        }

        if (candidateParents.Count == 0)
            return false;

        for (int attempt = 0; attempt < MaxPlacementAttempts; attempt++)
        {
            int parentIdx = candidateParents[GD.RandRange(0, candidateParents.Count - 1)];
            var parent = rooms[parentIdx];

            int parentDoorIdx = PickRandomFreeDoorIndex(parent);
            if (parentDoorIdx < 0) continue;

            Vector2I aPos = parent.DoorWorldPos(parentDoorIdx);
            Vector2I aDir = parent.DoorDir(parentDoorIdx);

            var newT = RandomTemplateVariant();

            // find a door in the new room that faces the parent door (opposite direction)
            int newDoorIdx = PickDoorFacing(newT, -aDir);
            if (newDoorIdx < 0) continue;

            int corridorLen = GD.RandRange(Mathf.Max(2, MinCorridorLen), Mathf.Max(2, MaxCorridorLen));

            Vector2I newDoorWorld = aPos + aDir * corridorLen;
            Vector2I newOrigin = newDoorWorld - newT.Doors[newDoorIdx].Pos;

            // corridor tiles (excluding door tiles themselves)
            var corridor = new List<Vector2I>(corridorLen - 1);
            for (int i = 1; i <= corridorLen - 1; i++)
                corridor.Add(aPos + aDir * i);

            // Check corridor doesn't smash into existing occupied
            bool corridorOk = true;
            foreach (var c in corridor)
            {
                if (occupied.Contains(c))
                {
                    corridorOk = false;
                    break;
                }
            }
            if (!corridorOk) continue;

            // Check new room placement against blocked (occupied + padding)
            bool roomOk = true;
            foreach (var lp in newT.OccupiedLocal())
            {
                Vector2I wp = newOrigin + lp;
                if (blockedForPlacement.Contains(wp))
                {
                    roomOk = false;
                    break;
                }
            }
            if (!roomOk) continue;

            // Commit placement
            var placed = new PlacedRoom(newT, newOrigin);

            // Mark doors used on both sides
            parent.DoorUsed[parentDoorIdx] = true;
            placed.DoorUsed[newDoorIdx] = true;

            // Apply corridor
            foreach (var c in corridor)
            {
                floors.Add(c);
                occupied.Add(c);
                AddToBlockedWithPadding(blockedForPlacement, c);
            }

            // Apply room
            rooms.Add(placed);
            ApplyRoom(placed, floors, occupied, blockedForPlacement);

            return true;
        }

        return false;
    }

    private static bool HasFreeDoor(PlacedRoom r)
    {
        for (int i = 0; i < r.DoorUsed.Length; i++)
            if (!r.DoorUsed[i])
                return true;
        return false;
    }

    private static int PickRandomFreeDoorIndex(PlacedRoom r)
    {
        var free = new List<int>();
        for (int i = 0; i < r.DoorUsed.Length; i++)
            if (!r.DoorUsed[i])
                free.Add(i);

        if (free.Count == 0) return -1;
        return free[GD.RandRange(0, free.Count - 1)];
    }

    private static int PickDoorFacing(AsciiRoomTemplate t, Vector2I neededDir)
    {
        var candidates = new List<int>();
        for (int i = 0; i < t.Doors.Count; i++)
        {
            if (t.Doors[i].Dir == neededDir)
                candidates.Add(i);
        }

        if (candidates.Count == 0) return -1;
        return candidates[GD.RandRange(0, candidates.Count - 1)];
    }

    private (Rect2I mapBounds, Rect2I destructionBounds) ComputeBounds_Tight(MapData map, HashSet<Vector2I> cellSet)
    {
        if (cellSet.Count == 0)
        {
            var fallback = new Rect2I(-8, -8, 17, 17);
            return (fallback, fallback);
        }

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var p in cellSet)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        int pad = map.TightOuterPadding + map.TerrainSafetyMargin;

        Vector2I pos0 = new(minX - pad, minY - pad);
        Vector2I end = new(maxX + pad + 1, maxY + pad + 1); // exclusive
        Rect2I bounds = new Rect2I(pos0, end - pos0);

        int inset = Mathf.Min(map.TightDestructionInset, Mathf.Min(bounds.Size.X / 2, bounds.Size.Y / 2));
        Rect2I destructionBounds = new Rect2I(
            bounds.Position + Vector2I.One * inset,
            bounds.Size - Vector2I.One * inset * 2
        );

        return (bounds, destructionBounds);
    }
}
