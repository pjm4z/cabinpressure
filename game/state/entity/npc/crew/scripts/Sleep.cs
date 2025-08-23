using Godot;
using System;

public partial class Sleep : CrewState
{
	private Furniture bed;
	private Sprite2D sprite;
	
	[Export] private CrewState idle;
	
	public override void enter() {
		bed = crew.bed;
		sprite = crew.sprite;
		crew.sleeping = true;
	}
	
	public override void exit() {
		crew.sleeping = false;
	}
		
	public override CrewState process(double delta) {
		CrewState newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		return rest();
	}
	
	private CrewState rest() {
		Vector2 sleepSpot = crew.GlobalPosition;
		if (bed != null) {
			sleepSpot = bed.GlobalPosition;
		}
		crew.move(sleepSpot);
		sprite.Rotation = crew.Rotation + 1.5708f;
		return null;
	}
	
	public override CrewState checkPriorities() {
		if (!crew.checkSleep()) {
			return idle;
		}
		return null;
	}
}
