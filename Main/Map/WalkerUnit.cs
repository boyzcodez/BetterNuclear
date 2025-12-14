using Godot;
using System.Collections.Generic;

public partial class WalkerUnit : Node2D
{
    public enum Dirs{LEFT,RIGHT,UP,DOWN}
    private Vector2 floorTile = new Vector2I(0, 0);
    public Godot.Collections.Array<Godot.Vector2I> Ground = [];
    

    public void CalcPaht()
    {
        Ground.Clear();

        var parent = GetParent<WalkerHead>();
        var PathLength = parent.PathLength;

        List<int> PathSteps = new();
        for (int i = 0; i < PathLength; i++)
        {
            var stepsi = GD.RandRange(0, Dirs.GetNames(typeof(Dirs)).Length - 1);
            PathSteps.Add(stepsi);
        }

        Vector2I location = (Vector2I)parent.GlobalPosition;
        TileMapLayer tm = parent.FloorMap;

        foreach (int dir in PathSteps)
        {
            var ModifierDirection = Vector2I.Zero;

            switch (dir)
            {
                case 0:
                    ModifierDirection = Vector2I.Left;
                    break;
                case 1:
                    ModifierDirection = Vector2I.Right;
                    break;
                case 2:
                    ModifierDirection = Vector2I.Up;
                    break;
                case 3:
                    ModifierDirection = Vector2I.Down;
                    break;
            }
            location += ModifierDirection;
            //tm.SetCell(location, 0, new Vector2I(0, 1));
            Ground.Add(location);
        }

    }
}
