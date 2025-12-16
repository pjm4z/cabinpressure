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
	private float prevRot = 0f;
	private double EarthMass = 5.972 * Math.Pow(10, 7); 
	private BaseScene game;
	private Space space;
	
	float circumference = 0f;
	float radius = 0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = (Sprite2D) GetNode("sprite");
		shape = (CollisionShape2D) GetNode("shape");
		brain = (StateMachine) GetNode("brain");
		
		game = (BaseScene) GetNode("/root/basescene");
		game.OriginShiftSignal += originShift;
		game.TreeLoadedSignal += treeLoaded;
		
		space = (Space) GetNode("%space");
		
		init();
		brain.init();
	}
	
	private void treeLoaded() {
		space.addBody(this);
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
		if (this.star == this) {
			predictPositions(10f, new Dictionary<string, Vector2>());
		}
	}
	
	public Vector2 getSize() {
		Vector2 aspectRatio = DisplayServer.WindowGetSize() / 
				Mathf.Min(DisplayServer.WindowGetSize().X, DisplayServer.WindowGetSize().Y);
				// new Vector2(1f/aspectRatio.X, 1f/aspectRatio.Y) * 
		return new Vector2(1f,1f) * ((CircleShape2D) shape.Shape).Radius;// * sprite.Scale;
	}
	
	public void treeReadyEvent(CelestialBody sysStar) {
	//	GlobalPosition *= sysStar.Scale;
		
		
		realPos = Position;// + this.star.realPos;
		
		radius = realPos.Length();//DistanceTo(this.star.realPos);
		circumference = (float) (2f * Math.PI * radius);
		
		this.Position = this.star.realPos;
		sprite.Position = realPos;
		shape.Position = realPos;
		prevPos = realPos;
		GD.Print(Name + " ::star-->:: " + sysStar.Name + " " + GlobalPosition + " " + realPos);
		//orbit /= sysStar.Scale.X;
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
	
	public Dictionary<string, Vector2> predictPositions(float t, Dictionary<string, Vector2> predMap) {
		Vector2 offset = Vector2.Zero;
		if (this.star != this) {
			offset = predMap[star.Name];
		}
		if (this.star != this) {
			predMap[Name] = predictPosition(t, offset);
		} else {
			predMap[Name] = GlobalPosition;
		}
		
		foreach (CelestialBody child in satellites.Values) {
			predMap = child.predictPositions(t, predMap);
		} 
		return predMap;
	}
	
	public Vector2 predictPosition(float t, Vector2 offset) {
		//float dist = (float) (radius * ((deltaRot * t)/delta));
		float dist = (radius * orbit * t) / 1000f;
		
		float theta = (float) (2 * Math.PI * dist / circumference);
		float theta0 = Mathf.Atan2(realPos.Y - GlobalPosition.Y, realPos.X - GlobalPosition.X);
		float theta1 = theta0 + theta;
		
		float x = GlobalPosition.X + radius * Mathf.Cos(theta1);
		float y = GlobalPosition.Y + radius * Mathf.Sin(theta1);
		Vector2 predPos = new Vector2(x, y);
		
		//GD.Print((predPos + offset) + " " + realPos + " !~! " + Name);
		return predPos + offset;
	}
	
	//private Vector2 originOffset;
	public void originShift(Vector2 offset) {
	//	this.originOffset = offset;
		prevPos -= offset;
	//	GlobalPosition -= originOffset;
		
	//	GD.Print("1111111111 " + Name);
	}
	
	Vector2 velocity = Vector2.Zero;
	float deltaRot = 0f;
	float delta = 0f;
	public float rotOff = 0f;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		delta = (float) delta;
		rotOff = sprite.GlobalRotation;
		sprite.GlobalRotation = 0f;
		realPos = sprite.GlobalPosition;
		brain.process(delta);
		//GravityPointCenter = ToLocal(realPos);
		
		Vector2 deltaPos = realPos - prevPos;
		velocity = deltaPos / (float) delta;
		prevPos = realPos;
		
		deltaRot = GlobalRotation - prevRot;
		prevRot = GlobalRotation;
		
		//speed = radius * (deltaRot/delta);
	//	float pred1 = (float) (radius * (deltaRot/delta));
	//	float pred2 = (radius * orbit) / 1000f;
	//	GD.Print(velocity.Length() + " !~! " + pred1 + " " + pred2 + " " + Name);
	}
	
	public Vector3 giveHeading(string name, Vector2 gPos, Vector3 heading, ref bool cont) {
		Vector2 dir = (realPos - gPos).Normalized();
		float dist = gPos.DistanceTo(realPos) / (star.Scale.X);
		//Vector2 diff = new Vector2(100f, 0f);// + (realPos - gPos);
		double gravity = Math.Pow(dist, 2);
		gravity = (mass * EarthMass) / gravity;
		Vector2 diff = Game.Instance.XY(heading);
		diff += (float) gravity * dir;
		heading.X += (float) Math.Round(diff.X);
		heading.Y += (float) Math.Round(diff.Y);
		
		if (!ships.ContainsKey(name)) {
			foreach (string key in satellites.Keys) {
				CelestialBody c = satellites[key];
				if (cont) {
					heading = c.giveHeading(name, gPos, heading, ref cont);
				}
				
			}
		} else {
			cont = false;
			heading = new Vector3((float) Math.Round(velocity.X),
				(float) Math.Round(velocity.Y), (float) Math.Round(mass));
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
