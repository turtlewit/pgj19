using Godot;
using System;

public class Paper : InteractableObject
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    [Export]
    float lerp_speed = 1;

    Transform initial_transform;
    Transform target;
    bool lerp_towards_target = false;
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        base._Ready();
        initial_transform = GlobalTransform;
    }

    public override void Use(Player player)
    {
        if (!lerp_towards_target) {
            SetHighlight(false);
            player.SetState(Player.State.READING);
            target = player.GetReadingTargetTransform();
            lerp_towards_target = true;
        } else {
            SetHighlight(true);
            lerp_towards_target = false;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (lerp_towards_target) {
            GlobalTransform = GlobalTransform.InterpolateWith(target, lerp_speed * delta);
        } else {
            GlobalTransform = GlobalTransform.InterpolateWith(initial_transform, lerp_speed * delta);
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
