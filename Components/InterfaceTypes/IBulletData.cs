using Godot;
using System.Collections.Generic;

public struct IBulletData
{
    public Shape2D Shape;
    public float Radius {get; set;}
    public float LifeTime {get;set;}
    public float Speed { get; set; }
    public DamageData damageData;
    public StringName key {get; set;}
    public int CollisionLayer {get;set;}
    public BulletPriority priority;
    
    public IReadOnlyList<BehaviorResource> Behaviors;

    public IBulletData(
        BulletPriority newPrior,
        Shape2D newShape,
        DamageData newDMG, 
        IReadOnlyList<BehaviorResource> newBehaviors,
        float newRadius, 
        float newSpeed, 
        float newLifeTime, 
        int newLayer, 
        StringName newKey)
    {
        priority = newPrior;
        Shape = newShape;
        damageData = newDMG;
        Behaviors = newBehaviors;
        Radius = newRadius;
        Speed = newSpeed;
        LifeTime = newLifeTime;
        CollisionLayer = newLayer;
        key = newKey;
    }
}
