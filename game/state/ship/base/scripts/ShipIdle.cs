using Godot;
using System;

public partial class ShipIdle : ShipState
{
	[Export] private ShipState player;
	[Export] private ShipState follow;
	
	private Skip skip;
	
	public override void enter() {
		this.skip = ship.skip;
	}
		
	public override State process(double delta) {
		ShipState newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		idle();
		return base.process(delta);
	}
	
	private void idle() {
		
	}
	
	public override ShipState checkPriorities() {
		if (skip != null) {
			return player;
		} else {
			return follow;
		}
	//	return null;
	}
}
