using Godot;
using System;
using System.Threading.Tasks;

public partial class Powering : Executing
{	
	private Engine eng;
	
	public override void enter() {
		base.enter();
		eng  = (Engine) sys;
	}
	protected override async Task execute() { 
		//GD.Print("POWER");
		await base.execute();
		if (eng.shouldPower() || eng.shouldQueue()) {
			eng.fire();
		}
	}
	
	protected override void workCallback(double elapsedTime) {
		if (!eng.shouldPower() && !eng.shouldQueue()) {
			elapsedTime = taskTime;
		}
		base.workCallback(elapsedTime);
	}
}
