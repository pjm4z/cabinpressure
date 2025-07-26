using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShipLayer : TileMapLayer
{
	[Export] private Ship ship;
	Dictionary<Vector2I, int> tiles = new Dictionary<Vector2I, int>();
	
	public Ship getShip() {
		return this.ship;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		initTiles();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void initTiles() {
		foreach (Vector2I tile in GetUsedCells()) {
			TileData td = GetCellTileData(tile);
			tiles.Add(tile, (int) td.GetCustomData("MAX_HP"));
			ship.changeHP((int) td.GetCustomData("MAX_HP"));
		}
	}
	
	//Dictionary<Vector2I, int> damageMap = new Dictionary<Vector2I, int>();
	//protected Color red = 
	public void applyDamage(Vector2 gPos, double radius, int damage) {
		Vector2I tilePos = LocalToMap(ToLocal(gPos));
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		visited.Add(tilePos);
		
		tiles[tilePos] = tiles[tilePos] - damage;
		if (tiles[tilePos] < 0) {
			tiles[tilePos] = 0;
			ship.changeHP((int) -(damage * 1.5));
		} else {
			ship.changeHP(-damage);
		}
		
		//TileData td = GetCellTileData(tilePos);
		//td.Modulate = new Color(1.0f,0.0f,0.0f,1.0f);
		
		//GD.Print(tilePos + " Origin " + tiles[tilePos]);
		
		foreach (Vector2I adjTile in getNeighbors(tilePos)) {
			Vector2 neighbor = ToGlobal(MapToLocal(adjTile));
			if (neighbor.DistanceTo(gPos) <= radius && !visited.Contains(adjTile)) {
				spreadDamage(ref visited, neighbor, gPos, radius, damage);
			}
		}
		GD.Print(ship.hp);
	}
	
	private void spreadDamage(ref HashSet<Vector2I> visited, Vector2 gPos, Vector2 origin, double radius, int damage) {
		Vector2I tilePos = LocalToMap(ToLocal(gPos));
		visited.Add(tilePos);
		
		Vector2I originTile = LocalToMap(ToLocal(origin));
		int tile_dist = (int) tilePos.DistanceTo(originTile); 
		
		
		tiles[tilePos] = tiles[tilePos] - (damage / tile_dist);
		if (tiles[tilePos] < 0) {
			tiles[tilePos] = 0;
			ship.changeHP((int) -((damage / tile_dist) * 1.5));
		} else {
			ship.changeHP(-(damage / tile_dist));
		}
		//tiles[tilePos] = tiles[tilePos] - (damage / tile_dist);
		//ship.changeHP(-(damage / tile_dist));
		//GD.Print(tilePos + " Origin-" + tile_dist + " " + tiles[tilePos]);
		
		foreach (Vector2I adjTile in getNeighbors(tilePos)) {
			Vector2 neighbor = ToGlobal(MapToLocal(adjTile));
			if (neighbor.DistanceTo(origin) <= radius && !visited.Contains(adjTile)) {
				spreadDamage(ref visited, neighbor, origin, radius, damage);
			}
		}
	}
	
	protected List<Vector2I> getNeighbors(Vector2I tilePos) { 
		HashSet<Vector2I> results = new HashSet<Vector2I>();
		// top
		Vector2I adjTilePos = new Vector2I(tilePos.X, tilePos.Y - 1);
		results = checkDirection(adjTilePos, results);
		// right
		adjTilePos = new Vector2I(tilePos.X + 1, tilePos.Y);
		results = checkDirection(adjTilePos, results);
		// bottom
		adjTilePos = new Vector2I(tilePos.X, tilePos.Y + 1);
		results = checkDirection(adjTilePos, results);
		// left
		adjTilePos = new Vector2I(tilePos.X - 1, tilePos.Y);
		results = checkDirection(adjTilePos, results);
		
		return results.ToList();
	}
	
	protected HashSet<Vector2I> checkDirection(Vector2I tilePos, HashSet<Vector2I> results) {
		if (isTileOccupied(tilePos)) {
			results.Add(tilePos);
		}
		return results;
	}
	
	public virtual bool isTileOccupied(Vector2I pos) {
		TileData td = GetCellTileData(pos);
		return td != null;
	}
	
	public virtual bool isTileOccupied(Vector2 gPos) {
		TileData td = GetCellTileData(LocalToMap(ToLocal(gPos)));
		return td != null;
	}
}
