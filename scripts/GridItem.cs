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
	//protected WireCtrl wireCtrl;
	//protected WireCtrl powerLine;
	protected WireCtrl wireCtrl;
	private Area2D area;
	protected Vector2I tilePos;
	protected List<GridItem> neighbors;
	[Export] protected Godot.Collections.Array<Vector2I> relatives;
	[Export] public float watts;
	
	public List<Vector2I> getRelatives() {
		if (relatives != null) {
			return this.relatives.ToList();
		}
		return null;
	}
	
	public virtual void init(PowerGrid grid, Vector2I tilePos, Vector2 localPos) { //WireCtrl wireCtrl, 
		this.grid = grid;
		this.tilePos = tilePos;
		this.Position = localPos;
		
		this.neighbors = getNeighbors();
		
		if (this.neighbors.Count() > 0) {
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
	
	public virtual void absorb(GridItem item) {
		this.wireCtrl.addItem(item);
		foreach (GridItem neighbor in item.getNeighbors()) {
			if (neighbor.wireCtrl != item.wireCtrl) {
				item.absorb(neighbor);
			}
		}
	}
	
	public virtual HashSet<Vector2I> checkConnections(HashSet<Vector2I> visited, WireCtrl newParent) { // todo add logic here to check posts/wpns?
		if (newParent != null) {
			newParent.addItem(this);
		}
		visited.Add(this.tilePos);
		List<GridItem> neighbors = getNeighbors();
		foreach (GridItem neighbor in neighbors) {
			 if (!visited.Contains(neighbor.tilePos)) {
				neighbor.checkConnections(visited, newParent);
			}
		}
		return visited;
	}
	
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
		}
		QueueFree();
	}
	
	public virtual bool isConnected(GridItem item) {
		return item.getWireCtrl() == this.getWireCtrl();
	}
	
	public Vector2I getTilePos() {
		return this.tilePos;
	}
	
	public Vector2 getPosition() {
		return this.GlobalPosition;
	}
	
	public virtual void setWireCtrl(WireCtrl wireCtrl) {
		if (this.wireCtrl != null && this.wireCtrl != wireCtrl) {
			this.wireCtrl.decrement();	
		}
		if (wireCtrl != null && wireCtrl != this.wireCtrl) {
			wireCtrl.increment();
		}
		this.wireCtrl = wireCtrl;
	}
	
	public WireCtrl getWireCtrl() {
		return this.wireCtrl;
	}
	
	public int getCount() {
		if (this.wireCtrl != null) {
			return this.wireCtrl.getCount();
		}
		return 0;
	}
	
	protected List<GridItem> getNeighbors() {
		return grid.getNeighbors(this);
	}
	
	public virtual void requestPower() {
		this.wireCtrl.requestPower(this);
	}
	
	public virtual bool possible() {
		return wireCtrl.possible(this.watts);
	}
	
	public virtual bool ready() {
		return !wireCtrl.overloaded();
	}
	
	public virtual void addCharge() {
		this.wireCtrl.addCharge(this.watts);
	}
	
	public virtual void removeCharge() {
		this.wireCtrl.removeCharge(this.watts);
	}
}
