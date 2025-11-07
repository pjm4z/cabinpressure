using Godot;
using System;

public partial class SeekJob : CrewState
{
	private Post post;
	private ShipSystem job;
	
	[Export] private CrewState seekFood;
	[Export] private CrewState seekBed;
	[Export] private CrewState idle;
	[Export] private CrewState work;
	[Export] private CrewState sleep;
	
	public override void enter() {
		job = crew.job;
		post = crew.post;
	}
		
	public override State process(double delta) {
		//GD.Print("SEEK JOB " + crew.lastName);
		CrewState newState = checkPriorities();
		if (newState != null) {
			crew.kickbackOrders();
			return newState;
		}
		
		return seekJob();
	}
	
	private CrewState seekJob() {
		// if at job location, dequeue job
		if (!post.sameNetwork(job)) {
			crew.kickbackOrders();
			return idle;
		} 
	//	GD.Print("forever seeking");
		crew.move(post.GlobalPosition);
		if (crew.atLocation(post)) {
			return work;
		}

		return null;
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
		if (!crew.checkSeekJob()) {
			return idle;
		}
		return null;
	}
	
}
