using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
	private Boat boat;
	//float RotationDegrees;
	float DesiredRotation;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		boat = (Boat) GetParent();
		SetPhysicsProcess(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		//if (GlobalRotationDegrees > DesiredRotation) {
		//	GlobalRotationDegrees += 0.1f;
		//} else if (GlobalRotationDegrees < DesiredRotation) {
		//	GlobalRotationDegrees -= 0.1f;
		//}
		//GlobalRotationDegrees = boat.GlobalRotationDegrees - 180;
		//Rotation = boat.Rotation;
		//LookAt(new Vector2(-1,-1));
		GD.Print(boat.GlobalRotationDegrees);
		GD.Print(GlobalRotationDegrees);
		GD.Print(DesiredRotation);
		GD.Print();
	}
}
