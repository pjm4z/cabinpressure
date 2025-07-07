using Godot;
using System;
using System.Threading.Tasks;

public partial class Work : State
{
	private Crew crew;
	private Post post;
	private JobTarget job;
	private CrewProgress crewProgress;
	
	[Export] private State seekFood;
	[Export] private State seekBed;
	[Export] private State idle;
	[Export] private State sleep;
	
	private bool working;
	
	public override void ready() {
		base.ready();
		crew = (Crew) base.parent;
	}
	
	public override void enter() {
		job = crew.job;
		post = crew.post;
		crewProgress = crew.crewProgress;
		working = false;
	}
		
	public override State process(double delta) {
		State newState = checkPriorities();
		if (newState != null) {
			crew.kickbackOrders();
			GD.Print("NEW STATE");
			return newState;
		}
		if (!post.isConnected(job)) {
			crew.kickbackOrders();
			GD.Print("NOT CONNECTED");
			return idle;
		}
		//GD.Print();
		if (!working) {
			work();
		}
		return null;
	}
	
	private async void work() {		
		if (job.count() > 0) {
			working = true;
			await post.doJob(job);
			working = false;
			crewProgress.deltaElapsed = 0;
		} else {
			await Task.Delay(1);
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
		if (!crew.checkWork()) {
			if ((job != null) && (job.count() > 0 || job.getActive() == true)) {
				crew.kickbackOrders();
			} else {
				crew.detachOrders();
			}
			return idle;
		}
		return null;
	}
	
}
