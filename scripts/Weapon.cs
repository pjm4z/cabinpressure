using Godot;
using System;
using static System.Math;

public partial class Weapon : Area2D
{
	public Area2D wpnSlot;
	public PostCtrl postCtrl;
	private CrewRoster crewRoster;
	public int queuedOrders = 0;
	public bool posted = false;
	public bool active = false;
	public double taskTime = 1;
	private Node2D shotPt;
	private SubViewport underwater;
	private PackedScene torpedoScene;
	public Crew assignedCrew;
	public HBoxContainer weaponsPanel;
	public Label label;
	
	Color red = new Color(1.0f,0.0f,0.0f,1.0f);
	Color white = new Color(1.0f,1.0f,1.0f,1.0f);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		wpnSlot = (Area2D) GetParent();
		shotPt = (Node2D) GetNode("shotpt");
		postCtrl = (PostCtrl) wpnSlot.GetParent().GetNode("postctrl");
		crewRoster = (CrewRoster) wpnSlot.GetParent().GetNode("crewroster");
		underwater = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		torpedoScene = GD.Load<PackedScene>("res://scenes/torpedo.tscn");
		
		label = new Label();
		weaponsPanel = (HBoxContainer) GetNode("/root/basescene/HUD/weaponscontainer/weaponspanel");
		label.Text = this.Name;
		weaponsPanel.AddChild(label);
		
		this.GlobalPosition = wpnSlot.GlobalPosition;
		this.GlobalRotation = wpnSlot.GlobalRotation;
		ProcessMode = Node.ProcessModeEnum.Always;
		//ProcessMode = Node.ProcessModeEnum.Pausable;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (active == true && queuedOrders == 0) {
			GD.Print(label.Text + " " + posted + " " + (assignedCrew == null));
			
			// --> POSTED IS REMAINING TRUE, but if posted shouldnt active be false? can only set active if q empty
		}
		
		label.Text = this.Name + " " + this.queuedOrders;
		if (active == true) {
			label.Set("theme_override_colors/font_color",red);
		} else {
			label.Set("theme_override_colors/font_color",white);
		}
		//GD.Print("(" + (this.queuedOrders > 0) + " || " + (this.active == true) + ") && (" + (this.posted == false) + " && " + (this.assignedCrew == null) + ")");
		if ((queuedOrders > 0 || active == true) && (posted == false && assignedCrew == null)) {
			//consoleCtrl.weaponQueue.Enqueue(this);
			
			crewRoster.postJob(this);
			this.posted = true;
		}
	}
	
	public void fire() {
		if (this.posted == false && this.assignedCrew == null) {
			GD.Print(":0000000000");
			crewRoster.postJob(this);
			this.posted = true;
		}
		queuedOrders += 1;
	}
	
	public void execute() {
		_Shoot_Torpedo();
		if (queuedOrders > 0) {
			queuedOrders -= 1;
		}
	}
	
	public bool canActivate() {
		return posted == false && ((crewRoster.jobBoard.Count == 0 && postCtrl.givePost() != null) || assignedCrew != null);
		
		// issue --> jobBoard.Count can be empty but still have active wpns already assigned to crew!
	}
	
	public void _Shoot_Torpedo() {
		Node2D torpedo = (Node2D)torpedoScene.Instantiate();
		torpedo.GlobalPosition = shotPt.GlobalPosition;
		//torpedo.RotationDegrees = RotationDegreesw - 180;
		torpedo.GlobalRotation = shotPt.GlobalRotation - (float) Math.PI;
		underwater.AddChild(torpedo);
	}	
}
