using Godot;
using System;

public partial class animrot : AnimatedSprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	
	Vector2 z0 = new Vector2(1.5f,1.5f);
	Vector2 z1 = new Vector2(2.5f,2.5f);
	Vector2 z2 = new Vector2(3.25f,3.25f);
	
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		Vector2 zoom = Game.Instance.camera.Zoom;
		if (zoom <= z0) {
			Frame = 0;
		} else if (zoom <= z1) {
			Frame = 1;
		} else if (zoom <= z2) {
			Frame = 2;
		}
	//	Frame = 0;
		//GD.Print(zoom + " " + Frame);
		//GlobalRotation += 0.01f;
	}
}
