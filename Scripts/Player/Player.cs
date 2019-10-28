using Godot;
using Godot.Collections;
using System;

public class Player : KinematicBody
{
    // Constants
    const string UP = "player_move_forward";
    const string DOWN = "player_move_backward";
    const string LEFT = "player_move_left";
    const string RIGHT = "player_move_right";
    const string FLASHLIGHT = "player_flashlight";
    const string USE = "player_interact";
    const float RAY_LEN = 10;

    public enum State {
        NORMAL,
        READING,
    }

    // Exports
    [Export]
    float sensitivity = 1;
    [Export]
    PackedScene flashlight_prefab;
    [Export]
    float speed = 1;
    [Export]
    float accel = 100;
    [Export]
    float deaccel = 100;
    [Export]
    Vector3 gravity = new Vector3(0, -9.8f, 0);
    [Export]
    NodePath raycast_path;

    // Class Variables
    float relative_mouse = 0;
    Flashlight flashlight;
    Spatial flashlight_reading_target;
    Vector3 linear_velocity = new Vector3();
    RayCast raycast;
    InteractableObject interactable;
    State current_state = State.NORMAL;
    Camera camera;
    Spatial paper_target;
    ushort floors = 0;
    Vector3 old_normal;
    bool wants_to_move = false;

    public override void _Ready()
    {
        raycast = GetNode<RayCast>(raycast_path);
        camera = GetNode<Camera>("Camera");
        flashlight_reading_target = camera.GetNode<Spatial>("FlashlightReadingTarget");
        paper_target = camera.GetNode<Spatial>("PaperTarget");
        Input.SetMouseMode(Input.MouseMode.Captured);
        SpawnFlashlight();
    }

    public override void _Process(float delta)
    {
        switch (current_state) {
            case State.NORMAL:
                NormalProcess(delta);
                break;
            case State.READING:
                ReadingProcess(delta);
                break;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (current_state) {
            case State.NORMAL:
                NormalPhysicsProcess(delta);
                break;
            case State.READING:
                ReadingPhysicsProcess(delta);
                break;
        }
    }

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventMouseMotion) {
            InputEventMouseMotion m = (InputEventMouseMotion) ev;
            relative_mouse += m.Relative.x;
        }
    }

    public void SetState(State to)
    {
        switch (to) {
            case State.NORMAL:
                SetNormal();
                break;
            case State.READING:
                SetReading();
                break;
        }
    }

    public void AddFloor(Node body)
    {
        floors += 1;
    }

    public void SubFloor(Node body)
    {
        floors -= 1;
    }


    // Normal State
    void SetNormal()
    {
        // TODO:
        // Get screen size
        relative_mouse = 0;
        Input.SetMouseMode(Input.MouseMode.Captured);
        flashlight.SetTarget(GetNode<Spatial>("Camera/FlashlightTarget"));
        current_state = State.NORMAL;
    }

    void NormalProcess(float delta)
    {
        RotatePlayer();
        HandleInput(delta);
        if (floors < 1)
            ApplyGravity(delta);
    }

    void NormalPhysicsProcess(float delta)
    {
        HandleRaycast();
        if (floors > 0 && !wants_to_move) {
            linear_velocity = new Vector3(linear_velocity.x, 0, linear_velocity.z);
        }
        if (floors > 0 && GetSlideCount() > 0) {
            KinematicCollision col = GetSlideCollision(0);
            old_normal = col.Normal;
        } 

        linear_velocity = MoveAndSlide(linear_velocity, floorNormal: Vector3.Up, floorMaxAngle: Mathf.Pi * 2);
        if (floors > 0 && GetSlideCount() < 1) {
            //MoveAndCollide(-1 * old_normal);
        }
    }

    void HandleInput(float delta)
    {
        if (Input.IsActionJustPressed(FLASHLIGHT)) {
            flashlight.Toggle();
        }

        float up = Input.GetActionStrength(DOWN) - Input.GetActionStrength(UP);
        float right = Input.GetActionStrength(RIGHT) - Input.GetActionStrength(LEFT);

        if (Mathf.Abs(up) + Mathf.Abs(right) > 0.01) {
            wants_to_move = true;
            Vector3 z = GlobalTransform.basis.z * up; 
            Vector3 x = GlobalTransform.basis.x * right;
            Vector3 vel = (z + x) * accel * delta;
            linear_velocity += vel;
            if (linear_velocity.Length() > speed) {
                // Remove y component, then put it back later.
                float y = linear_velocity.y;
                linear_velocity *= new Vector3(1, 0, 1);

                linear_velocity = linear_velocity.Normalized() * speed;

                linear_velocity += new Vector3(0, y, 0);
            }
        } else {
            // Remove y component, then put it back later.
            wants_to_move = false;
            float y = linear_velocity.y;
            linear_velocity *= new Vector3(1, 0, 1);

            linear_velocity = linear_velocity.LinearInterpolate(Vector3.Zero, Mathf.Clamp(deaccel * delta, 0, 1));

            linear_velocity += new Vector3(0, y, 0);
        }

        if (Input.IsActionJustPressed(USE) && interactable != null) {
            interactable.Use(this);
        }
    }

    void HandleRaycast()
    {
        if (raycast.IsColliding()) {
            interactable = (InteractableObject) ((Area) raycast.GetCollider()).GetParent();
            interactable.SetHighlight(true);
        } else if (interactable != null) {
            interactable.SetHighlight(false);
            interactable = null;
        }
    }

    void ApplyGravity(float delta)
    {
        linear_velocity += gravity * delta;
    }

    void SpawnFlashlight()
    {
        flashlight = (Flashlight) flashlight_prefab.Instance();
        GetParent().CallDeferred("add_child", flashlight);
    }

    void RotatePlayer()
    {
        Rotate(Vector3.Up, -relative_mouse * sensitivity);
        relative_mouse = 0;
    }

    
    // Reading State
    void SetReading()
    {
        Input.SetMouseMode(Input.MouseMode.Hidden);
        flashlight.SetTarget(flashlight_reading_target);
        current_state = State.READING;
    }

    void ReadingProcess(float delta)
    {
        HandleReadingInput();
    }

    void ReadingPhysicsProcess(float delta)
    {
        OrientReadingTarget();
    }

    void HandleReadingInput()
    {
        if (Input.IsActionJustPressed(FLASHLIGHT)) {
            flashlight.Toggle();
        }

        if (Input.IsActionJustPressed(USE)) {
            interactable.Use(this);
            SetState(State.NORMAL);
        }
    }

    void OrientReadingTarget()
    {
        Vector2 mouse_pos = GetViewport().GetMousePosition();
        Vector3 from = camera.ProjectRayOrigin(mouse_pos);
        Vector3 to = from + camera.ProjectRayNormal(mouse_pos) * RAY_LEN;

        PhysicsDirectSpaceState space = GetWorld().DirectSpaceState;
        Dictionary result = space.IntersectRay(from, to, collideWithBodies: false, collideWithAreas: true);
        if (result.Count > 0) {
            Vector3 position = (Vector3) result["position"];
            flashlight_reading_target.GlobalTransform = flashlight_reading_target.GlobalTransform.LookingAt(position, Vector3.Up);
        }
    }

    public Transform GetReadingTargetTransform()
    {
        return paper_target.GlobalTransform;
    }
}
