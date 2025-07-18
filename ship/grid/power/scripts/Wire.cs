using Godot;
using System;
using System.Collections.Generic;

public partial class Wire : GridItem
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		sprite = (Sprite2D)GetNode("sprite");
	}
}
