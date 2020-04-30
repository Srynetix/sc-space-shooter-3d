using Godot;

public class Level : Spatial {

    private bool isThirdPerson = false;

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("toggle_view")) {
            var player = GetNode<AnimationPlayer>("AnimationPlayer");
            if (isThirdPerson) {
                player.Play("top-down");
                isThirdPerson = false;
            } else {
                player.Play("third-person");
                isThirdPerson = true;
            }
        }
    }
}
