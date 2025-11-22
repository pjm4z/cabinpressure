using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CelestialBody : Area2D
{
	[Export] public float orbit;
	[Export] public float mass;
	public CelestialBody star = null;
	public Dictionary<string, CelestialBody> satellites = new Dictionary<string, CelestialBody>();
	public Dictionary<string, Ship> ships = new Dictionary<string, Ship>();
	private StateMachine brain;
	private CollisionShape2D shape;
	private Sprite2D sprite;
	private Vector2 prevPos = Vector2.Zero;
	public Vector2 realPos = Vector2.Zero;
	private double EarthMass = 5.972 * Math.Pow(10, 6); 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = (Sprite2D) GetNode("sprite");
		shape = (CollisionShape2D) GetNode("shape");
		brain = (StateMachine) GetNode("brain");
		
		init();
		brain.init();
	}
	
	private void init() {
		GlobalPosition *= Scale;
		Node2D parent = (Node2D) GetParent();
		if (parent is CelestialBody) {
			CelestialBody body = (CelestialBody) parent;
			this.star = body;
			float dist = GlobalPosition.DistanceTo(body.GlobalPosition);
			
			realPos = GlobalPosition;
			this.GlobalPosition = this.star.GlobalPosition;
			sprite.GlobalPosition = realPos;
			shape.GlobalPosition = realPos;
			prevPos = realPos;
			
			orbit /= star.Scale.X;
		} else {
			this.star = this;
		}
		initSatellites();
	}
	
	public void initSatellites() {
		var childArray = GetChildren()
			.Where(child => child is CelestialBody) 
			.Select(child => child)          
			.Cast<CelestialBody>();                 

		foreach(var child in childArray) {
			 satellites[child.Name] = child;
		}
	}
	
	Vector2 velocity = Vector2.Zero;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		sprite.GlobalRotation = 0f;
		realPos = sprite.GlobalPosition;
		brain.process(delta);
		GravityPointCenter = ToLocal(realPos);
		
		Vector2 deltaPos = realPos - prevPos;
		velocity = deltaPos / (float) delta;
		prevPos = realPos;
	}
	
	public Vector3 giveHeading(string name, Vector2 gPos, Vector3 heading) {
		Vector2 dir = (realPos - gPos).Normalized();
		float dist = gPos.DistanceTo(realPos) / (star.Scale.X);
		//Vector2 diff = new Vector2(100f, 0f);// + (realPos - gPos);
		double gravity = Math.Pow(dist, 2);
		gravity = (mass * EarthMass) / gravity;
		Vector2 diff = Game.Instance.XY(heading);
		diff += (float) gravity * dir;
		heading.X += (float) Math.Round(diff.X);
		heading.Y += (float) Math.Round(diff.Y);
		//heading *= 4f;
		if (!ships.ContainsKey(name)) {
			foreach (string key in satellites.Keys) {
				CelestialBody c = satellites[key];
				heading = c.giveHeading(name, gPos, heading);
			}
		} else {
			heading = new Vector3((float) Math.Round(velocity.X),
				(float) Math.Round(velocity.Y),
				(float) Math.Round(mass));
		}
		
		return heading; 
	} 
	
	public bool shouldOrbit() {
		return star != this;
	}
	
	public void _on_body_entered(Node body) {
		if (body is Ship) {
			Ship ship = (Ship) body;
			ships[ship.Name] = ship;
		}
	}
	
	public void _on_body_exited(Node body) {
		if (body is Ship) {
			Ship ship = (Ship) body;
			ships.Remove(ship.Name);
		}
	}
}
