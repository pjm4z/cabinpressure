using Godot;
using System;
using System.Threading.Tasks;

public partial class Engine : JobTarget
{
	//[Signal]
	//public delegate void PowerRTSignalEventHandler(GridItem item);
	
	private bool powering = false;
	private double usedWatts = 0;
	
	public override void _Ready() {
		base._Ready();
		watts = 1000;
		taskTime = 1;
		panel = (HBoxContainer) GetNode("/root/basescene/HUD/enginecontainer/enginepanel");
	}
	
	public override void fire() {
		if (this.posted == false && this.assignedCrew == null) {
			this.crewRoster.postJob(this);
			this.posted = true;
		} 
		queuedOrders = 1;
	}	
	
	public override async Task execute() { 
		
		while (queuedOrders > 0) {
			//GD.Print("Entering exec " + watts);
			await base.execute();
			if (wireCtrl.needsPower(this.watts) && this.powering == true) {
				queuedOrders = 1;
			}
			//GD.Print("Exiting exec " + watts + " " + queuedOrders);
			//GD.Print();
		}
	}
	
	protected override void workCallback(double elapsedTime) {
		if (!wireCtrl.needsPower(this.watts) || !this.powering) {
			elapsedTime = taskTime;
		}
	}
	
	public override void addCharge() {
		this.powering = true;
		base.addCharge();
	}
	
	public override void removeCharge() {
		this.powering = false;
		base.removeCharge();
	}
	
	public override void removeSelf() {
		removeCharge();
		base.removeSelf();
	}
	
	public override void setWireCtrl(WireCtrl wireCtrl) {
		base.setWireCtrl(wireCtrl);
		wireCtrl.PowerRQSignal += powerRQEvent;
		wireCtrl.addMaxPower(watts);
	}
	
	public bool overloaded() {
		return usedWatts > watts;
	}
	
	private void powerRQEvent(GridItem target) {
		if (!overloaded()) {
			if ((usedWatts - watts) < target.watts) {
				// emit usedW - w
				//EmitSignal(nameof(SignalName.PowerRQSignal), rq);
			} else {
				// emit target.rqw
			}
			fire();
		}
	//	this.usedVolts -= target.watts;
	}
	
	
	//public override void absorb(GridItem item) {}
	
	public override void init(PowerGrid grid, Vector2I tilePos, Vector2 localPos) {
		base.init(grid, tilePos, localPos);
		setName("engine");
	}
}
