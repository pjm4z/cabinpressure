using Godot;
using System;

public partial class Firing : Executing
{
	private Weapon wpn; 
	
	public override void enter() {
	
		base.enter();
		wpn = (Weapon) base.sys;
	}
	
	public override State process(double delta) {
			
		wpn.LookAt(wpn.GetGlobalMousePosition());
	//	GD.Print("WPN EXEC");
		return base.process(delta);
	}
}
