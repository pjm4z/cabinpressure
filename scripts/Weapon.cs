using Godot;
using System;
using static System.Math;

public partial class Weapon : JobTarget
{
	public WeaponSlot wpnSlot;
	/*[Export] private Boat ship;
	[Export] public PostCtrl postCtrl;
	[Export] private CrewRoster crewRoster;
	public int queuedOrders = 0;
	private double queueTime = 0;
	public bool posted = false;
	private bool active = false;
	public Crew assignedCrew;
	private Sprite2D sprite;
	Color red = new Color(1.0f,0.0f,0.0f,1.0f);
	Color white = new Color(1.0f,1.0f,1.0f,1.0f);
	private Label label;*/
	
	private PackedScene torpedoScene;
	private bool timing = false;
	
	private Node2D shotPt;
	private SubViewport underwater;
	public HBoxContainer weaponsPanel;
	

	// Called when the node enters the scene tree for the first time.
	public int groupId = 0;
	
	public override void _Ready()
	{
		shotPt = (Node2D) GetNode("shotpt");
		sprite = (Sprite2D) GetNode("sprite");
		
		underwater = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		torpedoScene = GD.Load<PackedScene>("res://scenes/torpedo.tscn");
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public void setWpnSlot(WeaponSlot wpnSlot) {
		this.wpnSlot = wpnSlot;
		this.GlobalPosition = wpnSlot.GlobalPosition;
		this.GlobalRotation = wpnSlot.GlobalRotation;
	}
	
	public override void setName(string name) {
		base.setName(name);
		weaponsPanel = (HBoxContainer) GetNode("/root/basescene/HUD/weaponscontainer/weaponspanel");
		weaponsPanel.AddChild(base.label);
	}


	// move to jt
	//public override void _Process(double delta) {
	//	base._Process(delta);
	//}
	
	
	/*
	public void reportReadiness(double delta) {
		if (this.crewRoster != null) {
			if (crewRoster.nextOrder != null) {
				if (this.queueTime > crewRoster.nextOrder.getTime()) {
					crewRoster.nextOrder = this;
				}
			} else {
				crewRoster.nextOrder = this;
			}
		}
	}*/
	
	// move to jt + override
	public override void removeSelf() {
		if (this.wpnSlot != null) {
			this.wpnSlot.removeWpn();
		}
		base.removeSelf();
	}
	
	// move to jt + override
	public override void execute() {
		base.execute();
		_Shoot_Torpedo();
	}
	
	public void _Shoot_Torpedo() {
		Node2D torpedo = (Node2D)torpedoScene.Instantiate();
		torpedo.GlobalPosition = shotPt.GlobalPosition;
		//torpedo.RotationDegrees = RotationDegreesw - 180;
		torpedo.GlobalRotation = shotPt.GlobalRotation - (float) Math.PI;
		underwater.AddChild(torpedo);
	}	
	
	
	
		/*public double getTime() {
		return this.queueTime;
	}
	
	public void startTiming() {
		timing = true;
	}
	
	public void startTiming(double startTime) {
		timing = true;
		this.queueTime = startTime;
	}
	
	public void pauseTiming() {
		timing = false;
	}
	
	public void stopTiming() {
		this.queueTime = 0f;
		timing = false;
	}*/
}
