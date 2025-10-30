using Godot;
using System;

[GlobalClass]
public partial class CelestialState : State
{
	protected CelestialBody celest;
	
	public override void ready() {
		base.ready();
		celest = (CelestialBody) base.parent;
	}
}
