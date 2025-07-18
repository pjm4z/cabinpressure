using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class GridItem : Node2D
{
	[Signal]
	public delegate void RMSelfSignalEventHandler(GridItem gi);
	protected PowerGrid grid;
	protected Vector2I tilePos;
	protected List<GridItem> neighbors;
	[Export] protected Godot.Collections.Array<Vector2I> relatives;
	protected Sprite2D sprite;
	protected Network network;
	protected Circuit circuit;
	protected bool powering = false;
	[Export] public float watts;
	
	public bool networkLocked = false;
	
	public List<Vector2I> getRelatives() {
		if (relatives != null) {
			return this.relatives.ToList();
		}
		return null;
	}
	
	public virtual void init(PowerGrid grid, Vector2I tilePos, Vector2 localPos) {
		this.grid = grid;
		this.tilePos = tilePos;
		this.Position = localPos;
		
		initCircuit();
		initNetwork();
	}
	
	protected virtual void reparentNetwork() {
		this.network.Reparent(this.circuit);
	}
	
	protected virtual void initCircuit() {
		int c = 0;
		this.neighbors = getNeighbors(); 
		foreach (GridItem i in neighbors) {
			if (i != this) {
				c += 1;
			}
		}
		if (c == 0) {
			grid.newCircuit(this); 
		} else {
			GridItem maxCount = neighbors[0];
			for (int i = 1; i < neighbors.Count; i++) {
				if (neighbors[i].circuitCount() > maxCount.circuitCount()) {//
					maxCount = neighbors[i];
				}
			}
			if (maxCount.circuit != this.circuit) {
				maxCount.absorbCircuit(this);
			}
		}
	}
	
	protected virtual void initNetwork() {
		int c = 0;
		this.neighbors = getNeighbors();
		foreach (GridItem i in neighbors) {
			if (i != this) {
				c += 1;
			}
		}
		if (c == 0) {
			grid.newNetwork(this);
			reparentNetwork();
		} else {
			GridItem maxCount = neighbors[0];
			GridItem maxJobs = neighbors[0];
			for (int i = 1; i < neighbors.Count; i++) {
				if (neighbors[i].networkCount() > maxCount.networkCount()) {
					maxCount = neighbors[i];
				}
				if (neighbors[i].networkJobCount() > maxJobs.networkJobCount()) {
					maxJobs = neighbors[i];
				}
			}
			if (maxJobs.networkJobCount() > 0) {
				maxJobs.absorbNetwork(this);
			} else {
				maxCount.absorbNetwork(this);
			}
		}
		
	}
	
	public virtual void absorbNetwork(GridItem item) {
		if (!item.networkLocked && item.networkEngineCount() > 0) {
			HashSet<Vector2I> visited = new HashSet<Vector2I>();
			item.getNetwork().reportToEngines(ref visited);
 		}
		if (!item.networkLocked || (item.networkLocked && this.networkJobCount() == 0)) {
			this.network.addItem(item);
			foreach (GridItem neighbor in item.getNeighbors()) {
				if (!sameNetwork(item, neighbor)) {
					item.absorbNetwork(neighbor);
				}
			}
		}
	}
	
	public bool sameNetwork(GridItem item1, GridItem item2) {
		return item1.network == item2.network;
	}
	
	public virtual void absorbCircuit(GridItem item) {
		this.circuit.addItem(item);
		if (item.network != null) {
			item.reparentNetwork();
		}
		foreach (GridItem neighbor in item.getNeighbors()) {
			if (neighbor.circuit != item.circuit) {
				item.absorbCircuit(neighbor);
			}
		}
	}
	
	protected void forfeitNetwork(Engine engine, Network network) {
		engine.ItemReportSignal -= forfeitNetwork; 
		if (network != null) {
			setNetwork(network);
			reparentNetwork();
		}
	}
	
	public virtual bool hasCxnToJobs(ref HashSet<Vector2I> visited, ref List<Engine> foundEngines, Engine initiator) {
		initiator.ItemReportSignal += forfeitNetwork; 
		// add visited
		visited.Add(this.tilePos);
		if (this.relatives != null) {
			foreach (Vector2I rel in this.relatives) {
				visited.Add(this.tilePos + rel);
			}
		}
		List<GridItem> neighbors = getNeighbors();
		foreach (GridItem neighbor in neighbors) {
			if (!visited.Contains(neighbor.getTilePos())) {
				if (neighbor.hasCxnToJobs(ref visited, ref foundEngines, initiator)) {
					return true;
				}
			}
		}
		return false;
	}
	
	public virtual HashSet<Vector2I> checkConnections(HashSet<Vector2I> visited, Circuit newCircuit, Network newNetwork) {
		// add visited
		visited.Add(this.tilePos);
		if (this.relatives != null) {
			foreach (Vector2I rel in this.relatives) {
				visited.Add(this.tilePos + rel);
			}
		}
		newCircuit.addItem(this);
		newNetwork.addItem(this);
		reparentNetwork();
		List<GridItem> neighbors = getNeighbors();
		foreach (GridItem neighbor in neighbors) {
			if (!visited.Contains(neighbor.getTilePos())) { 
				neighbor.checkConnections(visited, newCircuit, newNetwork);
			}
		}
		return visited;
	}
	
	public virtual void removeSelf() {
		EmitSignal(nameof(SignalName.RMSelfSignal), this);
		if (this.network != null) {
			this.network.decrement();
		}
		if (this.circuit != null) {
			this.circuit.decrement();
		}
		
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		visited.Add(this.tilePos);
		if (this.relatives != null) {
			foreach (Vector2I rel in this.relatives) {
				visited.Add(this.tilePos + rel);
			}
		}
		if (this.networkEngineCount() > 0) {
			this.network.reportToEngines(ref visited);
		}
		Circuit newCircuit;
		Network newNetwork;
		List<GridItem> neighbors = getNeighbors();
		for (int i = 0; i < neighbors.Count; i++) {
			GridItem neighbor = neighbors[i];
			if (!visited.Contains(neighbor.getTilePos())) {
				newCircuit = grid.newEmptyCircuit();
				newNetwork = grid.newEmptyNetwork();
				visited = neighbor.checkConnections(visited, newCircuit, newNetwork);
			}
		}
		QueueFree();
	}
	
	public virtual bool sameCircuit(GridItem item) {
		return item.getCircuit() == this.circuit;
	}
	
	public virtual bool sameNetwork(GridItem item) {
		return item.getNetwork() == this.network;
	}
	
	public Vector2I getTilePos() {
		return this.tilePos;
	}
	
	public Vector2 getPosition() {
		return this.GlobalPosition;
	}
	
	public virtual void setNetwork(Network network) {
		if (this.network != null && this.network != network) {
			this.network.decrement();
		}
		if (network != null && network != this.network) {
			network.increment();
		}
		this.network = network;
		if (this.sprite != null) {
			this.sprite.Modulate = network.color; 
		}
	}
	
	public virtual void setCircuit(Circuit circuit) {
		if (this.circuit != null && this.circuit != circuit) {
			this.circuit.decrement();	
		}
		if (circuit != null && circuit != this.circuit) {
			circuit.increment();
		}
		this.circuit = circuit;
		Reparent(this.circuit);
	}
	
	public Network getNetwork() {
		return this.network;
	}
	
	public Circuit getCircuit() {
		return this.circuit;
	}
	
	public int circuitCount() {
		if (this.circuit != null) {
			return this.circuit.getCount();
		}
		return 0;
	}
	
	public int networkCount() {
		if (this.network != null) {
			return this.network.getCount();
		}
		return 0;
	}
	
	public int networkJobCount() {
		if (this.network != null) {
			return this.network.jobCount();
		}
		return 0;
	}
	
	public int networkEngineCount() {
		if (this.network != null) {
			return this.network.engineCount();
		}
		return 0;
	}
	
	public List<GridItem> getNeighbors() {
		return grid.getNeighbors(this);
	}
	
	public virtual void requestPower() {
		this.circuit.requestPower(this);
	}
	
	public virtual bool possible() {
		return circuit.possible(this.watts);
	}
	
	public virtual bool ready() {
		return !circuit.overloaded();
	}
	
	public virtual void addCharge() {
		if (!this.powering) {
			this.powering = true;
			this.circuit.addCharge(this.watts);
		}
	}
	
	public virtual void removeCharge() {
		if (this.powering) {
			this.powering = false;
			this.circuit.removeCharge(this.watts);
		}
	}
}
