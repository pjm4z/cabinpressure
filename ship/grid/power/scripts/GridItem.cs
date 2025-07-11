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
	
	protected WireCtrl wireCtrl;
	protected Network network;
	protected Circuit circuit;
	protected bool powering = false;
	[Export] public float watts;
	
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
		
		initWireCtrl(); //network//circuit
	}
	//network//circuit
	protected virtual void initWireCtrl() {
		int c = 0;
		this.neighbors = getNeighbors();
		foreach (GridItem i in neighbors) {
			if (i != this) {
				c += 1;
			}
		}
		if (c == 0) {
			grid.newWireGroup(this);
		} else {
			GridItem maxCount = neighbors[0];
			for (int i = 1; i < neighbors.Count; i++) {
				if (neighbors[i].getCount() > maxCount.getCount()) {
					maxCount = neighbors[i];
				}
			}
			if (maxCount.wireCtrl != this.wireCtrl) {
				if (maxCount.getCount() < this.getCount()) {
					this.absorb(maxCount);
				} else {
					maxCount.absorb(this);
				}
			}
		}
	}
	
	//network//circuit
	public virtual void absorb(GridItem item) {
		this.wireCtrl.addItem(item);
		foreach (GridItem neighbor in item.getNeighbors()) {
			if (neighbor.wireCtrl != item.wireCtrl) {
				item.absorb(neighbor);
			}
		}
	}
	
	//network//circuit
	public virtual HashSet<Vector2I> checkConnections(HashSet<Vector2I> visited, WireCtrl newParent) { // todo add logic here to check posts/wpns?
		if (newParent != null) {
			newParent.addItem(this);
		}
		visited.Add(this.tilePos);
		List<GridItem> neighbors = getNeighbors();
		foreach (GridItem neighbor in neighbors) {
			 if (!visited.Contains(neighbor.tilePos)) {		//TODO this why it cant find relatives
				neighbor.checkConnections(visited, newParent);
			}
		}
		//TODO: need method get neighbors from relative
	//	foreach (Vector2I relative in relatives) {
	//		 if (!visited.Contains(this.tilePos + relative)) {		//TODO this why it cant find relatives
	//			relative.checkConnections(visited, newParent);
	//		}
		}
		return visited;
	}
	
	//network//circuit
	public virtual void removeSelf() {
		EmitSignal(nameof(SignalName.RMSelfSignal), this);
		if (this.wireCtrl != null) {
			this.wireCtrl.decrement();
		}
		List<GridItem> neighbors = getNeighbors();
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		visited.Add(this.tilePos);
		for (int i = 0; i < neighbors.Count; i++) {
			WireCtrl newParent;
			if (i == 0) {
				newParent = null;
			} else {
				newParent = grid.newEmptyWireGroup();	// todo will be grid for GI, this for JT
			}
			
			GridItem neighbor = neighbors[i];
			if (!visited.Contains(neighbor.getTilePos())) {
				visited = neighbor.checkConnections(visited, newParent);
			}
			
			// if newParent.cxns == 0  (not include posts)
				// add all kids to grid, rm circuit?
		}
		QueueFree();
	}
	
	//network//circuit
	public virtual bool isConnected(GridItem item) {
		return item.getWireCtrl() == this.getWireCtrl();
	}
	
	public Vector2I getTilePos() {
		return this.tilePos;
	}
	
	public Vector2 getPosition() {
		return this.GlobalPosition;
	}
	
	//network//circuit
	public virtual void setWireCtrl(WireCtrl wireCtrl) {
		if (this.wireCtrl != null && this.wireCtrl != wireCtrl) {
			this.wireCtrl.decrement();	
		}
		if (wireCtrl != null && wireCtrl != this.wireCtrl) {
			wireCtrl.increment();
		}
		this.wireCtrl = wireCtrl;
	}
	
	//network//circuit
	public WireCtrl getWireCtrl() {
		return this.wireCtrl;
	}
	
	//network//circuit
	public int getCount() {
		if (this.wireCtrl != null) {
			return this.wireCtrl.getCount();
		}
		return 0;
	}
	
	//network//circuit
	protected List<GridItem> getNeighbors() {
		return grid.getNeighbors(this);
	}
	
	//circuit
	public virtual void requestPower() {
		this.wireCtrl.requestPower(this);
	}
	
	//circuit
	public virtual bool possible() {
		return wireCtrl.possible(this.watts);
	}
	
	//circuit
	public virtual bool ready() {
		return !wireCtrl.overloaded();
	}
	
	//circuit
	public virtual void addCharge() {
		if (!this.powering) {
			this.powering = true;
			this.wireCtrl.addCharge(this.watts);
		}
	}
	
	//circuit
	public virtual void removeCharge() {
		if (this.powering) {
			this.powering = false;
			this.wireCtrl.removeCharge(this.watts);
		}
	}
}
