using Godot;
using System;

public class InteractableObject : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    MeshInstance highlight;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        highlight = GetNode<MeshInstance>("Mesh/Outline");
        highlight.CallDeferred("set_visible", false);
    }

    public void SetHighlight(bool to)
    {
        highlight.SetVisible(to);
    }

    public virtual void Use(Player player) {}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
