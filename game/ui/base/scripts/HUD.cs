using Godot;
using System;

public partial class HUD : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
