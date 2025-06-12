using Godot;
using System;

public partial class Surface : Node2D
{	
	[Export] public NodePath BoatPath;
	private CharacterBody2D boat;
	private ShaderMaterial _shaderMaterial;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		ZIndex = 1;
		boat = GetNode<CharacterBody2D>(BoatPath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
