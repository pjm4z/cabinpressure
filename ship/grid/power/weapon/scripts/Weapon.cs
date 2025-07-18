using Godot;
using System;
using static System.Math;
using System.Threading.Tasks;

public partial class Weapon : JobTarget
{
	[Export] private PackedScene torpedoScene;
	public WeaponSlot wpnSlot;
	private Node2D shotPt;
	private SubViewport underwater;
	public double usedWatts = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		shotPt = (Node2D) GetNode("shotpt");
		panel = (HBoxContainer) GetNode("/root/basescene/HUD/weaponscontainer/weaponspanel");
		watts = -500f;
		underwater = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public void setWpnSlot(WeaponSlot wpnSlot) {
		this.wpnSlot = wpnSlot;
		this.GlobalPosition = wpnSlot.GlobalPosition;
		this.GlobalRotation = wpnSlot.GlobalRotation;
	}
	
	protected override void reparentNetwork() {
		base.reparentNetwork();
		this.wpnSlot.Reparent(this.postCtrl);
		Reparent(this.wpnSlot);
	}
	
	public override void setNetwork(Network network) {
		base.setNetwork(network);
		this.wpnSlot.Reparent(this.postCtrl);
		Reparent(this.wpnSlot);
	}
	
	public override void setPostCtrl(PostCtrl postCtrl) {
		base.setPostCtrl(postCtrl);
		this.postCtrl = postCtrl;
		this.wpnSlot.Reparent(this.postCtrl);
		Reparent(this.wpnSlot);
	}
	
	public override void removeSelf() {
		if (this.wpnSlot != null) {
			this.wpnSlot.removeWpn();
		}
		if (this.postCtrl != null) {
			this.postCtrl.removeJob(this);
		}
		base.removeSelf();
	}
	
	public override async Task execute() {
		await base.execute();
		if (circuit.overloaded()) {
			GD.Print("BLACKOUT");
		} else {
			_Shoot_Torpedo();
		}
	}
	
	public void _Shoot_Torpedo() {
		Node2D torpedo = (Node2D)torpedoScene.Instantiate();
		torpedo.GlobalPosition = shotPt.GlobalPosition;
		torpedo.GlobalRotation = shotPt.GlobalRotation - (float) Math.PI;
		underwater.AddChild(torpedo);
	}
}
