using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[GlobalClass]
public partial class JobTarget : GridItem
{
	[Export] protected Ship ship;
	[Export] protected PostCtrl postCtrl;
	[Export] protected CrewRoster crewRoster;
	protected Label label;
	protected bool active = false;
	protected bool posted = false;
	protected int queuedOrders;
	public Crew assignedCrew;
	public double taskTime = 1;
	protected HBoxContainer panel;
	
	protected Color red = new Color(1.0f,0.0f,0.0f,1.0f);
	protected Color white = new Color(1.0f,1.0f,1.0f,1.0f);
	
	public override void _Ready() {
		sprite = (Sprite2D) GetNode("sprite");
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
	
	public override void setNetwork(Network network) {
		base.setNetwork(network);
		setPostCtrl(network.getPostCtrl());
	}
	
	public virtual void setName(string name) {
		this.Name = name;
		this.label = new Label();
		this.label.Text = this.Name;
		this.panel.AddChild(this.label);
	}
	
	public Post getPost() {
		return this.postCtrl.givePost();
	}

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
	
	public void setActive(bool active) {
		this.active = active;
	}
	
	public bool getActive() {
		return this.active;
	}
	
	public virtual void fire() {
		if (this.posted == false && this.assignedCrew == null) {
			this.circuit.requestPower(this);
			this.crewRoster.postJob(this);
			this.posted = true;
		} 
		queuedOrders += 1;
	}
	
	protected virtual void workCallback(double elapsedTime) {}
	
	public async Task waitForGameTime(double seconds, Action<double> callback) {
		double elapsedTime = 0;
		while (elapsedTime < seconds) {
			callback?.Invoke(elapsedTime);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); 
			elapsedTime += GetProcessDeltaTime(); 
		}
	}
	
	public virtual async Task execute() {
		await waitForGameTime(taskTime, (elapsedTime) => { workCallback(elapsedTime); });
		if (queuedOrders > 0) {
			queuedOrders -= 1;
		}
	}
	
	protected override void reparentNetwork() {
		base.reparentNetwork();
		this.Reparent(this.postCtrl);
	}
	
	public virtual void setPostCtrl(PostCtrl postCtrl) {
		if (this.postCtrl != null) {
			this.postCtrl.removeJob(this);
		}
		if (postCtrl != null) {
			postCtrl.addJob(this);
		}
		this.postCtrl = postCtrl;
		Reparent(postCtrl);
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
	
	public override bool hasCxnToJobs(ref HashSet<Vector2I> visited, ref List<Engine> foundEngines, Engine initiator) {
		// add visited
		visited.Add(this.tilePos);
		if (this.relatives != null) {
			foreach (Vector2I rel in this.relatives) {
				visited.Add(this.tilePos + rel);
			}
		}
		return true;
	}
	
	
	public bool canActivate() {
		return posted == false && ((crewRoster.jobBoard.Count == 0 && postCtrl.givePost() != null && crewRoster.maxReady != null) || assignedCrew != null);
		// issue --> jobBoard.Count ;can be empty but still have active wpns already assigned to crew!
	}
}
