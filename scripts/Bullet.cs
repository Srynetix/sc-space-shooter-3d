using Godot;

public class Bullet : Area {
    
    private bool active = false;
    private Vector3 velocity;
    private Vector3 angularVelocity;
    private Vector3 direction;
    private float speed = 40.0f;
    private float angularSpeed = 300.0f;
    private bool collided = false;

    #region Lifecycle methods

    public override void _Ready() {
        direction = Vector3.Forward;

        Connect("area_entered", this, nameof(_onCollision));
    }

    public override void _Process(float delta) {
        if (active) {
            HandleMovement(delta);
        }
    }

    #endregion Lifecycle methods

    #region Public methods

    public void Arm(Vector3 position) {
        // GO
        GlobalTransform = new Transform(GlobalTransform.basis, position);
        active = true;
    }

    #endregion Public methods

    #region Private methods

    private void HandleMovement(float delta) {
        velocity = direction * speed;
        angularVelocity = new Vector3(0, 0, 1) * angularSpeed;

        GlobalTranslate(velocity * delta);
        RotationDegrees = RotationDegrees + angularVelocity * delta;

        // Cap
        if (RotationDegrees.z > 360) {
            RotationDegrees = RotationDegrees - new Vector3(0, 0, 360);
        }
    }

    #endregion Private methods

    #region Signal callbacks

    private void _on_VisibilityNotifier_screen_exited() {
        QueueFree();
    }

    private void _onCollision(Node other) {
        if (!collided) {
            if (other.IsInGroup("enemy")) {
                collided = true;
                CallDeferred(nameof(_Fade));
            }
        }
    }

    private void _Fade() {
        // GetNode<MeshInstance>("MeshInstance").QueueFree();
        // GetNode<CollisionShape>("CollisionShape").Disabled = true;
        // GetNode<Particles>("Trail").Emitting = false;
        // GetNode<Particles>("Smoke").QueueFree();

        // await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        QueueFree();
    }

    #endregion Signal callbacks
}
