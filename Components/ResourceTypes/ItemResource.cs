using Godot;
using System;

[GlobalClass]
public partial class ItemResource : Resource
{
    [Export] public PackedScene itemScene;
    [Export] public string Id;
    [Export] public int AmountOfItem;
}
