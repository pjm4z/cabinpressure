using Godot;
using System;

public partial class WeaponSlot : Area2D
{
	[Export] private Weapon wpn;
	//public bool active = false;
	private Vector2I tilePos;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public void init(Vector2I tilePos, Vector2 pos) {//string key, 
		//this.key = key;
		this.tilePos = tilePos;
		this.Position = pos;
	}
	
	public void setWpn(Weapon wpn) {
		this.wpn = wpn;
		wpn.setWpnSlot(this);
		//wpn.setName("wpn_" + this.key);
		SetProcessInput(true);
	}
	
	public void removeWpn() {
		this.wpn.wpnSlot = null;
		this.wpn = null;
		SetProcessInput(false);
	}
	
	//4 TILES ADJACENT ERR

	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if (wpn != null) {
		//	wpn.setActive(active);
		//}
	}
	
	public void fire() {
	//	if (wpn != null) {
	//		wpn.fire();
	//	}
	}
}
