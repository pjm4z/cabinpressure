using Godot;
using System;

public partial class ShieldOccupied : Occupied
{
	private Shield shield;
	
	public override void enter() {
		base.enter();
		this.shield = (Shield) base.sys;
	}
	
	public override State process(double delta) {
		GD.Print("shield occ");
		
		//handleLoad((float)delta);
		
		return base.process(delta);
	}
}
