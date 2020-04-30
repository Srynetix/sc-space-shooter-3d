using Godot;

public class Player : Area
{
	private BulletSystem bulletSystem;
	private AudioStreamPlayer3D laserStream;
	private AnimationPlayer animationPlayer;

	private Vector3 velocity = Vector3.Zero;
	private Vector3 angularVelocity = Vector3.Zero;
	private float speed = 40.0f;
	private float dampFactor = 0.55f;
	private Vector3 initialPosition;

	private bool exploded = false;
	private bool respawned = false;

	private Vector3 topLeftLimit;
	private Vector3 bottomRightLimit;

	#region Lifecycle methods

	public override void _Ready() {
		bulletSystem = GetNode<BulletSystem>("BulletSystem");
		laserStream = GetNode<AudioStreamPlayer3D>("LaserStream");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		bulletSystem.container = GetParent();

		bulletSystem.Connect(nameof(BulletSystem.Fired), this, nameof(_OnFired));
		Connect("area_entered", this, nameof(_OnAreaEntered));

		ComputeRectLimits();

		initialPosition = GlobalTransform.origin;
	}

	public override void _Process(float delta) {
		if (exploded) return;

		HandleMovement(delta);
		HandleFire(delta);
	}

	#endregion Lifecycle methods

	#region Private methods

	private void HandleMovement(float delta) {
		var movement = Vector3.Zero;

		if (Input.IsActionPressed("move_left")) {
			movement.x -= 1;
		}

		if (Input.IsActionPressed("move_right")) {
			movement.x += 1;
		}

		if (Input.IsActionPressed("move_forward")) {
			movement.z -= 1;
		}

		if (Input.IsActionPressed("move_backward")) {
			movement.z += 1;
		}

		if (Input.IsActionPressed("move_far")) {
			movement.y += 1;
		}

		if (Input.IsActionPressed("move_near")) {
			movement.y -= 1;
		}

		movement = movement.Normalized();

		if (movement == Vector3.Zero) {
			// Apply damping
			velocity *= dampFactor;
			angularVelocity *= dampFactor;
		} else {
			velocity = movement * speed;
			angularVelocity.z += -movement.x * speed * 2 * delta;
		}

		// Clamping
		velocity.x = Mathf.Clamp(velocity.x, -speed, speed);
		velocity.z = Mathf.Clamp(velocity.z, -speed, speed);
		angularVelocity.z = Mathf.Clamp(angularVelocity.z, -45, 45);

		GlobalTranslate(velocity * delta);
		RotationDegrees = angularVelocity;

		ClampTransform();
	}

	private void HandleFire(float delta) {
		if (Input.IsActionPressed("fire")) {
			bulletSystem.StartFire(bulletSystem.GlobalTransform.origin);
		} else {
			bulletSystem.StopFire();
		}
	}

	private void ComputeRectLimits() {
		var z = 40;
		var viewport = GetViewport();
		var viewportSize = viewport.Size;
		var camera = viewport.GetCamera();
		var targetPosition = camera.UnprojectPosition(GlobalTransform.origin);
		topLeftLimit = camera.ProjectPosition(new Vector2(0, 0), z);
		bottomRightLimit = camera.ProjectPosition(new Vector2(viewportSize.x, viewportSize.y), z);
	}

	private void ClampTransform() {
		var position = GlobalTransform.origin;

		if (position.x < topLeftLimit.x) {
			position.x = topLeftLimit.x;
		} else if (position.x > bottomRightLimit.x) {
			position.x = bottomRightLimit.x;
		}

		if (position.z < topLeftLimit.z) {
			position.z = topLeftLimit.z;
		} else if (position.z > bottomRightLimit.z) {
			position.z = bottomRightLimit.z;
		}

		GlobalTransform = new Transform(GlobalTransform.basis, position);
	}

	async private void Explode() {
		if (exploded) return;

		// Reset movements
		bulletSystem.StopFire();

		exploded = true;
		animationPlayer.Play("exploding");
		await ToSignal(animationPlayer, "animation_finished");

		// Respawn
		CallDeferred(nameof(Respawn));
	}

	async private void Respawn() {
		if (respawned) return;

		// Reset position
		velocity = Vector3.Zero;
		angularVelocity = Vector3.Zero;
		GlobalTransform = new Transform(GlobalTransform.basis, initialPosition);
		RotationDegrees = Vector3.Zero;

		respawned = true;
		animationPlayer.Play("idle");
		await ToSignal(animationPlayer, "animation_finished");

		// TODO: Show respawn animation

		// Reset state
		respawned = false;
		exploded = false;
	}

	#endregion

	#region Signal callbacks

	private void _OnAreaEntered(Node other) {
		if (other.IsInGroup("enemy")) {
			CallDeferred(nameof(Explode));
		}
	}

	private void _OnFired(Bullet bullet) {
		laserStream.Play();
	}

	#endregion
}
