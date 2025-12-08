using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MapCtrl : Node2D
{
	[ExportGroup("TileMaps")]
	[Export] private Ship ship;
	[Export] private ShipLayer navMap;
	[Export] private ShipLayer hullMap;
	[Export] private PowerGrid powerGrid;
	[Export] private ShipLayer ceilingMap;
	private List<ShipLayer> maps = new List<ShipLayer>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		maps.Add(hullMap);
		maps.Add(ceilingMap);
		maps.Add(navMap);
		maps.Add(powerGrid);
		
		foreach (ShipLayer map in maps) {
			map.init(this.ship);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public ShipLayer getMap(Vector2I tilePos) {
		foreach (ShipLayer map in this.maps) {
			if (isTileOccupied(map, tilePos)) {
				if (map != ceilingMap) {
					return map;
				}
			}
		}
		return null;
	}
	
	public ShipLayer getMap(Vector2 lPos) {
		foreach (ShipLayer map in this.maps) {
			if (isTileOccupied(map, lPos)) {
				if (map != ceilingMap) {
					return map;
				}
			}
		}
		return null;
	}
	
	private bool isTileOccupied(TileMapLayer map, Vector2 lPos) {
		Vector2I tilePos = map.LocalToMap(lPos);
		TileData td = map.GetCellTileData(tilePos);
		return td != null;
	}
	
	private bool isTileOccupied(TileMapLayer map, Vector2I tilePos) {
		TileData td = map.GetCellTileData(tilePos);
		return td != null;
	}
	
	public void damageOuter(Vector2 gPos, double radius, int damage) {
		if (ceilingMap.isTileOccupied(gPos)) {
			GD.Print("DMG CEILING" );
			ceilingMap.applyDamage(gPos, radius, damage);
		} else if (hullMap.isTileOccupied(gPos)) {
			GD.Print("DMG HULL" );
			hullMap.applyDamage(gPos, radius, damage);
		}
		
	}
	
	public void damageInner(Vector2 gPos, double radius, int damage) {
		//mapCtrl.damageInner(gPos, radius, damage);
		GD.Print("DMG INNER" );
	}
}
