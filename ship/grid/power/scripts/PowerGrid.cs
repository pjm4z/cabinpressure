using Godot;
using System;
using System.Collections.Generic;

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
	//private WireCtrl wireCtrl;	


	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.GlobalPosition = tileMap.GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public List<Vector2I> getNeighbors(Vector2I tilePos) {
		List<Vector2I> results = new List<Vector2I>();
		
		// top
		Vector2I adjTilePos = new Vector2I(tilePos.X, tilePos.Y - 1);
		TileData adjTile = tileMap.GetCellTileData(adjTilePos);
		if (adjTile != null) {
			results.Add(adjTilePos);
		} 
		// right
		adjTilePos = new Vector2I(tilePos.X + 1, tilePos.Y);
		adjTile = tileMap.GetCellTileData(adjTilePos);
		if (adjTile != null) {
			results.Add(adjTilePos);
		}
		// bottom
		adjTilePos = new Vector2I(tilePos.X, tilePos.Y + 1);
		adjTile = tileMap.GetCellTileData(adjTilePos);
		if (adjTile != null) {
			results.Add(adjTilePos);
		}
		// left
		adjTilePos = new Vector2I(tilePos.X - 1, tilePos.Y);
		adjTile = tileMap.GetCellTileData(adjTilePos);
		if (adjTile != null) {
			results.Add(adjTilePos);
		}
		return results;
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
	

	
	
	public WireCtrl newWireGroup(GridItem item) {
		WireCtrl wireCtrl = new WireCtrl();
		AddChild(wireCtrl);
		wireCtrl.init();
		wireCtrl.addItem(item);
		return wireCtrl;
	}
	
	public WireCtrl newEmptyWireGroup() {
		WireCtrl wireCtrl = new WireCtrl();
		AddChild(wireCtrl);
		wireCtrl.init();
		return wireCtrl;
	}
	
	public void addWire(Vector2I tilePos) {
		Wire wire = (Wire) wireScene.Instantiate();
		addItem(wire, tilePos);
		wire.init(this, tilePos, MapToLocal(tilePos));
	}
	
	public void addEngine(Vector2I tilePos) {
		Engine engine = (Engine) engineScene.Instantiate();
		addItem(engine, tilePos);
		engine.setCrewRoster(this.crewRoster);
		engine.init(this, tilePos, MapToLocal(tilePos));
	}
	
	public void removeItem(Vector2I tilePos) {
		// check count, rm wirectrl if needed
		GridItem item = wireMap[tilePos];
		if (item.getRelatives() != null) {
			foreach (Vector2I i in item.getRelatives()) {
				wireMap.Remove(item.getTilePos() + i);
			}
		}
		item.removeSelf();
		wireMap.Remove(item.getTilePos());
	}
	
	public void addPost(Vector2I tilePos) {
		Post post = (Post) postScene.Instantiate();
		addItem(post, tilePos);
		post.init(this, tilePos, MapToLocal(tilePos));
	}
	
	public void addWpnSlot(Vector2I tilePos) {
		WeaponSlot wpnSlot = (WeaponSlot) wpnSlotScene.Instantiate();
		wpnSlots[tilePos] = wpnSlot;
		AddChild(wpnSlot);
		wpnSlot.init(newWpnSlotKey(), tilePos, MapToLocal(tilePos));
		//post.init(this, tilePos, MapToLocal(tilePos));
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
		List<Vector2I> neighborTiles = getNeighbors(item.getTilePos());
		List<GridItem> results = new List<GridItem>();
		foreach (Vector2I pos in neighborTiles) {
			if (isTileOccupied(pos)) {
				results.Add(wireMap[pos]);
			}
		}
		return results;
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
