using Godot;
using System;

public partial class WeaponSlot : Area2D
{
	[Export] private Weapon wpn;
	public bool active = false;
	[Export] public string key;
	private Vector2I tilePos;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public void init(string key, Vector2I tilePos, Vector2 pos) {
		this.key = key;
		this.tilePos = tilePos;
		this.Position = pos;
	}
	
	public void setWpn(Weapon wpn) {
		this.wpn = wpn;
		wpn.setWpnSlot(this);
		wpn.setName("wpn_" + this.key);
		SetProcessInput(true);
	}
	
	public void removeWpn() {
		this.wpn.wpnSlot = null;
		this.wpn = null;
		SetProcessInput(false);
	}
	
	//4 TILES ADJACENT ERR

	public override void _Input(InputEvent inputEvent) {
		if ((Input.IsActionJustPressed("shift") && Input.IsActionPressed(key)) || 
				(Input.IsActionPressed("shift") && Input.IsActionJustPressed(key))) {
			if (active == false && wpn.canActivate()) {
				active = !active;
			} else if (active == true) {
				active = !active;
			}
		} else {
			if ((Input.IsActionJustPressed("ctrl") && Input.IsActionPressed(key)) || 
					(Input.IsActionPressed("ctrl") && Input.IsActionJustPressed(key))) {
				wpn.clear();
			} else if (Input.IsActionJustPressed(key)) {
				fire();
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (wpn != null) {
			wpn.setActive(active);
		}
	}
	
	public void fire() {
		if (wpn != null) {
			wpn.fire();
		}
	}
}
