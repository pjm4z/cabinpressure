using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PowerGrid : TileMapLayer
{
	[Export] private TileMapLayer tileMap;
	public Dictionary<Vector2I, WeaponSlot> wpnSlots = new Dictionary<Vector2I, WeaponSlot>();
	public Dictionary<Vector2I, GridItem> wireMap = new Dictionary<Vector2I, GridItem>();
	[Export] private PackedScene wireScene;
	[Export] private PackedScene engineScene;
	[Export] private PackedScene postScene;
	[Export] private PackedScene wpnSlotScene;
	[Export] private PackedScene wpnScene;
	[Export] private CrewRoster crewRoster; // temp todo remove


	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.GlobalPosition = tileMap.GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public override void _Input(InputEvent inputEvent) {
		if (Input.IsActionPressed("shift")) {
			if (Input.IsActionJustPressed("3")) { 
				Vector2I mousePos = (Vector2I) GetLocalMousePosition();
				Vector2I tilePos = LocalToMap(mousePos);
				Vector2 tileLPos = MapToLocal(tilePos);
				TileData td = tileMap.GetCellTileData(tilePos);
				
				if (td != null && !isTileOccupied(tilePos)) {
					addPost(tilePos);
				} 
			}
			if (Input.IsActionJustPressed("4")) { 
				Vector2I mousePos = (Vector2I) GetLocalMousePosition();
				Vector2I tilePos = LocalToMap(mousePos);
				Vector2 tileLPos = MapToLocal(tilePos);
				TileData td = tileMap.GetCellTileData(tilePos);
				
				if (td != null && !isTileOccupied(tilePos)) {
					addWpnSlot(tilePos);
				}
			}
			if (Input.IsActionJustPressed("5")) { 
				Vector2I mousePos = (Vector2I) GetLocalMousePosition();
				Vector2I tilePos = LocalToMap(mousePos);
				Vector2 tileLPos = MapToLocal(tilePos);
				TileData td = tileMap.GetCellTileData(tilePos);
				
				if (td != null && !isTileOccupied(tilePos)) {
					addWpn(tilePos);
				}
			}
			if (Input.IsActionJustPressed("6")) { 
				Vector2I mousePos = (Vector2I) GetLocalMousePosition();
				Vector2I tilePos = LocalToMap(mousePos);
				Vector2 tileLPos = MapToLocal(tilePos);
				TileData td = tileMap.GetCellTileData(tilePos);
				
				if (td != null && !isTileOccupied(tilePos)) {
					addEngine(tilePos);
				}
			}			
			if (inputEvent is InputEventMouseButton mouseButton && mouseButton.Pressed) {
				if (mouseButton.ButtonIndex == MouseButton.Left)
				{
					Vector2I mousePos = (Vector2I) GetLocalMousePosition();
					Vector2I tilePos = LocalToMap(mousePos);
					Vector2 tileLPos = MapToLocal(tilePos);
					TileData td = tileMap.GetCellTileData(tilePos);
					
					if (td != null && !isTileOccupied(tilePos)) {
						addWire(tilePos);
					} else {
						removeItem(tilePos);
					}
				}
			}
		}
	}
	
	public void newWireGroup(GridItem item) {
		newCircuit(item);
		newNetwork(item);
	}
	
	private int circuitSeq = 0;
	public Circuit newCircuit(GridItem item) {
		Circuit circuit = new Circuit();
		AddChild(circuit);
		circuit.init(this);
		circuit.addItem(item);
		circuit.Name = "circuit" + circuitSeq;
		circuitSeq += 1;
		return circuit;
	}
	
	private int networkSeq = 0;
	public Network newNetwork(GridItem item) {
		Network network = new Network();
		AddChild(network);
		network.Name = "network" + networkSeq;
		networkSeq += 1;
		network.init(this);
		network.addItem(item);
		
		return network;
	}
	
	public Circuit newEmptyCircuit() {
		Circuit circuit = new Circuit();
		AddChild(circuit);
		circuit.init(this);
		circuit.Name = "circuit" + circuitSeq;
		circuitSeq += 1;
		return circuit;
	}
	
	public Network newEmptyNetwork() {
		Network network = new Network();
		AddChild(network);
		network.init(this);
		network.Name = "network" + networkSeq;
		networkSeq += 1;
		return network;
	}
	
	private int wireSeq = 0;
	public void addWire(Vector2I tilePos) {
		Wire wire = (Wire) wireScene.Instantiate();
		addItem(wire, tilePos);
		wire.init(this, tilePos, MapToLocal(tilePos));
		wire.Name = "wire" + wireSeq;
		wireSeq += 1;
	}
	
	private int engineSeq = 0;
	public void addEngine(Vector2I tilePos) {
		Engine engine = (Engine) engineScene.Instantiate();
		addItem(engine, tilePos);
		engine.setCrewRoster(this.crewRoster);
		engine.init(this, tilePos, MapToLocal(tilePos));
		engine.Name = "engine" + engineSeq;
		engineSeq += 1;
	}
	
	public void removeItem(Vector2I tilePos) {
		GD.Print("REMOVING ITEm PG");
		GridItem item = wireMap[tilePos];
		if (item.getRelatives() != null) {
			foreach (Vector2I i in item.getRelatives()) {
				wireMap.Remove(item.getTilePos() + i);
			}
		}
		item.removeSelf(); 
		wireMap.Remove(item.getTilePos());
	}
	
	
	private int postSeq = 0;
	public void addPost(Vector2I tilePos) {
		Post post = (Post) postScene.Instantiate();
		addItem(post, tilePos);
		post.init(this, tilePos, MapToLocal(tilePos));
		post.Name = "post" + postSeq;
		postSeq += 1;
	}
	
	public void addWpnSlot(Vector2I tilePos) {
		WeaponSlot wpnSlot = (WeaponSlot) wpnSlotScene.Instantiate();
		wpnSlots[tilePos] = wpnSlot;
		AddChild(wpnSlot);
		wpnSlot.init(newWpnSlotKey(), tilePos, MapToLocal(tilePos));
	}
	
	private int wpnSlotKey = 0;
	private string newWpnSlotKey() {
		this.wpnSlotKey += 1;
		return this.wpnSlotKey.ToString();
	}
	
	public void addWpn(Vector2I tilePos) {
		if (wpnSlots.ContainsKey(tilePos)) {
			Weapon wpn = (Weapon) wpnScene.Instantiate();
			addItem(wpn, tilePos);
			wpnSlots[tilePos].setWpn(wpn);
			wpn.init(this, tilePos, MapToLocal(tilePos));
			wpn.setCrewRoster(this.crewRoster);
		}
	}
	
	private void addItem(GridItem item, Vector2I tilePos) {
		wireMap[tilePos] = item;
		if (item.getRelatives() != null) {
			foreach (Vector2I i in item.getRelatives()) {
				wireMap[tilePos + i] = item;
			}
		}
		AddChild(item);
	}
	
	public List<GridItem> getNeighbors(GridItem item) {
		List<Vector2I> neighborTiles = getNeighbors(item.getTilePos(), item.getRelatives());
		List<GridItem> results = new List<GridItem>();
		foreach (Vector2I pos in neighborTiles) {
			if (isTileOccupied(pos)) {
				if (wireMap[pos] != item) {
					results.Add(wireMap[pos]);
				}
			}
		}
		return results;
	}
	
	private List<Vector2I> getNeighbors(Vector2I tilePos, List<Vector2I> relatives) { 
		HashSet<Vector2I> results = new HashSet<Vector2I>();
		// top
		Vector2I adjTilePos = new Vector2I(tilePos.X, tilePos.Y - 1);
		checkDirection(adjTilePos, relatives, results);
		// right
		adjTilePos = new Vector2I(tilePos.X + 1, tilePos.Y);
		checkDirection(adjTilePos, relatives, results);
		// bottom
		adjTilePos = new Vector2I(tilePos.X, tilePos.Y + 1);
		checkDirection(adjTilePos, relatives, results);
		// left
		adjTilePos = new Vector2I(tilePos.X - 1, tilePos.Y);
		checkDirection(adjTilePos, relatives, results);
		return results.ToList();
	}
	
	private void checkDirection(Vector2I tilePos, List<Vector2I> rels, HashSet<Vector2I> results) {
		TileData td = tileMap.GetCellTileData(tilePos);
		if (td != null) {
			results.Add(tilePos);
		}
		if (rels != null) {
			foreach (Vector2I rel in rels) {
				if (!results.Contains(tilePos + rel)) {
					td = tileMap.GetCellTileData(tilePos + rel);
					if (td != null) {
						results.Add(tilePos + rel);
					}
				}
			}
		}
	}
	
	public bool isTileOccupied(Vector2I pos) {
		GridItem item;
		if (wireMap.TryGetValue(pos, out item)) {
			if (item != null) {
				return true;
			}
		}
		return false;
	}
}
