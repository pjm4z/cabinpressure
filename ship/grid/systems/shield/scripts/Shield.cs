using Godot;
using System;

public partial class Shield : ShipSystem
{
	//private Sprite2D sprite;
	private ShieldBarrier barrier;
	private ShieldArea area;
	private GrooveJoint2D joint;
	[Export] public bool enabled;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		watts = -200f;
		sprite = (Sprite2D) GetNode("sprite");
		barrier = (ShieldBarrier) GetNode("barrier");
		area = (ShieldArea) GetNode("area");
		joint = (GrooveJoint2D) GetNode("joint");
		panel = (HBoxContainer) GetNode("/root/basescene/hudcanvas/HUD/systems/systemspanel/enginepanel");
		setEnabled(enabled);
	}
	
	public override void execute() { //async Task
		base.execute();
		setEnabled(true);
	}
	
	public void damage(float damage) {
		this.addLoad(damage);
	}
	
	public void heal(float hp) {
		this.removeLoad(watts);
	}
	
	public override bool canPower() {
		return circuit.canPower(this.watts - this.load);
	}
	
	public void setEnabled(bool enabled) {
		this.enabled = enabled;
		barrier.setEnabled(enabled);
		area.setEnabled(enabled);
	}
	
	public void init(Ship ship) {
		this.ship = ship;
		setEnabled(ship.shieldEnabled);
		barrier.init(this);
		area.init(this);
		joint.GlobalPosition = ship.GlobalPosition;
		joint.SetNodeA(ship.GetPath());
	}
}
