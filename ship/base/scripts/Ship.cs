using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ship : RigidBody2D
{
	[Signal]
	public delegate void JobCompletedEventHandler();
	
	public const float MaxSpeed = 100.0f;  // Max forward speed
	public const float MaxReverseSpeed = 100.0f;  // Max reverse speed
	public const float TurnAcceleration = 0.01f;  // Rotation speed (turning speed)
	public const float TurnSpeed = 100f;
	public const float Acceleration = 100.0f;  // Acceleration rate
	public const float ReverseAcceleration = 100f;//10.0f;  // Reverse cceleration rate
	private Vector2 velocity = Vector2.Zero;
	
	public Vector2 InitialPosition;
	
	public bool active = true;
	
	public Queue<Furniture> availableBeds = new Queue<Furniture>();  // TODO --> change to bed when i have bed class
	private List<Furniture> takenBeds = new List<Furniture>();
	[Export] public Camera2D camera;
	[Export] public CrewRoster defaultRoster;
	[Export] private MapCtrl mapCtrl;
	public float rotationSpeed;
	[Export] private Skip skip;
	[Export] private Shield shield;
	[Export] public bool shieldEnabled = false;
	[Export] public int hp = 1000;
	private Label speedLabel;
	private Label headingLabel;
	[Export] public CelestialBody star;
	
	private Sprite2D velocityIndicator;
	private Sprite2D headingIndicator;
	
	public override void _Ready() {
		ZIndex = 1;
		InitialPosition = Position;
		rotationSpeed = 0;
		initBeds();
		shield.init(this);
		HBoxContainer panel = (HBoxContainer) GetNode("/root/basescene/hudcanvas/HUD/ship/shippanel");
		speedLabel = new Label();
		headingLabel = new Label();
		
		panel.AddChild(speedLabel);
		panel.AddChild(headingLabel);
		velocityIndicator = (Sprite2D) panel.GetNode("velocity");
		headingIndicator = (Sprite2D) panel.GetNode("heading");
	}
	
	public void initBeds() {
		var bedArray = GetChildren()
			.Where(child => child is Furniture) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<Furniture>(); // TODO --> change to bed when I have bed class                 

		foreach(var bed in bedArray) {
			availableBeds.Enqueue(bed);
		}
	}
	
	public void giveBed(Crew crew) {
		if (availableBeds.Count > 0) {
			Furniture bed = availableBeds.Dequeue();
			crew.bed = bed;
			bed.crew = crew;
			takenBeds.Add(bed);
		} 
	}

	public override void _Input(InputEvent inputEvent) { }
	
	public void _on_area_body_entered(Node body) { }
	
	public Vector2 inputMap = new Vector2(0,0);
	
	public override void _PhysicsProcess(double delta) {
		
		
		velocity = inputMap;// * LinearVelocity.Normalized()).Normalized(); //
		rotationSpeed = AngularVelocity;
		float speed = (LinearVelocity).Length(); // - heading
		speedLabel.Text = Math.Round(LinearVelocity.Length(), 2).ToString();//Math.Round((LinearVelocity), 2).ToString(); //.Length(
		headingLabel.Text =  Math.Round(heading.Length(), 2).ToString();
		//LinearVelocity.Length();
		
		if (skip != null) { 
			// Handle rotation (turning the ship)
			if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
				if (Input.IsActionPressed("shift")) {
					if (MaxSpeed > LinearVelocity.Length()) {
						speed = MaxSpeed;
					}
					velocity = velocity.MoveToward(
						new Vector2(velocity.X, speed).Rotated(GlobalRotation - (float)Math.PI/2.0f), 
						Acceleration * (float)delta);
				} else {
					rotationSpeed = -1 * TurnSpeed;
				}
			}
			if (Input.IsActionPressed("ui_right") || Input.IsActionPressed("d")) {
				if (Input.IsActionPressed("shift")) {
					if (MaxSpeed > LinearVelocity.Length()) {
						speed = MaxSpeed;
					}
					velocity = velocity.MoveToward(
						new Vector2(velocity.X, speed).Rotated(GlobalRotation + (float)Math.PI/2.0f), 
						Acceleration * (float)delta);
				} else {
					rotationSpeed = TurnSpeed;
				}
			}
			ApplyTorque(rotationSpeed * 100); 
			
			// Handle forward and reverse movement with acceleration
			if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
				// Accelerate forward based on the ship's rotation
				if (MaxSpeed > LinearVelocity.Length()) {
					speed = MaxSpeed;
				}
				velocity = velocity.MoveToward(
					new Vector2(velocity.X, speed).Rotated(GlobalRotation), 
					Acceleration * (float)delta);
			}
			if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
				// Accelerate in reverse based on the ship's rotation
				if (MaxReverseSpeed > LinearVelocity.Length()) {
					speed = MaxReverseSpeed;
				}
				velocity = velocity.MoveToward(
					new Vector2(velocity.X, speed).Rotated(GlobalRotation + (float)Math.PI), 
					ReverseAcceleration * (float)delta);
			}
		} 
		//velocity += heading;
		ApplyCentralForce(velocity * 100f);// * new Vector2(100f,100f));
	}
	
	public void LookAt(Vector2 target) {
		float angle = GetAngleTo(target) - 1.5708f;
		float degrees = Mathf.RadToDeg(angle);
		float torque = angle;
		if (degrees < 0) {
			torque *= -1;
		}
		ApplyTorque(-torque * 100);
		GD.Print("LOOK " + degrees + " " + angle + " " + torque);
	}
	public Vector2 heading = new Vector2(0,0);
	public override void _IntegrateForces(PhysicsDirectBodyState2D state) {
		Vector2 df = updateHeading();
		ApplyCentralForce(heading);
		if (Input.IsActionPressed("c")) {
			Vector2 diff = state.LinearVelocity - heading;
			state.LinearVelocity -= new Vector2(diff.X / 100f, diff.Y / 100f);
		} 
		
		if (skip == null) {
			state.AngularVelocity = GetAngleTo(new Vector2(0,0)) - 1.5708f;
		}
		
		velocityIndicator.Rotation = state.LinearVelocity.Angle() + 1.5708f;
		headingIndicator.Rotation = heading.Angle() + 1.5708f;
	}
	
	public Vector2 updateHeading() {
		if (star != null) {
			Vector2 desiredHeading = Game.Instance.zero;
			if (this.timer) {
				desiredHeading = star.giveHeading(Name, GlobalPosition, Game.Instance.zero);
			}
			heading = desiredHeading;
			return desiredHeading - heading;
		} else {
			return Game.Instance.zero;
		}
		
	}

	public void damageOuter(Vector2 gPos, double radius, int damage) {
		mapCtrl.damageOuter(gPos, radius, damage);
	}
	
	public void damageInner(Vector2 gPos, double radius, int damage) {
		mapCtrl.damageInner(gPos, radius, damage);
	}
	
	public void changeHP(int hp) {
		this.hp += hp;
		if (this.hp < 0) {
			QueueFree();
		}
	}
	private bool timer = false;
	public void _on_timer_timeout() {
		GD.Print("TIMER");
		this.timer = true;
	}
}
