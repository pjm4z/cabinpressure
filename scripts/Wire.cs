using Godot;
using System;
using System.Collections.Generic;

public partial class Wire : GridItem
{
	private PowerGrid grid;
	//private WireCtrl wireCtrl;
	private Vector2I tilePos;
	private Sprite2D sprite;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		sprite = (Sprite2D)GetNode("sprite");
	}
	
	public override void setWireCtrl(WireCtrl wireCtrl) {
		base.setWireCtrl(wireCtrl);
		if (GetParent() != null) {
			Reparent(wireCtrl);
		} else {
			wireCtrl.AddChild(this);
		}
		sprite.Modulate =  wireCtrl.color; 
	}
}
