using Godot;
using System;
using System.Threading.Tasks;

public partial class Work : CrewState {
	private Post post;
	private ShipSystem job;
	private CrewProgress crewProgress;
	
	[Export] private CrewState seekFood;
	[Export] private CrewState seekBed;
	[Export] private CrewState idle;
	[Export] private CrewState sleep;
	
	private bool working;
	
	public override void enter() {
		job = crew.job;
		post = crew.post;
		crewProgress = crew.crewProgress;
		working = false;
		//crew.GlobalPosition = post.GlobalPosition;
		
		job.setOccupied(true);
		//job.addCharge();
	}
	
	public override void exit() {
		job.setOccupied(false);
		//job.removeCharge(); // how to terminate async task execute?
	}
		
	public override CrewState process(double delta) {
		CrewState newState = checkPriorities();
		if (newState != null) {
			if (crew.job != null) {
				crew.kickbackOrders();
			}
			return newState;
		}
		crew.move(post.GlobalPosition);
		/*if (!working) {
			work();
		}*/
		return null;
	}
	
	private void work() { //async
		/*if (job.shouldQueue()) {
			working = true;
			await post.doJob(job);
			working = false;
			crewProgress.deltaElapsed = 0;
		} else {
			if (!job.ready()) {
				GD.Print("CANT SHOOT NO POWER");
				job.requestPower(); 
				working = true;
				await Task.Delay(1000);
				working = false;
			}
		}*/
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
		if (!crew.checkWork()) {
			if ((job != null) && (job.count() > 0 || job.getActive() == true)) {
				//GD.Print("!!");
				crew.kickbackOrders();
			} else {
				crew.detachOrders();
			}
			return idle;
		}
		return null;
	}
	
}
