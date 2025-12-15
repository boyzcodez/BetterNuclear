using Godot;
using System.Collections.Generic;

public class SpatialGrid
{
    private readonly float cellSize;
    private readonly Dictionary<Vector2I, List<ICollidable>> cells = new();

    public SpatialGrid(float _cellSize)
    {
        cellSize = _cellSize;
    }

    public void Clear()
    {
        cells.Clear();
    }

    private Vector2I WorldToCell(Vector2 pos)
    {
        return new Vector2I(
            Mathf.FloorToInt(pos.X / cellSize),
            Mathf.FloorToInt(pos.Y / cellSize)
        );
    }

    public void Insert(ICollidable obj)
    {
        Vector2I cell = WorldToCell(obj._Position);

        if (!cells.TryGetValue(cell, out var list))
        {
            list = new List<ICollidable>();
            cells[cell] = list;
        }

        list.Add(obj);
    }

    public IEnumerable<ICollidable> QueryNearby(Vector2 pos)
    {
        Vector2I center = WorldToCell(pos);

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector2I cell = center + new Vector2I(x,y);
                if (cells.TryGetValue(cell, out var list))
                {
                    foreach (var obj in list) yield return obj;
                }
            }
        }
    }
}
