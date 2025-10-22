using Godot;
using System;

public partial class Map : Panel
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public override void _Input(InputEvent inputEvent) {
		if (Input.IsActionJustReleased("m")) { //todo nav doesnt pause (crew in motion)
													// maybe from nav callback in crew script calling movenadslide?
			this.Visible = !this.Visible;
			
		}
	}
}
