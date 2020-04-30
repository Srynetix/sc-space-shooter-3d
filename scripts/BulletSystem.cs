using Godot;

public class BulletSystem : Spatial {

    [Signal] public delegate void Fired(Bullet bullet);
    
    [Export] public PackedScene bulletScene = (PackedScene)GD.Load("res://objects/Bullet.tscn");
    [Export] public Node container = null;
    [Export] public float fireSpeed = 0.05f;

    private Timer timer;
    private Vector3 muzzlePosition;
    private bool shouldFire;

    #region Lifecycle methods

    public override void _Ready() {
        // If container is not declared, use parent
        if (container == null) {
            container = GetParent();
        }

        timer = GetNode<Timer>("Timer");
        timer.WaitTime = fireSpeed;
        shouldFire = false;

        // Connect
        timer.Connect("timeout", this, nameof(_ProcessAutofire));
    }

    #endregion Lifecycle methods

    #region Public methods

    public void StartFire(Vector3 position) {
        muzzlePosition = position;
        shouldFire = true;
    }

    public void StopFire() {
        shouldFire = false;
    }

    #endregion Public methods

    #region Private methods

    private void _Fire() {
        // Spawn
        var instance = (Bullet)bulletScene.Instance();
        container.AddChild(instance);

        // Fire
        instance.Arm(muzzlePosition);

        EmitSignal(nameof(Fired), instance);
    }

    #endregion Private methods

    #region Signal callbacks

    private void _ProcessAutofire() {
        // Should fire ?
        if (shouldFire) {
            _Fire();
        }

        // Restart timer
        timer.Start();
    }

    #endregion Signal callbacks
}
