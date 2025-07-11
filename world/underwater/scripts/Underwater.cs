using Godot;
using System;

public partial class Underwater : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		ZIndex = -1;
		ProcessMode = Node.ProcessModeEnum.Pausable;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
