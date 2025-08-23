using Godot;
using System;

public partial class SysIdle : SysState
{
	
	[Export] private SysState queued;
	[Export] private SysState occupied;
	[Export] private SysState executing;
	
	private CrewRoster roster;
	
		
	public override SysState process(double delta) {
		SysState newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		idle();
		
		return null;
	}
	
	private void idle() {}
	
	public override SysState checkPriorities() {
		if (sys.shouldQueue()) {
			roster = sys.crewRoster;
			roster.postJob(sys);
			GD.Print("IDLES");
			return queued;
		}
		return null;
	}
}
