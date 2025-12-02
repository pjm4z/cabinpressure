using Godot;
using System;
using System.Linq;

public partial class Follow : ShipState
{
	[Export] private ShipState player;
	[Export] private ShipState idle;
	
	private Skip skip;
	private Ship target;
	
	public override void enter() {
		this.skip = ship.skip;
		this.target = ship.target;
	}
		
	public override State process(double delta) {
		ShipState newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		follow();
		return base.process(delta);
	}
	
	private void follow() {
		//GD.Print(target.LinearVelocity + " " +  target.Acceleration + " " + (target.LinearVelocity.X / target.Acceleration));
		ship.move(target.GlobalPosition, 1000f, target.LinearVelocity, target.Acceleration);
	}
	
	public override ShipState checkPriorities() {
		if (skip != null) {
			return player;
		} 
		return null;
	}
}
