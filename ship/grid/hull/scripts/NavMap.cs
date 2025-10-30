using Godot;
using System;

public partial class NavMap : ShipLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		TopLevel = true;
		GlobalPosition = Game.Instance.zero;
		base._Ready();
	}
}
