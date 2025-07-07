using Godot;
using System;

public partial class Sleep : State
{
	
	private Crew crew;
	private Furniture bed;
	private Sprite2D sprite;
	
	[Export] private State idle;
	
	public override void ready() {
		base.ready();
		crew = (Crew) base.parent;
	}
	
	public override void enter() {
		bed = crew.bed;
		sprite = crew.sprite;
		crew.sleeping = true;
	}
	
	public override void exit() {
		crew.sleeping = false;
	}
		
	public override State process(double delta) {
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		return rest();
	}
	
	private State rest() {
		Vector2 sleepSpot = crew.GlobalPosition;
		if (bed != null) {
			sleepSpot = bed.GlobalPosition;
		}
		crew.move(sleepSpot);
		sprite.Rotation = crew.Rotation + 1.5708f;
		return null;
	}
	
	public override State checkPriorities() {
		if (!crew.checkSleep()) {
			return idle;
		}
		return null;
	}
}
