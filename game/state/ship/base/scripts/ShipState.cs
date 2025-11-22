using Godot;
using System;

[GlobalClass]
public partial class ShipState : State
{
	protected Ship ship;
	
	public override void ready() {
		base.ready();
		ship = (Ship) base.parent;
	}
	
	public override State process(double delta) {
		return base.process(delta);
	}
}
