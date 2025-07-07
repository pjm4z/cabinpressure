using Godot;
using System;

public partial class Skip : CharacterBody2D
{
	public bool active = false;
	public Camera2D camera;
	public Boat boat;
	//public Surface surface;
	[Export] public float Speed = 100f;
	public Sprite2D sprite;
	
	public override void _Ready() {
		boat = (Boat) GetNode("/root/basescene/surface/boat");
		camera = (Camera2D) GetNode("/root/basescene/surface/boat/playercamera");
		sprite = (Sprite2D) GetNode("sprite"); 
		
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		if (Input.IsActionJustPressed("tab")) {
			if (camera.GetParent() == this) {
				RemoveChild(camera);
				boat.AddChild(camera);
				this.active = false;
				boat.active = true;
				GlobalPosition = new Vector2(boat.Position.X - 40, boat.Position.Y + 20);
			} else if (camera.GetParent() == boat) {
				boat.RemoveChild(camera);
				AddChild(camera);
				this.active = true;
				boat.active = false;
				GlobalPosition = new Vector2(boat.Position.X - 40, boat.Position.Y + 20);
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!active) {
			//
		}
		else { // Handle rotation (turning the boat)
		Vector2 dir = Vector2.Zero;
		// WASD movement
		if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
			dir.Y -= 1;
		}
		if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
			dir.Y += 1;
		}
		if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
			dir.X -= 1;
		}
		if (Input.IsActionPressed("ui_right") || Input.IsActionPressed("d")) {
			dir.X += 1;
		}
		// Normalize to avoid faster diagonal movement, and apply speed
		Velocity = dir.Normalized() * Speed;
		LookAt(GetGlobalMousePosition());
		// Move the player
		MoveAndSlide();
		}
	}
}
