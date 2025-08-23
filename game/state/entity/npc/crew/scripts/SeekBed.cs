using Godot;
using System;

public partial class SeekBed : CrewState
{
	private Furniture bed;
	
	[Export] private CrewState sleep;
	[Export] private CrewState idle;
	public override void enter() {
		bed = crew.bed;
	}
		
	public override CrewState process(double delta) {
		CrewState newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		return seekBed();
	}
	
	private CrewState seekBed() {		// handle case of no food
		crew.move(bed.GlobalPosition);
		if (crew.atLocation(bed)) {			// if at job location, dequeue job
			return sleep;
		} 
		return null;
	}
	
	public override CrewState checkPriorities() {
		if (crew.checkSleep()) {
			return sleep;
		}
		return null;
	}
}
