using Godot;
using System;

public partial class SeekJob : State
{
	private Crew crew;
	private Post post;
	private JobTarget job;
	
	[Export] private State seekFood;
	[Export] private State seekBed;
	[Export] private State idle;
	[Export] private State work;
	[Export] private State sleep;
	
	public override void ready() {
		base.ready();
		crew = (Crew) base.parent;
	}
	
	public override void enter() {
		job = crew.job;
		post = crew.post;
	}
		
	public override State process(double delta) {
		//GD.Print("SEEK JOB " + crew.lastName);
		State newState = checkPriorities();
		if (newState != null) {
			crew.kickbackOrders();
			return newState;
		}
		
		return seekJob();
	}
	
	private State seekJob() {
		// if at job location, dequeue job
		if (!post.isConnected(job)) {
			crew.kickbackOrders();
			return idle;
		} 
		crew.move(post.GlobalPosition);
		if (crew.atLocation(post)) {
			return work;
		}

		return null;
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
		if (!crew.checkSeekJob()) {
			return idle;
		}
		return null;
	}
	
}
