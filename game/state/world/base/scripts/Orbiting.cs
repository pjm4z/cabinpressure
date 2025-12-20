using Godot;
using System;

public partial class Orbiting : CelestialState
{
	[Export] CelestialState resting;
	private float orbit;
	private float dampening = 0.001f;
	
	public override void enter() {
		base.enter();
		this.orbit = celest.orbit;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override State process(double delta) {
		if (orbit > 0f) {
			celest.GlobalRotation += (float) (((Math.Tau * orbit) / 10000f) * delta) - celest.star.rotOff;
		}
		return base.process(delta);
	}
	
	public override State checkPriorities() {
		if (!celest.shouldOrbit()) {
			return resting;
		}
		return null;
	}
}
