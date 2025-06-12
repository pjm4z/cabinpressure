using Godot;
using System;

public partial class Skip : CharacterBody2D
{
	public bool active = false;
	public Camera2D camera;
	public Boat boat;
	public SurfaceMap surfaceMap;
	//public Surface surface;
	[Export] public float Speed = 100f;
	public Texture2D sprite;
	
	public override void _Ready() {
		boat = (Boat) GetNode("/root/basescene/surface/boat");
		camera = (Camera2D) GetNode("/root/basescene/surface/boat/playercamera");
		surfaceMap = GetNode<SurfaceMap>("/root/basescene/surface/surfaceviewport/surfacemap");
		//surface = GetNode<Surface>("/root/basescene/surface");
		//material = GD.Load<ShaderMaterial>("res://shaders/materials/spritemapmaterial.tres");
		
		sprite = (Texture2D)GD.Load("res://assets/skip.png"); //"res://assets/spriteTest.png");
		//spriteMap = (Texture2D)GD.Load("res://assets/mapTest.png");
		//material.SetShaderParameter("sprite", sprite);
		//material.SetShaderParameter("spriteMap", spriteMap);
		
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		if (Input.IsActionPressed("tab"))
		{
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
		Vector2 velocity = Vector2.Zero;
		// WASD movement
		if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
			velocity.Y -= 1;
		}
		if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
			velocity.Y += 1;
		}
		if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
			velocity.X -= 1;
		}
		if (Input.IsActionPressed("ui_right") || Input.IsActionPressed("d")) {
			velocity.X += 1;
		}

		// Normalize to avoid faster diagonal movement, and apply speed
		Velocity = velocity.Normalized() * Speed;

		// Move the player
		MoveAndSlide();
		}
	}
}
