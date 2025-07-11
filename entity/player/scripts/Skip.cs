using Godot;
using System;

public partial class Skip : CharacterBody2D
{
	public bool active = false;
	public Camera2D camera;
	public Ship ship;
	//public Surface surface;
	[Export] public float Speed = 100f;
	public Sprite2D sprite;
	
	public override void _Ready() {
		ship = (Ship) GetNode("/root/basescene/surface/ship");
		camera = (Camera2D) GetNode("/root/basescene/surface/ship/playercamera");
		sprite = (Sprite2D) GetNode("sprite"); 
		
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		if (Input.IsActionJustPressed("tab")) {
			if (camera.GetParent() == this) {
				RemoveChild(camera);
				ship.AddChild(camera);
				this.active = false;
				ship.active = true;
				GlobalPosition = new Vector2(ship.Position.X - 40, ship.Position.Y + 20);
			} else if (camera.GetParent() == ship) {
				ship.RemoveChild(camera);
				AddChild(camera);
				this.active = true;
				ship.active = false;
				GlobalPosition = new Vector2(ship.Position.X - 40, ship.Position.Y + 20);
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!active) {
			//
		}
		else { // Handle rotation (turning the ship)
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
