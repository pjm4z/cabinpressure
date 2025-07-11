using Godot;
using System;

public partial class SeekBed : State
{
	
	private Crew crew;
	
	private Furniture bed;
	
	[Export] private State sleep;
	[Export] private State idle;
	
	public override void ready() {
		base.ready();
		crew = (Crew) base.parent;
	}
	
	public override void enter() {
		bed = crew.bed;
	}
		
	public override State process(double delta) {
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		return seekBed();
	}
	
	private State seekBed() {		// handle case of no food
		crew.move(bed.GlobalPosition);
		if (crew.atLocation(bed)) {			// if at job location, dequeue job
			return sleep;
		} 
		return null;
	}
	
	public override State checkPriorities() {
		if (crew.checkSleep()) {
			return sleep;
		}
		return null;
	}
}
