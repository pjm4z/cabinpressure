using Godot;
using System;

[GlobalClass]
public partial class SysState : State
{
	protected ShipSystem sys;
	
	public override void ready() {
		base.ready();
		sys = (ShipSystem) base.parent;
	}
}
