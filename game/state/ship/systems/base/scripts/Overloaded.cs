using Godot;
using System;

public partial class Overloaded : SysState
{
	[Export] private SysState idle;
	[Export] private SysState executing;
	
	public override void enter() {
		base.enter();
		sys.addCharge();
		load_factor *= 2;
	}
	
	public override void exit() {
		base.exit();
		sys.removeCharge();
	}
		
	public override State process(double delta) {
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		overloaded();
		
		return base.process(delta);
	}
	
	private void overloaded() {
		GD.Print("overloaded " + sys.Name);
	}
	
	public override SysState checkPriorities() {
		if (!sys.shouldQueue() || !sys.isOccupied()) {
			return idle;
		}
		if (!sys.overloaded()) {
			return executing;
		}
		return null;
	}	
}
