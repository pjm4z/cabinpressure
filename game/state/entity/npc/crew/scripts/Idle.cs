using Godot;
using System;

public partial class Idle : State
{
	private Crew crew;
	
	[Export] private State seekFood;
	[Export] private State seekBed;
	[Export] private State seekJob;
	[Export] private State sleep;
	[Export] private State work;
	
	public override void ready() {
		base.ready();
		crew = (Crew) base.parent;
	}
		
	public override State process(double delta) {
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		} 
		crew.reportReadiness();
		idle();
		return null;
	}
	
	private void idle() {
		if (crew.isNavReady()) {
			crew.move(new Vector2(-1,-1));
		}
	}
	
	public override State checkPriorities() {
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
