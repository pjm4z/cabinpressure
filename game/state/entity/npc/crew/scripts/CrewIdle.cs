using Godot;
using System;

public partial class CrewIdle : CrewState
{
	
	[Export] private CrewState seekFood;
	[Export] private CrewState seekBed;
	[Export] private CrewState seekJob;
	[Export] private CrewState sleep;
	[Export] private CrewState work;
		
	public override CrewState process(double delta) {
		CrewState newState = checkPriorities();
		if (newState != null) {
			return newState;
		} 
		crew.reportReadiness();
		idle();
		return null;
	}
	
	private void idle() {
		if (crew.isNavReady()) {
			crew.move(crew.GetGlobalMousePosition());//new Vector2(-1,-1)
		}
	}
	
	public override CrewState checkPriorities() {
		if (crew.checkSleep()) {
			return sleep;
		}
		if (crew.checkSeekBed()) {
			return seekBed;
		}
		if (crew.checkSeekFood()) {
			return seekFood;
		}
		if (crew.checkSeekJob()) {
			return seekJob;
		}
		if (crew.checkWork()) {
			return work;
		}
		return null;
	}
}
