using Godot;
using System;
using System.Collections.Generic;

public partial class Space : Node2D
{
	public CelestialBody star;
	public Dictionary<string, CelestialBody> bodies;
	public List<Ship> ships;
	
	int bodySeq = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		bodies = new Dictionary<string, CelestialBody>();
		ships = new List<Ship>();
	}
	
	public void addBody(CelestialBody body) {
		if (bodies.ContainsKey(body.Name)) {
			body.Name = body.Name + bodySeq;
			bodySeq++;
		}
		bodies[body.Name] = body;
	}
}
