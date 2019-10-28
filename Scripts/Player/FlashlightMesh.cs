using Godot;
using System;

public class FlashlightMesh : CSGCombiner
{
    Spatial parent;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        parent = (Spatial) GetParent().GetParent().GetParent();
    }

    public override void _Process(float delta)
    {
        Vector3 scale = Scale;
        GlobalTransform = parent.GlobalTransform;
        Scale = scale;
    }
}
