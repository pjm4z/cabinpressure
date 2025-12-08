using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Torpedo : Area2D
{
	public float Speed = 400.0f;
	public int Damage = 100;
	
	private Vector2 initPos;
	private Vector2 mousePos;
	private Vector2 targetPos;
	private Vector2 init_velocity;
	private Vector2 total_velocity;
	private double lifespan;
	private bool dead = false;
	
	private Dictionary<string, Node> bodies = new Dictionary<string, Node>();
	
	[Export] private CollisionPolygon2D collision;
	[Export] private CollisionShape2D radius;
	
	[Export] private Sprite2D sprite;
	[Export] private CpuParticles2D explosion;
	[Export] private GpuParticles2D wake;
	
	public Ship ship;
	
	public override void _Ready() {
		
		
		radius.SetDeferred("disabled", true);
		
		initPos = Position;
		Vector2 base_velocity = new Vector2(0, Speed).Rotated(Rotation - 1.5708f);
		total_velocity = init_velocity + base_velocity;
		lifespan = initPos.DistanceTo(mousePos)/Math.Sqrt((base_velocity.X * base_velocity.X) + (base_velocity.Y * base_velocity.Y));

		wake.Amount = 10;
		ParticleProcessMaterial wakeMaterial = (ParticleProcessMaterial) wake.ProcessMaterial;
		wakeMaterial.Gravity = Vector3.Zero;
		wakeMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
		wakeMaterial.EmissionBoxExtents = new Vector3(5, 1, 1);
		wake.Lifetime = 0.5f;
		wakeMaterial.LifetimeRandomness = 1.0f;
		
		Timer timer = new Timer();
		timer.WaitTime = 10f; // Extra buffer
		timer.OneShot = true;
		timer.Autostart = true;
		timer.Timeout += () => GracefulQF();
		AddChild(timer);
		timer.Start();
		GlobalPosition = initPos;
		GD.Print("WPN EXWEC 5 " + GlobalPosition + " "  + initPos);
	}
	
	public void init(Ship ship, Vector2 init_velocity, Vector2 mousePos) {
		GD.Print("WPN EXWEC 3 " + ship.GlobalPosition + " " + GlobalPosition);
		this.ship = ship;
		this.init_velocity = init_velocity;
		this.mousePos = mousePos;
	}

	
	public override void _PhysicsProcess(double delta) {
		GD.Print("WPN EXWEC 4 " + GlobalPosition);
   		GlobalPosition += total_velocity * (float) delta;
		lifespan -= delta;
		if (shouldDetonate()) {
			GracefulQF();
		}
	}
	
	public virtual bool shouldDetonate() {
		return (lifespan <= 0 && !dead && bodies.Count > 0);
	}
	
	public void _on_body_entered(Node body) {
		/*if (body is RigidShield) {
			GD.Print("SHIELD");
			GracefulQF();
		}*/
		if (!bodies.ContainsKey(body.Name)) {
			bodies.Add(body.Name, body);
		}
	}
	
	public void _on_area_entered(Area2D body) {
		if (!bodies.ContainsKey(body.Name)) {
			bodies.Add(body.Name, body);
		}
		if (body is ShieldArea) {
			ShieldArea shield = (ShieldArea) body;
			if (shield.ship.Name != this.ship.Name) {
				bodies = new Dictionary<string, Node>();
				bodies.Add(shield.Name, shield);
				GD.Print("SHIELD " + shield.ship.Name + " " + ship.Name + " " + bodies.Count);
				GracefulQF();
			} else {
				bodies.Remove(shield.Name);
			}
		}		
	}
	
	public void _on_area_exited(Area2D body) {
		bodies.Remove(body.Name);
		GD.Print(bodies.Count);
	}
	
	public void _on_body_exited(Node body) {
		bodies.Remove(body.Name);
		GD.Print(bodies.Count);
	}
	
	public async void GracefulQF() {
		GD.Print("!!!1");
		if (!dead) {
			GD.Print("???2");
		this.dead = true;
		radius.SetDeferred("disabled", false);
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		if (bodies.Count > 0) {
			
			GD.Print("???33");
			GD.Print("HIT " + bodies.Count);
		}
		Game.Instance.HasProperty(this, "Name");
		foreach (string key in bodies.Keys) {
			GD.Print(key);
			Node body = bodies[key];
			if (Game.Instance.HasProperty(body, "Name")) {
				GD.Print("TARGET NAME " + body.Name);
			}
			if (body is ShieldArea) {
				GD.Print("Shield " + body.Name);
				//body.applyDamage(GlobalPosition, radius, damage);
				((ShieldArea)body).damage(Damage);
			}
			if (body is Crew) {
				GD.Print("Crew " + body.Name);
				//body.QueueFree();
				//body.applyDamage(GlobalPosition, radius, damage);
			}
			if (body is GridItem) {
				GD.Print("GI " + body.Name);
				//body.QueueFree();
				//body.applyDamage(GlobalPosition, radius, damage);
			}
			
			if (body is Ship) {
				GD.Print("Ship " + body.Name);
				//body.applyDamage(GlobalPosition, radius, damage);
				((Ship)body).damageOuter(GlobalPosition, ((CircleShape2D) radius.Shape).Radius, Damage);
			}
		}
		
		explosion.Emitting = true;
		sprite.Visible = false;
		wake.Emitting = false;
		Speed = 0;
		Timer timer = new Timer();
		timer.WaitTime = wake.Lifetime + 1.0f; // Extra buffer
		timer.OneShot = true;
		timer.Timeout += () => QueueFree();
		AddChild(timer);
		timer.Start();
		
		}
	}
}
