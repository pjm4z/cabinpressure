using Godot;
using System;
using static System.Math;
using System.Threading.Tasks;

public partial class Weapon : JobTarget
{
	public WeaponSlot wpnSlot;
	[Export] private PackedScene torpedoScene;
	private Node2D shotPt;
	private SubViewport underwater;
	
	public double usedWatts = 0;

	// Called when the node enters the scene tree for the first time.
	public int groupId = 0;
	
	public override void _Ready()
	{
		base._Ready();
		shotPt = (Node2D) GetNode("shotpt");
		panel = (HBoxContainer) GetNode("/root/basescene/HUD/weaponscontainer/weaponspanel");
		watts = -500f;
		underwater = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		//torpedoScene = GD.Load<PackedScene>("res://scenes/torpedo.tscn");
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public void setWpnSlot(WeaponSlot wpnSlot) {
		this.wpnSlot = wpnSlot;
		this.GlobalPosition = wpnSlot.GlobalPosition;
		this.GlobalRotation = wpnSlot.GlobalRotation;
	}
	
	public override void removeSelf() {
		if (this.wpnSlot != null) {
			this.wpnSlot.removeWpn();
		}
		base.removeSelf();
	}
	
	public override async Task execute() {
		
		await base.execute();
		
		if (wireCtrl.overloaded()) {
			GD.Print("BLACKOUT");
		} else {
			_Shoot_Torpedo();
		}
	}
	
	public void _Shoot_Torpedo() {
		Node2D torpedo = (Node2D)torpedoScene.Instantiate();
		torpedo.GlobalPosition = shotPt.GlobalPosition;
		//torpedo.RotationDegrees = RotationDegreesw - 180;
		torpedo.GlobalRotation = shotPt.GlobalRotation - (float) Math.PI;
		underwater.AddChild(torpedo);
	}
}
