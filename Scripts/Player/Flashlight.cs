using Godot;
using System;

public class Flashlight : Spatial
{
    [Export]
    NodePath target_path;

    [Export]
    float lerp_speed = 1;

    [Export]
    bool on = true;

    Spatial target;
    Camera camera;
    Player player;
    Spatial flashlight;
    Spatial flashlight_camera;
    SpotLight light;

    public override void _Ready()
    {
        target = GetNode<Spatial>(target_path);
        camera = (Camera) target.GetParent();
        player = (Player) camera.GetParent();
        flashlight_camera = GetNode<Spatial>("FlashlightCameraOrigin");
        flashlight = flashlight_camera.GetNode<Spatial>("Flashlight");
        light = flashlight.GetNode<SpotLight>("SpotLight");
        light.Visible = on;

        GlobalTransform = GlobalTransform;
    }

    public override void _Process(float delta)
    {
        GlobalTransform = new Transform(GlobalTransform.basis.Quat().Slerp(player.GlobalTransform.basis.Quat(), lerp_speed * delta), GlobalTransform.origin);
        flashlight_camera.Transform = flashlight_camera.Transform.InterpolateWith(camera.Transform, lerp_speed * delta);
        flashlight.Transform = flashlight.Transform.InterpolateWith(target.Transform, lerp_speed * delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        GlobalTransform = new Transform(GlobalTransform.basis, player.GlobalTransform.origin);
    }

    public void SetTarget(Spatial to)
    {
        target = to;
    }

    public void Toggle()
    {
        SetFlashlight(!on);
    }

    void SetFlashlight(bool to)
    {
        on = to;
        light.Visible = to;
        PlayClick();
    }

    void PlayClick()
    {

    }
}
