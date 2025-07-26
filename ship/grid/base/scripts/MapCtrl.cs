using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MapCtrl : Node2D
{
	[ExportGroup("TileMaps")]
	[Export] private ShipLayer floorMap;
	[Export] private ShipLayer hullMap;
	[Export] private PowerGrid powerGrid;
	[Export] private ShipLayer ceilingMap;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private bool isTileOccupied(TileMapLayer map, Vector2 lPos) {
		Vector2I tilePos = map.LocalToMap(lPos);
		TileData td = map.GetCellTileData(tilePos);
		return td == null;
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
