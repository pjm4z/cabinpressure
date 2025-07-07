using Godot;
using System;
using System.Threading.Tasks;

public partial class Post : GridItem
{
	[Export] private PowerGrid grid;
	private Vector2 tilePos;
	public bool isOccupied = false;
	[Export] public Crew assignedCrew;
	private PackedScene torpedoScene;
	private SubViewport underwater;
	public int groupId;
	//private WireCtrl wireCtrl;
	private PostCtrl postCtrl;
	private Sprite2D sprite;
	private Area2D area;
	
	//[Signal]
	//public delegate void RMSelfSignalEventHandler();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		ProcessMode = Node.ProcessModeEnum.Always;
		sprite = (Sprite2D) GetNode("sprite");
		area = (Area2D) GetNode("area");
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (HasOverlappingAreas() == true) {
			isOccupied = true;
		} else {
			isOccupied = false;
		}
		if (this.assignedCrew == null && this.postCtrl != null) {
			reportReadiness();
		}
	}
	
	public bool HasOverlappingAreas() {
		return area.HasOverlappingAreas();
	}
	
	
	public void reportReadiness() { // add connected check
		Post maxReady = postCtrl.getMaxReady();
		if (maxReady != null) {
			if (maxReady.assignedCrew != null) {
				postCtrl.setMaxReady(this);
			}
		} else {
			postCtrl.setMaxReady(this);
		}
	}	
	
	public override void setWireCtrl(WireCtrl wireCtrl) {
		base.setWireCtrl(wireCtrl);
		this.postCtrl = wireCtrl.getPostCtrl();
		if (GetParent() != null) {
			Reparent(wireCtrl.getPostCtrl());
		} else {
			wireCtrl.getPostCtrl().AddChild(this);
		}
		sprite.Modulate =  wireCtrl.color; 
	}
	
	public override void removeSelf() {
		if (this.postCtrl.getMaxReady() == this) {
			this.postCtrl.setMaxReady(null);
		}
		if (this.assignedCrew != null) {
			this.assignedCrew.kickbackOrders();
		}
		//EmitSignal(SignalName.RMSelfSignal);
		base.removeSelf();
	}
	
	public async Task doJob(JobTarget target) {
		//GD.Print("trying to shoot");
		await waitForGameTime(target.taskTime);
		//GD.Print("EXEC");
		if (isOccupied == true && isConnected(target)) {
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
		timer.QueueFree();
	}
}
