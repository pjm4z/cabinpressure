using Godot;
using System;

public partial class Queued : SysState
{
	[Export] private SysState idle;
	[Export] private SysState occupied;
		
	public override State process(double delta) {
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		queued();
		//GD.Print("Q'd");
		return null;
	}
	
	private void queued() {}
	
	public override SysState checkPriorities() {
		if (!sys.shouldQueue()) {
			return idle;
		}
		if (sys.isOccupied()) {
			GD.Print("OCCCCC");
			return occupied;
		}
		return null;
	}
}
