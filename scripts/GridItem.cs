using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class GridItem : Node2D
{
	[Signal]
	public delegate void RMSelfSignalEventHandler(GridItem gi);
	
	private PowerGrid grid;
	private WireCtrl wireCtrl;
	private Area2D area;
	private Vector2I tilePos;
	public List<GridItem> neighbors;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}
	
	public virtual void init(PowerGrid grid, Vector2I tilePos, Vector2 localPos) { //WireCtrl wireCtrl, 
		this.grid = grid;
		this.tilePos = tilePos;
		this.Position = localPos;
		
		neighbors = getNeighbors();
		if (neighbors.Count == 0) {
			grid.newWireGroup(this);
		} else {
			GridItem maxCount = neighbors[0];
			for (int i = 1; i < neighbors.Count; i++) {
				if (neighbors[i].getCount() > maxCount.getCount()) {
					maxCount = neighbors[i];
				}
			}
			if (maxCount.getCount() < this.getCount()) {
				this.absorb(maxCount);
			} else {
				maxCount.absorb(this);
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
		this.wireCtrl.decrement();
		List<GridItem> neighbors = getNeighbors();
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		visited.Add(this.tilePos);
		for (int i = 0; i < neighbors.Count; i++) {
			WireCtrl newParent;
			if (i == 0) {
				newParent = null;
			} else {
				newParent = grid.newEmptyWireGroup();
			}
			
			GridItem neighbor = neighbors[i];
			if (!visited.Contains(neighbor.getTilePos())) {
				visited = neighbor.checkConnections(visited, newParent);
			}
		}
		QueueFree();
	}
	
	public bool isConnected(GridItem item) {
		return item.getWireCtrl() 
			== 
			this.getWireCtrl();
	}
	
	public Vector2I getTilePos() {
		return this.tilePos;
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
	
	/*public void charge(float watts) {
		this.charge -= watts;
		//if (this.charge < 0) do something?
		foreach (n in this.neighbors) {
			n.charge(this.charge);
		}
	}*/
	
	private List<GridItem> getNeighbors() {
		return grid.getNeighbors(this);
	}
}
