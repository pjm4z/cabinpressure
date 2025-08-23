using Godot;
using System;

public partial class SeekFood : CrewState
{
	private Item food;
	
	[Export] private CrewState seekBed;
	[Export] private CrewState idle;
	[Export] private CrewState sleep;
	
	
	public override void enter() {
		food = crew.soughtFood;
	}
		
	public override CrewState process(double delta) {
		if (crew.inventory != null) {
			crew.inventory.use(crew);
			crew.soughtFood = null;
		}
		CrewState newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		return seekFood();
	}
	
	private CrewState seekFood() {		// handle case of no food
		crew.move(food.GlobalPosition);
		if (crew.atLocation(food)) {			// if at job location, dequeue job
			food.pickUp(crew);
		} 
		return null;
	}
	
	public override CrewState checkPriorities() {
		if (crew.checkSleep()) {
			return sleep;
		}
		if (crew.checkSeekBed()) {
			return seekBed;
		} if (!crew.checkSeekFood()) {
			return idle;
		}
		return null;
	}
}
