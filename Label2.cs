using Godot;
using System;

public partial class Label2 : Godot.Label
{
	[Export] private Shield shield;
	[Export] private Engine engine;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (shield != null) {
			Text = shield.load.ToString();
		}
		else if (engine != null) {
			Text = engine.circuit.power + " " + engine.circuit.load;
		}
	}
}
