using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[GlobalClass]
public partial class ShipSystem : GridItem
{
	[Export] public Ship ship;
	[Export] public PostCtrl postCtrl;
	[Export] public CrewRoster crewRoster;
	
	protected Label label;
	protected bool active = false;
	protected bool occupied = false;
	protected bool posted = false;
	protected int queuedOrders;
	public Crew assignedCrew;
	public double taskTime = 1;
	protected HBoxContainer panel;
	protected Sprite2D sprite;
	protected StateMachine brain;
	
	protected Color red = new Color(1.0f,0.0f,0.0f,1.0f);
	protected Color white = new Color(1.0f,1.0f,1.0f,1.0f);
	
	public override void _Ready() {
		sprite = (Sprite2D) GetNode("sprite");
		brain = (StateMachine) GetNode("brain");
		//setName(Name);
	}
	
	public override void init(PowerGrid grid, Vector2I tilePos, Vector2 localPos) {
		base.init(grid, tilePos, localPos);
		brain.init();
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
		if (this.sprite != null) {
			this.sprite.Modulate = network.color; 
		}
	}
	
	public virtual void setName(string name) {
		this.Name = name;
		if (this.label == null) {
			this.label = new Label();
			this.panel.AddChild(this.label);
		}
		this.label.Text = this.Name;
	}
	
	public Post getPost() {
		return this.postCtrl.givePost();
	}

	public override void _Process(double delta) {
		if (this.label != null) {
			label.Text = this.Name + " " + this.count();
		if (active == true) {
			//GD.Print("ACTIVE " + Name);
				label.Set("theme_override_colors/font_color",red);
			} else {
				label.Set("theme_override_colors/font_color",white);
			}
		}
		//if ((queuedOrders > 0 || active == true) && (posted == false && assignedCrew == null)) {
		//	crewRoster.postJob(this);
	//	}
		if (!GetTree().Paused) {
			brain.process(delta);
		}
	}
	
	[Export] public string key;
	
	public override void _Input(InputEvent inputEvent) {
		if ((Input.IsActionJustPressed("shift") && Input.IsActionPressed(key)) || 
				(Input.IsActionPressed("shift") && Input.IsActionJustPressed(key))) {
			if ((active == false && canActivate()) || (active == true)) {
				active = !active;
			} 
		} else {
			if ((Input.IsActionJustPressed("ctrl") && Input.IsActionPressed(key)) || 
					(Input.IsActionPressed("ctrl") && Input.IsActionJustPressed(key))) {
				clear();
			} else if (Input.IsActionJustPressed(key)) {
				fire();
			}
		}
	}
	
	public void setActive(bool active) {
		this.active = active;
	}
	
	public bool getActive() {
		return this.active;
	}
	
	public void setOccupied(bool occupied) {
		this.occupied = occupied;
	}
	
	public bool isOccupied() {
		return occupied;
	}
	
	public void completeOrder() {
		if (queuedOrders > 0) {
			queuedOrders -= 1;
		}
	}
	
	public bool shouldQueue() {
		return (active || queuedOrders > 0);
	}
	
	public virtual void fire() {
		//if (this.posted == false && this.assignedCrew == null) {
			//this.circuit.requestPower(this);
		//	this.crewRoster.postJob(this);
		//	this.posted = true;
		//} 
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
	
	public virtual void execute() { //async Task
		//addCharge();
		//await waitForGameTime(taskTime, (elapsedTime) => { workCallback(elapsedTime); });
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
	
	protected virtual void leavePostCtrl() {
		if (this.postCtrl != null) {
			this.postCtrl.removeJob(this);
		}
	}
	
	public override void removeSelf() {
		this.queuedOrders = 0;
		if (this.assignedCrew != null) {
			this.assignedCrew.detachOrders();
		}
		if (this.label != null) {
			this.label.QueueFree();
		}
		leavePostCtrl();
		removeCharge();
		base.removeSelf();
	}
	
	[Signal]
	public delegate void ItemReportSignalEventHandler(ShipSystem reporter, Network network);
	
	public void reportToItems(Network network) {
		EmitSignal(nameof(SignalName.ItemReportSignal), this, network);
	}
	
	public virtual void networkReportEvent(ref HashSet<Vector2I> covered, ref HashSet<Vector2I> visitedJobs, Network newNetwork) {
		setNetwork(newNetwork);
		visitedJobs.Add(getTilePos());
		
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		visited.UnionWith(covered);
		visited.Add(this.tilePos);
		
		if (this.relatives != null) {
			foreach (Vector2I rel in this.relatives) {
				visited.Add(this.tilePos + rel);
			}
		}
		
		List<GridItem> neighbors = getNeighbors();
		List<ShipSystem> foundJobs = new List<ShipSystem>();
		
		for(int i = 0; i < neighbors.Count; i++) {
			GridItem neighbor = neighbors[i];
			if (!visited.Contains(neighbor.getTilePos())) {
				List<ShipSystem> curJobs = new List<ShipSystem>();
				neighbor.connectJobs(ref visited, ref curJobs, this); 
				visited.Add(neighbors[i].getTilePos());
				foundJobs.AddRange(curJobs);
				covered.UnionWith(visited);
				reportToItems(this.network);
			}
		}
		
		foreach (ShipSystem job in foundJobs) {
			if (job.network != this.network) {
				job.networkReportEvent(ref covered, ref visitedJobs, newNetwork);
			}
		}
		reparentNetwork();
	}
	
	public override void connectJobs(ref HashSet<Vector2I> visited, ref List<ShipSystem> foundJobs, ShipSystem initiator) {
		// add visited
		visited.Add(this.tilePos);
		if (this.relatives != null) {
			foreach (Vector2I rel in this.relatives) {
				visited.Add(this.tilePos + rel);
			}
		}
		foundJobs.Add(this);
	}
	
	public override bool hasCxnToJobs(ref HashSet<Vector2I> visited, ref List<Engine> foundEngines, Engine initiator) {
		return true;
	}
	
	public bool canActivate() {
		return ((crewRoster.jobBoard.Count == 0 && postCtrl.givePost() != null && crewRoster.maxReady != null) || assignedCrew != null);
		// issue --> jobBoard.Count ;can be empty but still have active wpns already assigned to crew!
	}
}
