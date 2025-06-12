using Godot;
using System;

public partial class CrewRay : RayCast2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetCollisionMask(2);
		SetCollideWithAreas(true);
		SetCollideWithBodies(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.IsColliding()) {		//CollideWithAreas
				//try {
				//	Crew tml = (Crew) GetCollider();
				//	GD.Print(tml.firstName);
				//	GD.Print(this.GetName());
				//	GD.Print(GetCollisionPoint());
				//	GD.Print();
				//} catch (Exception e) {
				//	GD.Print(GetCollider());
				//	GD.Print();
				//}
		}
	}
}
