using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class Executing : SysState
{
	[Export] private SysState idle;
	[Export] private SysState occupied;
	
	protected double taskTime; 
	protected bool executing = false;
	
	public override void enter() {
		sys.addCharge();
		taskTime = sys.taskTime;
	}
	
	public override void exit() {
		sys.removeCharge();
	}
		
	public override State process(double delta) {
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		
		if (!executing) {
			
			execute();
		}
		return null;
	}
	
	protected virtual async Task execute() {
		executing = true;
		while (checkPriorities() == null && sys.count() == 0) {
			await waitForGameTime(0.1, (elapsedTime) => { workCallback(elapsedTime); });
		}
		await waitForGameTime(taskTime, (elapsedTime) => { workCallback(elapsedTime); });
		if (sys.count() > 0) {
			sys.execute();
		}
		executing = false;
	}
	
	protected virtual void workCallback(double elapsedTime) {
		if (checkPriorities() != null) {
			elapsedTime = taskTime;
		}
	}
	
	public async Task waitForGameTime(double seconds, Action<double> callback) {
		double elapsedTime = 0;
		while (elapsedTime < seconds) {
			callback?.Invoke(elapsedTime);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); 
			elapsedTime += GetProcessDeltaTime(); 
		}
	}
	
	public override SysState checkPriorities() {
		if (!sys.shouldQueue() || !sys.isOccupied()) {
			return idle;
		}
		if (!sys.powered()) {
			return occupied;
		}
		return null;
	}
}
