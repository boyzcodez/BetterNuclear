using Godot;
using System;

[GlobalClass]
public partial class Animation : Resource
{
    [Export] public Texture2D SpriteSheet;

    [Export] public int Vertical;        // Total rows in the sheet
    [Export] public int Horizontal;      // Total columns in the sheet

    [Export] public int CellSize = 64;    // Size of each frame (px)
    [Export] public int FrameRate = 12;
    [Export] public bool Loops = true;

    [Export(PropertyHint.Enum, "OnShoot,OnHit")]
    public string Name { get; set; } = "OnShoot";

    [Export] public int WhichRow = 0;     // Row index (0-based)

    [Export] public int FrameCount = 0;   // Frames to use from this row (0 = use all)
}
