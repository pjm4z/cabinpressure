using Godot;
using System;

public partial class Occupied : SysState
{
	[Export] private SysState idle;
	[Export] private SysState executing;
		
	public override State process(double delta) {
		
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		occupied();
		return base.process(delta);
	}
	
	private void occupied() {}
	
	public override SysState checkPriorities() {
		if (!sys.shouldQueue() || !sys.isOccupied()) {
			return idle;
		}
		if (sys.canPower()) {
			return executing;
		}
		
		return null;
	}	
}
