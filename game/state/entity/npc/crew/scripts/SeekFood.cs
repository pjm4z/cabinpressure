using Godot;
using System;

public partial class SeekFood : State
{
	private Crew crew;
	private Item food;
	
	[Export] private State seekBed;
	[Export] private State idle;
	[Export] private State sleep;
	
	public override void ready() {
		base.ready();
		crew = (Crew) base.parent;
	}
	
	public override void enter() {
		food = crew.soughtFood;
	}
		
	public override State process(double delta) {
		if (crew.inventory != null) {
			crew.inventory.use(crew);
			crew.soughtFood = null;
		}
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		return seekFood();
	}
	
	private State seekFood() {		// handle case of no food
		crew.move(food.GlobalPosition);
		if (crew.atLocation(food)) {			// if at job location, dequeue job
			food.pickUp(crew);
		} 
		return null;
	}
	
	public override State checkPriorities() {
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
