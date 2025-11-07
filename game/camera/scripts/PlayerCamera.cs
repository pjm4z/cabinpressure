using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
	//public static PlayerCamera Instance;
	
	[Export] public float ZoomSpeed = 0.1f;
	[Export] public float MinZoom = 0.1f;
	[Export] public float MaxZoom = 1.0f;
	[Export] private Ship ship;
	//float RotationDegrees;
	float DesiredRotation;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ship = (Ship) GetParent();
		Game.Instance.camera = this;
		//Instance = this;
		//Reparent(ship);
		//SetPhysicsProcess(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	[Export] private Vector2 zoomSpeed = new Vector2(0.2f,0.2f);
	public override void _Input(InputEvent inputEvent) {
		if (Input.IsActionPressed("scrollup")) {
			if (Zoom + zoomSpeed < new Vector2(4f,4f)) {
				Zoom = Zoom + zoomSpeed;
			} else {
				Zoom = new Vector2(4f,4f);
			}
			GD.Print("^^^^^^ " + Zoom);
		}
		if (Input.IsActionPressed("scrolldown")) {
			if (Zoom - zoomSpeed != new Vector2(0.1f,0.1f)) {
				Zoom = Zoom - zoomSpeed;
			} else {
				Zoom = Zoom - zoomSpeed * 2f;
			}
			GD.Print("vvvvvv " + Zoom);
		}
	}
}
