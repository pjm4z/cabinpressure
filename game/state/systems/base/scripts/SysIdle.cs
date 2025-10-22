using Godot;
using System;

public partial class SysIdle : SysState
{
	
	[Export] private SysState queued;
	[Export] private SysState occupied;
	[Export] private SysState executing;
	
	private CrewRoster roster;
	
		
	public override State process(double delta) {
	//	GD.Print("idle " + sys.Name);
		SysState newState = checkPriorities();
		
		if (newState != null) {
			return newState;
		}
		idle();
		
		return base.process(delta);
	}
	
	private void idle() {}
	
	public override SysState checkPriorities() {
		if (sys.shouldQueue()) {
			roster = sys.crewRoster;
			roster.postJob(sys);
		//	GD.Print("QUEUING" + sys.Name);
			return queued;
		}
		return null;
	}
}
