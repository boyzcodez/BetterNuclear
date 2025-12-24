using Godot;
using System;

public partial class Trail : Line2D
{
    private AnimatedSprite2D parent;
    [Export] private int length = 20;
    private Vector2 offset;

    public override void _Ready()
    {
        parent = GetOwner<AnimatedSprite2D>();
        offset = Position;
        TopLevel = true;
    }

    public void Update(Vector2 position)
    {
        AddPoint(position, 0);
        if (GetPointCount() > length) RemovePoint(GetPointCount() - 1);
    }
}
