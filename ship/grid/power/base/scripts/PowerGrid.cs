using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PowerGrid : ShipLayer
{
	//[Export] private TileMapLayer tileMap;
	[Export] private MapCtrl mapCtrl;
	[Export] private CrewRoster crewRoster; // temp todo remove
	public Dictionary<Vector2I, WeaponSlot> wpnSlots = new Dictionary<Vector2I, WeaponSlot>();
	public Dictionary<Vector2I, GridItem> wireMap = new Dictionary<Vector2I, GridItem>();
	[Export] private PackedScene wireScene;
	[Export] private PackedScene engineScene;
	[Export] private PackedScene postScene;
	[Export] private PackedScene wpnSlotScene;
	[Export] private PackedScene wpnScene;
	
	

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		//this.GlobalPosition = this.mapCtrl.getMap(new Vector2I(0,0)).GlobalPosition;
	}
	
	public void initItems() {
		var wpnSlots = GetChildren()
			.Where(child => child is WeaponSlot) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<WeaponSlot>(); // TODO --> change to bed when I have bed class    
		
		foreach(WeaponSlot slot in wpnSlots) {
			addWpnSlot(slot);
		}
		
		var gridItems = GetChildren()
			.Where(child => child is GridItem) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<GridItem>(); // TODO --> change to bed when I have bed class                 
		GD.Print("0-0");
		foreach(var item in gridItems) {
			GD.Print("0-1");
			if (item is Wire) {
				GD.Print("1-wire");
				addWire((Wire) item);
			}
			else if (item is Post) {
				GD.Print("1-post");
				addPost((Post) item);
			}
			else if (item is Engine) {
				GD.Print("1-eng");
				addEngine((Engine) item);
			}
			else if (item is Weapon) {
				GD.Print("1-wpn");
				addWpn((Weapon) item);
			}
		}
	}
	
	public override void _Input(InputEvent inputEvent) {
		if (Input.IsActionPressed("shift")) {
			if (Input.IsActionJustPressed("3")) { 
				Vector2I mousePos = (Vector2I) GetLocalMousePosition();
				Vector2I tilePos = LocalToMap(mousePos);
				Vector2 tileLPos = MapToLocal(tilePos);
				ShipLayer tileMap = mapCtrl.getMap(tilePos);
				if (tileMap != null) {
					TileData td = tileMap.GetCellTileData(tilePos);
					if (td != null && !isTileOccupied(tilePos)) {
						addPost(tilePos);
					} 
				}
				
			}
			if (Input.IsActionJustPressed("4")) { 
				Vector2I mousePos = (Vector2I) GetLocalMousePosition();
				Vector2I tilePos = LocalToMap(mousePos);
				Vector2 tileLPos = MapToLocal(tilePos);
				ShipLayer tileMap = mapCtrl.getMap(tilePos);
				if (tileMap != null) {
					TileData td = tileMap.GetCellTileData(tilePos);
					if (td != null && !isTileOccupied(tilePos)) {
						addWpnSlot(tilePos);
					} 
				}
			}
			if (Input.IsActionJustPressed("5")) { 
				Vector2I mousePos = (Vector2I) GetLocalMousePosition();
				Vector2I tilePos = LocalToMap(mousePos);
				Vector2 tileLPos = MapToLocal(tilePos);
				ShipLayer tileMap = mapCtrl.getMap(tilePos);
				if (tileMap != null) {
					TileData td = tileMap.GetCellTileData(tilePos);
					if (td != null && !isTileOccupied(tilePos)) {
						addWpn(tilePos);
					}
				}
			}
			if (Input.IsActionJustPressed("6")) { 
				Vector2I mousePos = (Vector2I) GetLocalMousePosition();
				Vector2I tilePos = LocalToMap(mousePos);
				Vector2 tileLPos = MapToLocal(tilePos);
				ShipLayer tileMap = mapCtrl.getMap(tilePos);
				if (tileMap != null) {
					TileData td = tileMap.GetCellTileData(tilePos);
					if (td != null && !isTileOccupied(tilePos)) {
						addEngine(tilePos);
					}
				}
			}			
			if (inputEvent is InputEventMouseButton mouseButton && mouseButton.Pressed) {
				if (mouseButton.ButtonIndex == MouseButton.Left)
				{
					Vector2I mousePos = (Vector2I) GetLocalMousePosition();
					Vector2I tilePos = LocalToMap(mousePos);
					Vector2 tileLPos = MapToLocal(tilePos);
					ShipLayer tileMap = mapCtrl.getMap(tilePos);
					if (tileMap != null) {
						TileData td = tileMap.GetCellTileData(tilePos);
						GD.Print(tileMap.Name);
						if (td != null && !isTileOccupied(tilePos)) {
							addWire(tilePos);
						} else {
							removeItem(tilePos);
						}
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
	
	public void addWire(Wire item) {
		Vector2 lPos = item.Position;
		Vector2I tilePos = LocalToMap(lPos);
		ShipLayer map = mapCtrl.getMap(tilePos);
		if (map != null) {
			addItem(item, tilePos);
			item.init(this, tilePos, MapToLocal(tilePos));
			item.Name = "wire" + wireSeq;
			wireSeq += 1;
		} else {
			GD.Print("2-wire");
			item.QueueFree();
		}
	}
	
	private int engineSeq = 0;
	public void addEngine(Vector2I tilePos) {
		Engine engine = (Engine) engineScene.Instantiate();
		addItem(engine, tilePos);
		engine.setCrewRoster(this.crewRoster);
		engine.init(this, tilePos, MapToLocal(tilePos));
		engine.setName("engine" + engineSeq);
		engineSeq += 1;
		engine.key = newWpnSlotKey();
	}
	
	public void addEngine(Engine item) {
		Vector2 lPos = item.Position;
		Vector2I tilePos = LocalToMap(lPos);
		ShipLayer map = mapCtrl.getMap(tilePos);
		if (map != null) {
			addItem(item, tilePos);
			item.setCrewRoster(this.crewRoster);
			item.init(this, tilePos, MapToLocal(tilePos));
			item.setName("engine" + engineSeq);
			engineSeq += 1;
			item.key = newWpnSlotKey();
		} else {
			GD.Print("2-eng");
			item.QueueFree();
		}
	}
	
	
	public void removeItem(Vector2I tilePos) {
		GD.Print("REMOVING ITEm PG");
		GridItem item = wireMap[tilePos];
		if (item.getRelatives() != null) {
			foreach (Vector2I i in item.getRelatives()) {
				wireMap.Remove(item.getTilePos() + i);
			}
		}
		wireMap.Remove(item.getTilePos());
		item.removeSelf(); 
		
	}
	
	
	private int postSeq = 0;
	public void addPost(Vector2I tilePos) {
		Post post = (Post) postScene.Instantiate();
		addItem(post, tilePos);
		post.init(this, tilePos, MapToLocal(tilePos));
		post.Name = "post" + postSeq;
		postSeq += 1;
	}
	
	public void addPost(Post item) {
		Vector2 lPos = item.Position;
		Vector2I tilePos = LocalToMap(lPos);
		ShipLayer map = mapCtrl.getMap(tilePos);
		if (map != null) {
			addItem(item, tilePos);
			item.init(this, tilePos, MapToLocal(tilePos)); 
			item.Name = "post" + postSeq;
			postSeq += 1;
		} else {
			GD.Print("2-post");
			item.QueueFree();
		}
	}
	
	public void addWpnSlot(Vector2I tilePos) {
		WeaponSlot wpnSlot = (WeaponSlot) wpnSlotScene.Instantiate();
		wpnSlots[tilePos] = wpnSlot;
		AddChild(wpnSlot);
		wpnSlot.init(tilePos, MapToLocal(tilePos)); //newWpnSlotKey(), 
	}
	
	public void addWpnSlot(WeaponSlot wpnSlot) {
		Vector2 lPos = wpnSlot.Position;
		Vector2I tilePos = LocalToMap(lPos);
		ShipLayer map = mapCtrl.getMap(tilePos);
		if (map != null) {
			wpnSlot.Position = MapToLocal(tilePos);
			wpnSlots[tilePos] = wpnSlot;
			wpnSlot.init(tilePos, MapToLocal(tilePos)); //newWpnSlotKey(), 
		} else {
			wpnSlot.QueueFree();
		}
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
			wpn.key = newWpnSlotKey();
			wpn.setName("wpn-" + wpn.key);
		}
	}
	
	public void addWpn(Weapon item) {
		Vector2 lPos = item.Position;
		Vector2I tilePos = LocalToMap(lPos);
		ShipLayer map = mapCtrl.getMap(tilePos);
		if (map != null && wpnSlots.ContainsKey(tilePos)) {
			addItem(item, tilePos);
			wpnSlots[tilePos].setWpn(item);
			item.init(this, tilePos, MapToLocal(tilePos)); 
			item.setCrewRoster(this.crewRoster);
			item.key = newWpnSlotKey();
			item.setName("wpn-" + item.key);
		} else {
			GD.Print("2-wpn");
			item.QueueFree();
		}
	}
	
	private void addItem(GridItem item, Vector2I tilePos) {
		wireMap[tilePos] = item;
		if (item.getRelatives() != null) {
			foreach (Vector2I i in item.getRelatives()) {
				wireMap[tilePos + i] = item;
			}
		}
		if (item.GetParent() == null) {
			AddChild(item);
		} else if (item.GetParent() != this) {
			Reparent(item);
		}
		
	}
	
	public GridItem getItem(Vector2I tilePos) {
		if (wireMap.ContainsKey(tilePos)) {
			return wireMap[tilePos];
		}
		return null;
	}
	
	public Wire getWire(Vector2I tilePos) {
		if (wireMap.ContainsKey(tilePos)) {
			if (wireMap[tilePos] is Wire) {
				return (Wire) wireMap[tilePos];
			}
		}
		return null;
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
		ShipLayer tileMap = mapCtrl.getMap(tilePos);
		TileData td = null;
		if (tileMap != null) {
			td = tileMap.GetCellTileData(tilePos);
			if (td != null) {
				results.Add(tilePos);
			}
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
	
	public override bool isTileOccupied(Vector2I pos) {
		GridItem item;
		if (wireMap.TryGetValue(pos, out item)) {
			if (item != null) {
				return true;
			}
		}
		return false;
	}
}
