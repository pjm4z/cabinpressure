using Godot;
using System;

public partial class shieldsprite : Sprite2D
{
	//[Export] PhysicsBody2D shield;
	//[Export] private Shield shield;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	//	shield = (Shield) GetSibling("shield");
		//Modulate.A = 0.5f;
		//shield = (PhysicsBody2D) GetNode("PhysicsBody2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//Rotation = shield.Rotation;
		
	}
}
