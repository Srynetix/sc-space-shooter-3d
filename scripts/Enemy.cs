using Godot;

public class Enemy : Area {

    [Export] public float moveSpeed = 5.0f;
    [Export] public int targetHitCount = 10;

    private bool destroyed;
    private AnimationPlayer animationPlayer;
    private int currentHitCount;

    #region Lifecycle methods 

    public override void _Ready() {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        var notifier = GetNode<VisibilityNotifier>("VisibilityNotifier");
        notifier.Connect("screen_exited", this, nameof(_Disappear));
        Connect("area_entered", this, nameof(_AreaCollision));

        currentHitCount = 0;
        destroyed = false;
    }

    public override void _Process(float delta) {
        if (destroyed)
            return;

        GlobalTranslate(new Vector3(0, 0, moveSpeed * delta));
    }

    #endregion Lifecycle methods

    #region Private method

    private void _Hit() {
        if (destroyed)
            return;

        animationPlayer.Stop();
        animationPlayer.Play("hit");

        currentHitCount += 1;
        if (currentHitCount >= targetHitCount) {
            CallDeferred(nameof(_Destroy));
        }
    }

    async private void _Destroy() {
        if (destroyed)
            return;

        destroyed = true;
        animationPlayer.Play("exploding");
        await ToSignal(animationPlayer, "animation_finished");
        QueueFree();
    }

    private void _Disappear() {
        QueueFree();
    }

    #endregion Private method

    #region Signal callbacks

    private void _AreaCollision(Node other) {
        if (other.IsInGroup("bullet")) {
            CallDeferred(nameof(_Hit));
        }
    }

    #endregion Signal callbacks
}
