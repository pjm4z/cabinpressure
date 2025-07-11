using Godot;
using System;

public partial class Surface : Node2D
{	
	[Export] public NodePath ShipPath;
	private CharacterBody2D ship;
	private ShaderMaterial _shaderMaterial;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		ZIndex = 1;
		ship = GetNode<CharacterBody2D>(ShipPath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
