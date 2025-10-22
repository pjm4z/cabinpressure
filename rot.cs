using Godot;
using System;

public partial class rot : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	
	Vector2 minZoom = new Vector2(0.75f,0.75f);
	Vector2 maxZoom = new Vector2(4f,4f);
	
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GD.Print(Game.Instance.camera.Zoom);
		
		GlobalRotation += 0.01f;
	}
}
