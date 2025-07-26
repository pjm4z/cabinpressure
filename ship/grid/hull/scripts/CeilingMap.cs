using Godot;
using System;

public partial class CeilingMap : ShipLayer
{
	public override void _Input(InputEvent inputEvent) {
		if (Input.IsActionJustPressed("c")) { 
			Visible = !Visible;
		}
	}
}
