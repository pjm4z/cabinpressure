using Godot;
using System;
using System.Threading.Tasks;

public partial class Post : GridItem
{
	public bool isOccupied = false;
	[Export] public Crew assignedCrew;
	private PackedScene torpedoScene;
	private SubViewport underwater;
	public int groupId;
	private PostCtrl postCtrl;
	//private Area2D area;
	protected Sprite2D sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		ProcessMode = Node.ProcessModeEnum.Always;
		sprite = (Sprite2D) GetNode("sprite");
		//area = (Area2D) GetNode("area");
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	//	GD.Print("! " + postCtrl.GlobalRotationDegrees);
		//GlobalRotation = 0;
		
		/*if (postCtrl.GlobalRotationDegrees > -7.5 && postCtrl.GlobalRotationDegrees <= 82.5) {
			GlobalRotationDegrees = -90;
		} else if (postCtrl.GlobalRotationDegrees > 82.5) { // && postCtrl.GlobalRotationDegrees <= 172.5
			GlobalRotationDegrees = 0;
		} else if (postCtrl.GlobalRotationDegrees < -7.5 && postCtrl.GlobalRotationDegrees >= -82.5) {
			GlobalRotationDegrees = 180;
		} else if (postCtrl.GlobalRotationDegrees < -82.5 ) { //&& postCtrl.GlobalRotationDegrees >= -172.5
			GlobalRotationDegrees = 90;
		}*/
		
		/*if (postCtrl.GlobalRotationDegrees > 0 && postCtrl.GlobalRotationDegrees <= 90) {
			GlobalRotationDegrees = -90;
		} else if (postCtrl.GlobalRotationDegrees > 90 && postCtrl.GlobalRotationDegrees <= 180) { 
			GlobalRotationDegrees = 0;
		} else if (postCtrl.GlobalRotationDegrees < 0 && postCtrl.GlobalRotationDegrees >= -90) {
			GlobalRotationDegrees = 180;
		} else if (postCtrl.GlobalRotationDegrees < -90 && postCtrl.GlobalRotationDegrees >= -180) { 
			GlobalRotationDegrees = 90;
		}*/
		int section = ((int) (Math.Abs(postCtrl.GlobalRotationDegrees) % 90) / 15);
		if (postCtrl.GlobalRotationDegrees < 0) {
			section = 5 - section; // todo how to get frame count?
		}
		//sprite.Frame = section;
		
		
		
		
		 /*else if (postCtrl.GlobalRotationDegrees > 172.5 || postCtrl.GlobalRotationDegrees <= -82.5) { 
			GlobalRotationDegrees = 90;
		} else if (postCtrl.GlobalRotationDegrees > 262.5 && postCtrl.GlobalRotationDegrees <= 352.5) { 
			GlobalRotationDegrees = 180;
		}*/
		
		/*if (postCtrl.GlobalRotationDegrees >= -7.5 && postCtrl.GlobalRotationDegrees <= 7.5) {
			sprite.Frame = 0;
			GlobalRotationDegrees = -90;
		}
		if (postCtrl.GlobalRotationDegrees >= 7.5 && postCtrl.GlobalRotationDegrees <= 22.5) {
			sprite.Frame = 1;
			GlobalRotationDegrees = -90;
		}
		if (postCtrl.GlobalRotationDegrees >= 22.5 && postCtrl.GlobalRotationDegrees <= 37.5) {
			sprite.Frame = 2;
			GlobalRotationDegrees = -90;
		}
		if (postCtrl.GlobalRotationDegrees >= 37.5 && postCtrl.GlobalRotationDegrees <= 52.5) {
			sprite.Frame = 3;
			GlobalRotationDegrees = -90;
		}
		if (postCtrl.GlobalRotationDegrees >= 52.5 && postCtrl.GlobalRotationDegrees <= 67.5) {
			sprite.Frame = 4;
			GlobalRotationDegrees = -90;
		}
		if (postCtrl.GlobalRotationDegrees >= 67.5 && postCtrl.GlobalRotationDegrees <= 82.5) {
			sprite.Frame = 5;
			GlobalRotationDegrees = -90;
		}*/
		
		if (HasOverlappingAreas() == true) {
			isOccupied = true;
		} else {
			isOccupied = false;
		}
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
	
	public void doJob(ShipSystem target) { //async Task
		if (isOccupied == true && sameNetwork(target)) {
			 target.execute(); //await
		}
	}
}
