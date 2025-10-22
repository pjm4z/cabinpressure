using Godot;
using System;
using static System.Math;
using System.Threading.Tasks;

public partial class Weapon : ShipSystem
{
	[Export] private PackedScene torpedoScene;
	public WeaponSlot wpnSlot;
	private Node2D shotPt;
	private Node2D surface;
	public double usedWatts = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		shotPt = (Node2D) GetNode("shotpt");
		panel = (HBoxContainer) GetNode("/root/basescene/hudcanvas/HUD/systems/systemspanel/weaponspanel");
		watts = -500f;
		surface = (Node2D) GetNode("/root/basescene/surface");
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public override void init(PowerGrid grid, Vector2I tilePos, Vector2 localPos) {
		base.init(grid, tilePos, localPos);
		this.ship = grid.getShip();
	}
	
	public void setWpnSlot(WeaponSlot wpnSlot) {
		this.wpnSlot = wpnSlot;
		this.GlobalPosition = wpnSlot.GlobalPosition;
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
	
	public override void execute() { //async Task
		
		_Shoot_Torpedo();
		base.execute(); //await
		//if (circuit.overloaded()) {
		//	GD.Print("BLACKOUT");
	//	} else {
	//	_Shoot_Torpedo();
	//	}
	}
	
	public override void _Process(double delta) {
		base._Process(delta);
		//if (this.powering) {
		//	LookAt(GetGlobalMousePosition());
		//}
	}
	
	public void _Shoot_Torpedo() {
		Torpedo torpedo = (Torpedo)torpedoScene.Instantiate();
		torpedo.init(this.ship, this.ship.LinearVelocity, GetGlobalMousePosition());
		torpedo.GlobalPosition = shotPt.GlobalPosition;
		torpedo.GlobalRotation = shotPt.GlobalRotation;
		surface.AddChild(torpedo);
	}
}
