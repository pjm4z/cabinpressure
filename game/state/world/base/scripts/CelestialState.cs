using Godot;
using System;

[GlobalClass]
public partial class CelestialState : State
{
	[Export] CelestialState orbiting;
	
	protected CelestialBody celest;
	
	public override void ready() {
		base.ready();
		celest = (CelestialBody) base.parent;
	}
	
	public override State checkPriorities() {
		if (celest.shouldOrbit()) {
			return orbiting;
		}
		return null;
	}
}
