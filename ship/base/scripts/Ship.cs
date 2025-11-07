using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ship : RigidBody2D
{
	[Signal]
	public delegate void JobCompletedEventHandler();
	
	public float MaxSpeed = 100.0f;  // Max forward speed
	public float MaxReverseSpeed = 100.0f;  // Max reverse speed
	public float TurnAcceleration = 0.01f;  // Rotation speed (turning speed)
	public float TurnSpeed = 10000f;
	public float Acceleration = 100.0f;  // Acceleration rate
	public float ReverseAcceleration = 100f;//10.0f;  // Reverse cceleration rate
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
	private Label accelLabel;
	private Label velLabel;
	private Label driveLabel;
	private Label headLabel;
	[Export] public CelestialBody star;
	
	private AnimatedSprite2D accelMeter;
	private AnimatedSprite2D velMeter;
	private AnimatedSprite2D driveMeter;
	private AnimatedSprite2D headMeter;
	
	public override void _Ready() {
		ZIndex = 1;
		InitialPosition = Position;
		rotationSpeed = 0;
		initBeds();
		shield.init(this);
		HBoxContainer panel = (HBoxContainer) GetNode("/root/basescene/hudcanvas/HUD/ship/shippanel");
		accelLabel = new Label();
		velLabel = new Label();
		driveLabel = new Label();
		headLabel = new Label();
		if (skip != null) {
			panel.AddChild(accelLabel);
			panel.AddChild(velLabel);
			panel.AddChild(driveLabel);
			panel.AddChild(headLabel);
		}
			accelMeter = (AnimatedSprite2D) panel.GetNode("acceleration");
			velMeter = (AnimatedSprite2D) panel.GetNode("velocity");
			driveMeter = (AnimatedSprite2D) panel.GetNode("drive");
			headMeter = (AnimatedSprite2D) panel.GetNode("heading");
		
		GlobalPosition *= star.Scale;
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
	private Vector2 pushForce = new Vector2(0,0);
	private float delta = 0f;
	public override void _PhysicsProcess(double delta) {
		this.delta = (float) delta;
		
		velocity = Game.Instance.zero;
		rotationSpeed = AngularVelocity;
		float speed = (LinearVelocity).Length(); 
		velLabel.Text = Math.Round(LinearVelocity.Length(), 2).ToString();
		headLabel.Text =  Math.Round(heading.Length(), 2).ToString();
		
		if (skip != null) { 
			// Handle rotation (turning the ship)
			if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
				if (Input.IsActionPressed("shift")) {
					if (MaxSpeed > LinearVelocity.Length()) {
						speed = MaxSpeed;
					}
					velocity = velocity.MoveToward(
						new Vector2(velocity.X, speed).Rotated(GlobalRotation - (float)Math.PI/2.0f), 
						Acceleration);
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
						Acceleration);
				} else {
					rotationSpeed = TurnSpeed;
				}
			}
			ApplyTorque(rotationSpeed);
			
			// Handle forward and reverse movement with acceleration
			if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
				// Accelerate forward based on the ship's rotation
				if (MaxSpeed > LinearVelocity.Length()) {
					speed = MaxSpeed;
				}
				velocity = velocity.MoveToward(
					new Vector2(velocity.X, speed).Rotated(GlobalRotation), 
					Acceleration);
			}
			if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
				// Accelerate in reverse based on the ship's rotation
				if (MaxReverseSpeed > LinearVelocity.Length()) {
					speed = MaxReverseSpeed;
				}
				velocity = velocity.MoveToward(
					new Vector2(velocity.X, speed).Rotated(GlobalRotation + (float)Math.PI), 
					Acceleration);
			}
			
			velocity = velocity.Normalized();
			dir = velocity;
			velocity *= Acceleration;
			ApplyCentralForce(velocity);
			driveLabel.Text = Math.Round(velocity.Length(), 2).ToString();
		}
		
		updateHeading();
		
	}
	
	Vector2 dir = new Vector2(0,0);
	
	public Vector2 heading = new Vector2(0,0);
	[Export] Ship target;
	public override void _IntegrateForces(PhysicsDirectBodyState2D state) {
		if (Input.IsActionPressed("c")) {
			Vector2 diff = state.LinearVelocity - heading;
			if (state.LinearVelocity.Length() <= 1f) {
				state.LinearVelocity = Game.Instance.zero;
			} else if (state.LinearVelocity.Length() <= 100f) {
				state.LinearVelocity -= new Vector2(diff.X / 10f, diff.Y / 10f);
			} else if (state.LinearVelocity.Length() <= 1000f) {
				state.LinearVelocity -= new Vector2(diff.X / 50f, diff.Y / 50f);
			} else {
				state.LinearVelocity -= new Vector2(diff.X / 100f, diff.Y / 100f);
			}
		} 
		
		if (Input.IsActionPressed("v")) {
			float diff = state.AngularVelocity;
			if (state.AngularVelocity <= 0.1f) {
				state.AngularVelocity -= diff / 10f;
			} else {
				state.AngularVelocity -= diff / 100f;
			}
		} 
		
		if (skip == null && target != null) {
			float angle = GetAngleTo(target.GlobalPosition) - 1.5708f;
			float rot = 0f;
			
			if (angle > 0f) {
				rot = 1f;
			} else if (angle < 0f) {
				rot = -1f;
			}
			
			float stopRotTime = Math.Abs(AngularVelocity) * Inertia / Math.Abs(TurnSpeed);
			float turnTime = Math.Abs(angle / AngularVelocity);
			
			if (stopRotTime >= turnTime - 1f) {
				ApplyTorque(-AngularVelocity * TurnSpeed);
			} else {
				ApplyTorque(rot * TurnSpeed);
			}
			
			/*float angularDiff = 0f;
			if (LinearVelocity.Length() > 1000f) {
				angularDiff = GetAngleTo(LinearVelocity) - angle;
				angularDiff = angle - (angularDiff /2f);
			}*/
			
			Vector2 v = velocity.MoveToward(
					target.GlobalPosition - GlobalPosition, 
					Acceleration);
					
			float newSpeed = Math.Abs(v.X);
			float curSpeed = Math.Abs(LinearVelocity.X);
			
			Vector2 dist = target.GlobalPosition + target.LinearVelocity - GlobalPosition;
			dist.X = Math.Abs(dist.X);
			dist.Y = Math.Abs(dist.Y);
			Vector2 ratio = dist.Normalized();
			dist -= new Vector2(1000f * ratio.X, 1000f * ratio.Y);
			
			float decelTime = curSpeed / Acceleration;
			float traverseTime = dist.X / curSpeed;
			float targetDecelTime =  Math.Abs(target.LinearVelocity.X) / target.Acceleration; 
			
			
			if (decelTime >= traverseTime + targetDecelTime - 1f) {
				if (curSpeed > newSpeed) {
					v.X = LinearVelocity.Normalized().X - (LinearVelocity.Normalized().X * Acceleration);
				} else {
					v.X = -LinearVelocity.X;
				} 
			}
			
			newSpeed = Math.Abs(v.Y);
			curSpeed = Math.Abs(LinearVelocity.Y);
			
			decelTime = curSpeed / Acceleration;
			traverseTime = dist.Y / curSpeed;
			targetDecelTime =  Math.Abs(target.LinearVelocity.Y) / target.Acceleration; 
			
			if (decelTime >= traverseTime + targetDecelTime - 1f) {
				if (curSpeed > newSpeed) {
					v.Y = LinearVelocity.Normalized().Y - (LinearVelocity.Normalized().Y * Acceleration);
				} else {
					v.Y = -LinearVelocity.Y;
				} 
			}
			v = v.Normalized() * Acceleration;
			driveLabel.Text = Math.Round(v.Length(), 2).ToString();
			//updateMeters(state);
			ApplyCentralForce(v);
		}
		if (skip != null) {
			updateMeters(state);
		}
	}
	
	private Vector2 prevVel = new Vector2(0,0);
	
	private void updateMeters(PhysicsDirectBodyState2D state) {
		Vector2 accel = (state.LinearVelocity - prevVel);
		accelLabel.Text = Math.Round(accel.Length(), 2).ToString();
		if (accel.Length() >= 0.1f) {
			prevVel = state.LinearVelocity;
			accelMeter.Frame = 0;
			accelMeter.Rotation = accel.Angle() + 1.5708f;
		} else {
			accelMeter.Frame = 1;
			accelMeter.Rotation = 1.5708f;
		}
		
		if (state.LinearVelocity.Length() >= 0.1f) {
			velMeter.Frame = 0;
			velMeter.Rotation = state.LinearVelocity.Angle() + 1.5708f;
		} else {
			velMeter.Frame = 1;
			velMeter.Rotation = 1.5708f;
		}
		
		if (dir.Length() != 0f) {
			driveMeter.Frame = 0;
			driveMeter.Rotation = dir.Angle() + 1.5708f;
		} else {
			driveMeter.Frame = 1;
			driveMeter.Rotation = 1.5708f;
		}
		
		if (heading.Length() >= 0.1f) {
			headMeter.Frame = 0;
			headMeter.Rotation = heading.Angle() + 1.5708f;
		} else {
			headMeter.Frame = 1;
			headMeter.Rotation = 1.5708f;
		}
	}
	
	
	
	public Vector2 updateHeading() {
		if (star != null && this.timer) {
			Vector3 desiredHeading = new Vector3(0f, 0f, 0f);
			//if () {
				desiredHeading = star.giveHeading(Name, GlobalPosition, desiredHeading);
			//}
			Vector2 diff = (Game.Instance.XY(desiredHeading) - heading) ;/// 100f;
			
			//heading += diff;
			
			//if (desiredHeading.Length() < heading.Length()) {
				//diff *= -0f;
			//	heading = desiredHeading;
			//}
			heading = Game.Instance.XY(desiredHeading);
			if (desiredHeading.Z != 0f) {
				Vector2 vel = LinearVelocity;
				bool apply = false;
				float factor = 0;
				float x = 1f;
				
				if (heading.X < -x) {
					if (vel.X > heading.X) {
						apply = true;
						factor = -1f;
					}
				} else if (heading.X > x) {
					if (vel.X < heading.X) {
						apply = true;
						factor = 1f;
					}
				} 
				x *= (factor);
				//GD.Print("! " + vel.X + " " + heading.X + " "  + apply + " " + Math.Abs(vel.X - heading.X) + " " + factor);
				if (apply) {
					if (((heading.X < 0) && (x + vel.X < heading.X)) ||
						((heading.X > 0) && (x + vel.X > heading.X))) {
							x =  heading.X - vel.X;
					}
					ApplyCentralForce(new Vector2(x, 0f));// * desiredHeading.Z);
				} 
				apply = false;
				factor = 0;
				float y = 1;
				
				if (heading.Y < -y) {
					if (vel.Y > heading.Y) {
						apply = true;
						factor = -1f;
					}
				} else if (heading.Y > y) {
					if (vel.Y < heading.Y) {
						apply = true;
						factor = 1f;
					}
				} 
				y *= (factor);
				if (apply) {//} && Math.Abs(vel.Y - heading.Y) >= 0.1f && Math.Abs(vel.Y + heading.Y) >= 0.1f) {
					if (((heading.Y < 0) && (y + vel.Y < heading.Y)) ||
						((heading.Y > 0) && (y + vel.Y > heading.Y))) {
						y =  heading.Y - vel.Y;
					}
					ApplyCentralForce(new Vector2(0, y));// * desiredHeading.Z);
				} 
			} else {
				ApplyCentralForce(heading);
			}
			
			return diff;
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
