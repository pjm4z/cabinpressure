using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class Executing : SysState
{
	[Export] private SysState idle;
	[Export] private SysState occupied;
	[Export] private SysState overloaded;
	//protected Action
	
	protected double taskTime; 
	protected bool executing = false;
	
	public override void enter() {
		sys.addCharge();
		sys.executing = true;
		taskTime = sys.taskTime;
	}
	
	public override void exit() {
		GD.Print(":)) " + sys.load + " " + sys.watts + " " + sys.circuit.load);
		sys.removeCharge();
		
		sys.executing = false;
	}
		
	public override State process(double delta) {
		State newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		
		if (!executing) {
			execute();//null);
		}
		return base.process(delta);
	}
	
	protected virtual async Task execute() {//Action action) {
		//executing = true;
		//while (checkPriorities() == null && sys.count() == 0) {
		//	await waitForGameTime(0.1, (elapsedTime) => { workCallback(elapsedTime); });
		//}
		//await waitForGameTime(taskTime, (elapsedTime) => { workCallback(elapsedTime); });
		if (sys.count() > 0) {
			executing = true;
			//GD.Print("EXEC");
			await waitForGameTime(taskTime, (elapsedTime) => { workCallback(elapsedTime); });
			sys.execute();//action);
			executing = false;
		}
		//executing = false;
	}
	
	protected virtual void workCallback(double elapsedTime) {}
	
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
		if (sys.overloaded()) {
			return overloaded;
		}
		return null;
	}
}
