using Godot;
using System;
using System.Threading.Tasks;

public partial class Post : Area2D
{
	//public bool active = false;
	public bool isOccupied = false;
	//public bool posted = false;
	//public bool crewAssigned = false;
	public Crew assignedCrew;
	//public int queuedOrders = 0;
	[Export] private PackedScene torpedoScene;
	private SubViewport underwater;
	private Boat boat;
	private CrewRoster crewRoster;
	
	
	//public Weapon target;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Monitoring = true;
		boat = (Boat) GetNode("/root/basescene/surface/boat");
		crewRoster = (CrewRoster) boat.GetNode("crewroster");
		//ProcessMode = Node.ProcessModeEnum.Pausable;
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

		//if (queuedOrders > 0 && assignedCrew == null) { 
		//	crewRoster.assignCrew(this);
		//}
		
		//if ((queuedOrders() > 0 || targetActive() == true) && (posted == false && assignedCrew == null)) {
		//	crewRoster.postJob(this);
		//}
		if (HasOverlappingAreas() == true) {
			isOccupied = true;
		} else {
			isOccupied = false;
		}
	}
	
	public async Task doJob(Weapon target) {
		await waitForGameTime(target.taskTime);
		if (isOccupied == true) {
			target.execute();
		}
	}
	
	public async Task waitForGameTime(double seconds) {
		Timer timer = new Timer();
		timer.WaitTime = seconds;
		timer.OneShot = true;
		timer.ProcessMode = Node.ProcessModeEnum.Pausable;
		AddChild(timer);
		timer.Start();
		await ToSignal(timer, "timeout");
		GD.Print("QF");
		timer.QueueFree();
	}
}
