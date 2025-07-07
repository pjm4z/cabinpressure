using Godot;
using System;

public partial class JobTarget : GridItem
{
	[Export] protected Boat ship;
	[Export] protected PostCtrl postCtrl;
	[Export] protected CrewRoster crewRoster;
	protected Sprite2D sprite;
	protected Label label;
	protected bool active = false;
	protected bool posted = false;
	protected int queuedOrders;
	public Crew assignedCrew;
	public double taskTime = 1;
	
	protected Color red = new Color(1.0f,0.0f,0.0f,1.0f);
	protected Color white = new Color(1.0f,1.0f,1.0f,1.0f);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	
	public void setCrewRoster(CrewRoster crewRoster) {
		this.crewRoster = crewRoster;
	}
	
	public int count() {
		return this.queuedOrders;
	}
	
	public void clear() {
		this.queuedOrders = 0;
	}
	
	public bool isPosted() {
		return this.posted;
	}
	
	public void setPosted(bool p) {
		this.posted = p;
	}
	
	public override void setWireCtrl(WireCtrl wireCtrl) {
		base.setWireCtrl(wireCtrl);
		this.postCtrl = getWireCtrl().getPostCtrl();
		if (GetParent() != null) {
			Reparent(this.postCtrl);
		} else {
			this.postCtrl.AddChild(this);
		}
		sprite.Modulate =  getWireCtrl().color; 
	}
	
	// move to jt + override
	public virtual void setName(string name) {
		this.Name = name;
		this.label = new Label();
		//weaponsPanel = (HBoxContainer) GetNode("/root/basescene/HUD/weaponscontainer/weaponspanel");
		label.Text = this.Name;
		//weaponsPanel.AddChild(label);
	}
	
	public Post getPost() {
		return this.postCtrl.givePost();
	}


	// move to jt
	public override void _Process(double delta) {
		if (this.label != null) {
			label.Text = this.Name + " " + this.count();
		if (active == true) {
				label.Set("theme_override_colors/font_color",red);
			} else {
				label.Set("theme_override_colors/font_color",white);
			}
		}
		if ((queuedOrders > 0 || active == true) && (posted == false && assignedCrew == null)) {
			crewRoster.postJob(this);
		}
	}
	
	
	
	// move to jt
	public void setActive(bool active) {
		this.active = active;
	}
	
	// move to jt
	public bool getActive() {
		return this.active;
	}	
	
	public void fire() {
		if (this.posted == false && this.assignedCrew == null) {
			this.crewRoster.postJob(this);
			this.posted = true;
		}
		queuedOrders += 1;
	}
	
	public override void removeSelf() {
		this.queuedOrders = 0;
		if (this.assignedCrew != null) {
			this.assignedCrew.detachOrders();
		}
		if (this.label != null) {
			this.label.QueueFree();
		}
		base.removeSelf();
	}
	

	public virtual void execute() {
		if (queuedOrders > 0) {
			queuedOrders -= 1;
		}
	}
	
	public bool canActivate() {
		return posted == false && ((crewRoster.jobBoard.Count == 0 && postCtrl.givePost() != null && crewRoster.maxReady != null) || assignedCrew != null);
		
		// issue --> jobBoard.Count ;can be empty but still have active wpns already assigned to crew!
	}
}
