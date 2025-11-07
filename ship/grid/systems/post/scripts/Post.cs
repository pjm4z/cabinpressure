using Godot;
using System;
using System.Threading.Tasks;

public partial class Post : GridItem
{
	public bool isOccupied = false;
	[Export] public Crew assignedCrew;
	private PackedScene torpedoScene;
	public int groupId;
	private PostCtrl postCtrl;
	protected AnimatedSprite2D sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		ProcessMode = Node.ProcessModeEnum.Always;
		sprite = (AnimatedSprite2D) GetNode("sprite");
		//area = (Area2D) GetNode("area");
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		int section = ((int) (Math.Abs(postCtrl.GlobalRotationDegrees) % 90) / 15);
		if (postCtrl.GlobalRotationDegrees < 0) {
			section = 5 - section; // todo how to get frame count?
		}
		
		/*if (HasOverlappingAreas() == true) {
			isOccupied = true;
			GD.Print("OCC");
		} else {
			isOccupied = false;
		}*/
		if (this.assignedCrew == null && this.postCtrl != null) {
			reportReadiness();
		}
	}
	
	
	public void reportReadiness() {
		Post maxReady = postCtrl.getMaxReady();
		if (maxReady != null) {
			if (maxReady.assignedCrew != null) {
				postCtrl.setMaxReady(this);
			}
		} else {
			postCtrl.setMaxReady(this);
		}
	}
	
	public void setPostCtrl(PostCtrl postCtrl) {
		this.postCtrl = postCtrl;
		this.Reparent(postCtrl);
	}
	
	protected override void reparentNetwork() {
		base.reparentNetwork();
		this.Reparent(this.postCtrl);
	}
	
	public override void setNetwork(Network network) {
		base.setNetwork(network);
		setPostCtrl(network.getPostCtrl());
		if (this.sprite != null) {
			this.sprite.Modulate = network.color; 
		}
	}
	
	public override void removeSelf() {
		if (this.postCtrl.getMaxReady() == this) {
			this.postCtrl.setMaxReady(null);
		}
		if (this.assignedCrew != null) {
			this.assignedCrew.kickbackOrders();
		}
		base.removeSelf();
	}
	
	public void setOccupied(bool occupied) {
		this.isOccupied = occupied;
	}
	
	public void doJob(ShipSystem target) { //async Task
		if (isOccupied == true && sameNetwork(target)) {
			 target.execute(); //await
		}
	}
}
