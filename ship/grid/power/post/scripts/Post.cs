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
	private Area2D area;
	
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
	
	public async Task doJob(JobTarget target) {
		if (isOccupied == true && sameNetwork(target)) {
			await target.execute();
		}
	}
}
