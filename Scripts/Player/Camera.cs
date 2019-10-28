using Godot;
using System;

public class Camera : Godot.Camera
{
    const float LIMIT = 89;

    [Export]
    float sensitivity = 1;

    float relative_mouse = 0;

    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        RotateCamera();
    }

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventMouseMotion) {
            InputEventMouseMotion m = (InputEventMouseMotion) ev;
            relative_mouse += m.Relative.y;
        }
    }

    void RotateCamera()
    {
        RotationDegrees = new Vector3(Mathf.Clamp(RotationDegrees.x - relative_mouse * sensitivity, -LIMIT, LIMIT), RotationDegrees.y, RotationDegrees.z);
        relative_mouse = 0;
    }
}
