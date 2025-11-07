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
	public Sprite2D sprite;
	private StateMachine brain;
	private CollisionShape2D shape;
	private Area2D innerArea;
	Vector2 prevPos = new Vector2(0,0);
	
	[Export] float hillSphere;
	
	private double EarthMass = 5.972 * Math.Pow(10, 7); 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = (Sprite2D) GetNode("sprite");
		shape = (CollisionShape2D) GetNode("shape");
		brain = (StateMachine) GetNode("brain");
		innerArea = (Area2D) GetNode("innerarea");
		GlobalPosition *= Scale;
		init();
		brain.init();
	}
	
	private void init() {
		Node2D parent = (Node2D) GetParent();
		if (parent is CelestialBody) {
			GD.Print("PARENT IS CELEST");
			CelestialBody body = (CelestialBody) parent;
			this.star = body;
			float dist = GlobalPosition.DistanceTo(body.GlobalPosition);
			float radius = dist * (mass/(3 * body.mass)) / (star.Scale.X / 10f);
			shape.Scale *= new Vector2(radius,radius);
			GD.Print("HILL SPHERE RADIUS " + radius + " " + shape.Scale);
			
			
			Vector2 RealPosition = GlobalPosition;
			this.GlobalPosition = this.star.GlobalPosition;
			sprite.GlobalPosition = RealPosition;
			shape.GlobalPosition = RealPosition;
			innerArea.GlobalPosition = RealPosition;
			prevPos = RealPosition;
			
			orbit /= star.Scale.X;
		} else {
			shape.Scale *= hillSphere;
			this.star = this;
		}
		initSatellites();
	}
	
	
	
	public void initSatellites() {
		var childArray = GetChildren()
			.Where(child => child is CelestialBody) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<CelestialBody>(); // TODO --> change to bed when I have bed class                 

		foreach(var child in childArray) {
			 satellites[child.Name] = child;
		}
	}
	
	Vector2 velocity = new Vector2(0,0);
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		brain.process(delta);
		GravityPointCenter = sprite.Position;
		
		Vector2 deltaPos = sprite.GlobalPosition - prevPos;
		velocity = deltaPos / (float) delta;
		prevPos = sprite.GlobalPosition;
	}
	
	public Vector3 giveHeading(string name, Vector2 gPos, Vector3 heading) {
		//GD.Print("GIVE HEADING1 " + Name +  " " + mass + " " + sprite.GlobalPosition.DistanceTo(gPos));
		//Vector2 heading = Game.Instance.zero;
		
		Vector2 dir = (sprite.GlobalPosition - gPos).Normalized();
		float dist = gPos.DistanceTo(sprite.GlobalPosition) / (star.Scale.X);
		double gravity = Math.Pow(dist, 2);
		gravity = (mass * EarthMass) / gravity;
		//GD.Print("??? " + Name + " " + ships.ContainsKey(name) + " " + star.Scale + " " + dir + " " +  dist + " " + " " + gravity + " " + ((float) gravity * dir));
		Vector2 diff = Game.Instance.XY(heading);
		diff += (float) gravity * dir;
		heading.X += diff.X;
		heading.Y += diff.Y;
		if (!ships.ContainsKey(name)) {
			//GD.Print("NOT IN CORE " + name + " " + ships.ContainsKey(name));
			foreach (string key in satellites.Keys) {
				CelestialBody c = satellites[key];
				heading = c.giveHeading(name, gPos, heading);
				//GD.Print("GIVE HEADING2 " + c.Name + " " + c.mass + " " + c.sprite.GlobalPosition.DistanceTo(gPos));
			}
		} else {
			//GD.Print("in core");
			heading = new Vector3(velocity.X, velocity.Y, mass);
		}
		
		//heading = Game.Instance.zero;
		
		return heading; 
	} 
	
	public bool shouldOrbit() {
		return star != this;
	}
	
	public void _on_body_entered(Node body) {
		if (body is Ship) {
			Ship ship = (Ship) body;
			if (ship.star != null) {
				if (!satellites.ContainsKey(ship.star.Name)) {
					//ship.star = this;
				}
			}
		}
	}
	
	public void _on_innerarea_body_entered(Node body) {
		if (body is Ship) {
			Ship ship = (Ship) body;
			//ship.star = this;
			ships[ship.Name] = ship;
			//ship.Reparent(this);
			//GD.Print("REPARENT " + ships.ContainsKey(ship.Name) + " " + ship.Name);
		}
	}
	
	public void _on_innerarea_body_exited(Node body) {
		if (body is Ship) {
			Ship ship = (Ship) body;
			ships.Remove(ship.Name);
		}
	}
	
	public void _on_body_exited(Node body) {
		if (body is Ship) {
			Ship ship = (Ship) body;
		//	ship.star = this.star;
		}
	}
}
