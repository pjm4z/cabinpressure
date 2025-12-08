using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CelestialBody : Area2D
{
	[Signal]
	public delegate void InitSignalEventHandler(CelestialBody sysStar);
	
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
	private double EarthMass = 5.972 * Math.Pow(10, 7); 
	private BaseScene game;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = (Sprite2D) GetNode("sprite");
		shape = (CollisionShape2D) GetNode("shape");
		brain = (StateMachine) GetNode("brain");
		game = (BaseScene) GetNode("/root/basescene");
		game.OriginShiftSignal += originShift;
		
		init();
		brain.init();
	}
	
	private void init() {
		Node2D parent = (Node2D) GetParent();
		if (parent is CelestialBody) {
			CelestialBody body = (CelestialBody) parent;
			this.star = body;
			this.star.InitSignal += treeReadyEvent;
		} else {
			this.star = this;
			GlobalPosition *= Scale;
			EmitSignal(nameof(SignalName.InitSignal), this);
		}
		initSatellites();
	}
	
	public void treeReadyEvent(CelestialBody sysStar) {
	//	GlobalPosition *= sysStar.Scale;
		float dist = GlobalPosition.DistanceTo(this.star.realPos);
			
		realPos = GlobalPosition + this.star.realPos;
		this.GlobalPosition = this.star.realPos;
		sprite.GlobalPosition = realPos;
		shape.GlobalPosition = realPos;
		prevPos = realPos;
		GD.Print(Name + " ::star-->:: " + sysStar.Name + " " + GlobalPosition + " " + realPos);
		orbit /= sysStar.Scale.X;
		EmitSignal(nameof(SignalName.InitSignal), sysStar);
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
	
	//private Vector2 originOffset;
	public void originShift(Vector2 offset) {
	//	this.originOffset = offset;
		prevPos -= offset;
	//	GlobalPosition -= originOffset;
		
	//	GD.Print("1111111111 " + Name);
	}
	
	Vector2 velocity = Vector2.Zero;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		sprite.GlobalRotation = 0f;
		realPos = sprite.GlobalPosition;
		brain.process(delta);
		GravityPointCenter = ToLocal(realPos);
		
	//	if (originOffset != Vector2.Zero) {
	//		prevPos -= originOffset;
	//		originOffset = Vector2.Zero;
			
	//		GD.Print(" 1.51.51.51.51.5 " + Name + " " +  velocity.Length());
	//	} else {
			Vector2 deltaPos = realPos - prevPos;
			velocity = deltaPos / (float) delta;
			prevPos = realPos;
			if (velocity.Length() > 1000f) {
	//			GD.Print(" 22222222 " + Name + " " + velocity.Length());
			}
	//	}
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
				if (velocity.Length() > 1000f) {
	//				GD.Print("333333333");
				}
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
