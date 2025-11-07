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
	public override State process(double delta)
	{
		celest.GlobalRotation += orbit * (float) delta * dampening;
		return base.process(delta);
	}
	
	public override State checkPriorities() {
		if (!celest.shouldOrbit()) {
			return resting;
		}
		return null;
	}
}
