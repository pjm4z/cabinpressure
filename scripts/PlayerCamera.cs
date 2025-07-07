using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
	[Export] public float ZoomSpeed = 0.1f;
	[Export] public float MinZoom = 0.1f;
	[Export] public float MaxZoom = 1.0f;
	private Boat boat;
	//float RotationDegrees;
	float DesiredRotation;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		boat = (Boat) GetParent();
		//SetPhysicsProcess(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public override void _Input(InputEvent inputEvent) {
				// Check for zoom input actions.
		if (Input.IsActionPressed("scrollup"))
		{
			GD.Print("^^^^^^");
			Zoom = Zoom + new Vector2(0.2f,0.2f);
		}
		if (Input.IsActionPressed("scrolldown"))
		{
			if (Zoom > new Vector2(0.75f,0.75f)) {
				Zoom = Zoom - new Vector2(0.2f,0.2f);
			} else {
				Zoom = new Vector2(0.75f,0.75f);
			}
			GD.Print("vvvvvv");
		}
	}
}
