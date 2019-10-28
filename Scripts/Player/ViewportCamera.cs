using Godot;
using System;

public class ViewportCamera : Camera
{
    [Export]
    NodePath camera_path;
    
    Camera camera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        camera = GetNode<Camera>(camera_path);
    }

    public override void _Process(float delta)
    {
        GlobalTransform = camera.GlobalTransform;
    }
}
