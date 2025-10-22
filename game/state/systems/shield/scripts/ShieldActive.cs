using Godot;
using System;

public partial class ShieldActive : Executing
{
	
	private Shield shield; 
	private Circuit circuit;
	
	public override void enter() {
		base.enter();
		shield = (Shield) base.sys;
		shield.setEnabled(true);
		//circuit = shield.circuit;
		//circuit.addCharge(-shield.load);
		//dGD.Print("ENTER " + dmg + " " + shield.hp);
	}
	
	public override void exit() {
		shield.setEnabled(false);
		base.exit();
	}
}
