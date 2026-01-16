using Godot;
using System;

public partial class WheelSlot : Node
{
    public readonly int Index;
    public readonly float StartAngle;  // radians
    public readonly float EndAngle;    // radians
    public readonly float MidAngle;    // radians
    public readonly float Weight;      // optional (for future)
    public WheelSlot(int index, float start, float end, float weight = 1f)
    {
        Index = index;
        StartAngle = start;
        EndAngle = end;
        MidAngle = (start + end) * 0.5f;
        Weight = weight;
    }
}
