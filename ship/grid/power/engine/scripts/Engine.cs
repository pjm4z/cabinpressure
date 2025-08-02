using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public partial class Engine : JobTarget
{
	private double usedWatts = 0;
	
	public override void _Ready() {
		base._Ready();
		networkLocked = true;
		watts = 1000;
		taskTime = 1;
		panel = (HBoxContainer) GetNode("/root/basescene/hudcanvas/HUD/systems/systemspanel/enginepanel");
	}
	
	public override void fire() {
		if (this.posted == false && this.assignedCrew == null) {
			this.crewRoster.postJob(this);
			this.posted = true;
		} 
		queuedOrders = 1;
	}	
	
	public override async Task execute() { 
		while (queuedOrders > 0 || this.active == true) {
			await base.execute();
			if ((circuit.needsPower(this.watts) && this.powering == true) || this.active == true) {
				queuedOrders = 1;
			}
		}
	}
	
	protected override void workCallback(double elapsedTime) {
		if (!circuit.needsPower(this.watts) || !this.powering) {
			elapsedTime = taskTime;
		}
	}
	
	public override void setCircuit(Circuit circuit) {
		base.setCircuit(circuit);
		GD.Print("SET CIRC");
		circuit.PowerRQSignal += powerRQEvent;
		circuit.addMaxPower(this.watts);
	}
	
	public override void removeSelf() {
		this.circuit.PowerRQSignal -= powerRQEvent;
		GD.Print("RM CIRC");
		this.postCtrl.removeEngine(this);
		removeCharge();
		base.removeSelf();
	}
	
	public bool overloaded() {
		return usedWatts > watts;
	}
	
	private void powerRQEvent(GridItem target) {
		GD.Print("POWER RQ");
		if (!overloaded()) {
			fire();
		}
	}
	
	public override void setPostCtrl(PostCtrl postCtrl) {
		if (this.postCtrl != null) {
			this.postCtrl.removeEngine(this);
		}
		this.postCtrl = postCtrl;
		Reparent(this.postCtrl);
		this.postCtrl.addEngine(this);
	}
	
	protected override void initNetwork() {
		grid.newNetwork(this);
		reparentNetwork();
		foreach (GridItem i in getNeighbors()) {
			if (i.networkJobCount() == 0) {
				this.absorbNetwork(i);
			}
		}
	}
	
	public override void networkReportEvent(ref HashSet<Vector2I> covered, ref HashSet<Vector2I> visitedEngines, Network newNetwork) {
		setNetwork(newNetwork);
		visitedEngines.Add(getTilePos());
		
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		visited.UnionWith(covered);
		visited.Add(this.tilePos);
		
		if (this.relatives != null) {
			foreach (Vector2I rel in this.relatives) {
				visited.Add(this.tilePos + rel);
			}
		}
		
		List<GridItem> neighbors = getNeighbors();
		List<Engine> foundEngines = new List<Engine>();
		
		for(int i = 0; i < neighbors.Count; i++) {
			GridItem neighbor = neighbors[i];
			if (!visited.Contains(neighbor.getTilePos())) {
				List<Engine> curEngines = new List<Engine>();
				HashSet<Vector2I> curVisited = new HashSet<Vector2I>(visited);
				bool jobFound = neighbor.hasCxnToJobs(ref curVisited, ref curEngines, this); //**
				if (jobFound) {
					reportToItems(null);
					GD.Print("!!!1");
				} else {
					foundEngines.AddRange(curEngines);
					visited.Add(neighbors[i].getTilePos());
					covered.UnionWith(visited);
					reportToItems(this.network);
					GD.Print("!!!2");
				}
			}
		}
		
		foreach (Engine eng in foundEngines) {
			if (eng.network != this.network) {
				eng.networkReportEvent(ref covered, ref visitedEngines, newNetwork);
			}
		}
		reparentNetwork();
	}
	
	public override void connectJobs(ref HashSet<Vector2I> visited, ref List<JobTarget> foundJobs, JobTarget initiator) {}
		
	public override bool hasCxnToJobs(ref HashSet<Vector2I> visited, ref List<Engine> foundEngines, Engine initiator) {
		if (!visited.Contains(getTilePos())) {
			// add visited
			visited.Add(this.tilePos);
			if (this.relatives != null) {
				foreach (Vector2I rel in this.relatives) {
					visited.Add(this.tilePos + rel);
				}
			}
			foundEngines.Add(this);
		}
		return false;
	}
	
	public override HashSet<Vector2I> checkConnections(HashSet<Vector2I> visited, Circuit newCircuit, Network newNetwork) { // todo add logic here to check posts/wpns?
		bool proceed = false;
		// add visited
		visited.Add(this.tilePos);
		if (this.relatives != null) {
			foreach (Vector2I rel in this.relatives) {
				visited.Add(this.tilePos + rel);
			}
		}
		newCircuit.addItem(this);
		if (newNetwork.jobCount() == 0) {
			newNetwork.addItem(this);
			reparentNetwork();
			proceed = true;
		}
		
		// continue as normal
		if (proceed) {
			List<GridItem> neighbors = getNeighbors();
			foreach (GridItem neighbor in neighbors) {
				if (!visited.Contains(neighbor.getTilePos()) && newNetwork.jobCount() == 0) { // may break for multi-tile item
					neighbor.checkConnections(visited, newCircuit, newNetwork);
				}
			}
		} 
		return visited;
	}
}
