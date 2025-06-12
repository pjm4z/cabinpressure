using Godot;
using System;

public partial class WeaponSlot : Area2D
{
	private Weapon wpn;
	public bool active = false;
	public string key;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		wpn = (Weapon) GetNode("weapon");
		float relPos = this.GlobalPosition.X - ((Boat)GetParent()).GlobalPosition.X;
		if (relPos < 0) {
			this.key = "1";
			//this.Rotation += (float) Math.PI;
			//wpn.Rotation += (float) Math.PI;
		} else {
			this.key = "2";
		}
		//wpn.Rotation = Rotation;
		ProcessMode = Node.ProcessModeEnum.Always;
	}

	public override void _Input(InputEvent inputEvent) {
		//if (inputEvent is InputEventMouseButton mouseEventL && 
		//	(int) mouseEventL.ButtonIndex == (int)MouseButton.Left && mouseEventL.Pressed) {
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
				wpn.queuedOrders = 0;
			} else if (Input.IsActionJustPressed(key)) {
				fire();
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (wpn != null) {
			wpn.active = active;
		}
	}
	
	public void fire() {
		wpn.fire();
	}
	
}
