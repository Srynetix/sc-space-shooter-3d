using Godot;
using System;

public class Explosion : Particles
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public void Start() {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("on");
    }
}
