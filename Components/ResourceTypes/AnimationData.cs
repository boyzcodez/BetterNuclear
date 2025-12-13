using Godot;
using System;

[GlobalClass]
public partial class AnimationData : Resource
{
    [Export] public Texture2D SpriteSheet {get;set;}
    [Export] public int HorizontalFrames {get;set;}
    [Export] public int VerticalFrames {get;set;}
    [Export] public int FrameRate {get;set;} = 7;
    [Export] public bool Looping {get;set;} = false;
}
