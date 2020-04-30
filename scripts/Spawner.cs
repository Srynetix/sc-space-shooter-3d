using Godot;

public class Spawner : Spatial {

    [Export] public Node container;
    [Export] public PackedScene source;
    [Export] public Vector3 randomOffset = Vector3.Zero;
    [Export] public float spawnCountPerMinute = 10.0f;

    private Timer timer;

    #region Lifecycle methods

    public override void _Ready() {
        if (container == null) {
            container = GetParent();
        }

        timer = GetNode<Timer>("Timer");
        timer.Connect("timeout", this, nameof(Spawn));
        
        timer.WaitTime = 1.0f / (spawnCountPerMinute / 60.0f);
        timer.Start();

        CallDeferred(nameof(Spawn));
    }

    #endregion Lifecycle methods

    #region Public methods

    public Spatial Spawn() {
        var instance = (Spatial)source.Instance();
        container.AddChild(instance);

        instance.GlobalTransform = new Transform(instance.GlobalTransform.basis, _GenerateSpawnPosition());
        return instance;
    }

    #endregion Public methods

    #region Private methods

    private Vector3 _GenerateSpawnPosition() {
        var rX = (float)GD.RandRange(-randomOffset.x, randomOffset.x);
        var rY = (float)GD.RandRange(-randomOffset.y, randomOffset.y);
        var rZ = (float)GD.RandRange(-randomOffset.y, randomOffset.z);

        return GlobalTransform.origin + new Vector3(rX, rY, rZ);
    }

    #endregion Private methods

    #region Signal callbacks

    #endregion Signal callbacks
}
