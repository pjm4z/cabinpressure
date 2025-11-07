using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ship : RigidBody2D
{	
	[Export] public Camera2D camera;
	[Export] public CrewRoster defaultRoster;
	[Export] private MapCtrl mapCtrl;
	
	public float TurnSpeed = 10000f;
	public float Acceleration = 100.0f; 
	private float angularDrive = 0f;
	private Vector2 linearDrive = Vector2.Zero;
	private Vector2 heading = Vector2.Zero;
	
	[Export] public CelestialBody star;
	[Export] private Ship target;
	[Export] private Skip skip;
	[Export] private Shield shield;
	[Export] public bool shieldEnabled = false;
	[Export] public int hp = 1000;
	
	public Queue<Furniture> availableBeds = new Queue<Furniture>();  // TODO --> change to bed when i have bed class
	private List<Furniture> takenBeds = new List<Furniture>();
	
	private Label accelLabel;
	private Label velLabel;
	private Label driveLabel;
	private Label headLabel;
	private AnimatedSprite2D accelMeter;
	private AnimatedSprite2D velMeter;
	private AnimatedSprite2D driveMeter;
	private AnimatedSprite2D headMeter;
	
	public override void _Ready() {
		HBoxContainer panel = (HBoxContainer) GetNode("/root/basescene/hudcanvas/HUD/ship/shippanel");
		accelMeter = (AnimatedSprite2D) panel.GetNode("acceleration");
		velMeter = (AnimatedSprite2D) panel.GetNode("velocity");
		driveMeter = (AnimatedSprite2D) panel.GetNode("drive");
		headMeter = (AnimatedSprite2D) panel.GetNode("heading");
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
		
		GlobalPosition *= star.Scale;
		initBeds();
		shield.init(this);
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

	public override void _Input(InputEvent inputEvent) { base._Input(inputEvent); }
	
	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);
		linearDrive = Vector2.Zero;
		angularDrive = AngularVelocity;
		velLabel.Text = Math.Round(LinearVelocity.Length(), 2).ToString();
		headLabel.Text =  Math.Round(heading.Length(), 2).ToString();
		
		if (skip != null) { 
			// Handle rotation (turning the ship)
			if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
				if (Input.IsActionPressed("shift")) {
					linearDrive += new Vector2(0f,1f).Rotated(GlobalRotation - (float)Math.PI/2f);
				} else {
					angularDrive = -TurnSpeed;
				}
			}
			if (Input.IsActionPressed("ui_right") || Input.IsActionPressed("d")) {
				if (Input.IsActionPressed("shift")) {
					linearDrive += new Vector2(0f,1f).Rotated(GlobalRotation + (float)Math.PI/2f);
				} else {
					angularDrive = TurnSpeed;
				}
			}
			
			// Handle forward and reverse movement with acceleration
			if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
				linearDrive += new Vector2(0f,1f).Rotated(GlobalRotation);
			}
			if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
				linearDrive += new Vector2(0f,1f).Rotated(GlobalRotation + (float)Math.PI);
			}
			linearDrive = linearDrive.Normalized() * Acceleration;
			ApplyCentralForce(linearDrive);
			ApplyTorque(angularDrive);
			driveLabel.Text = Math.Round(linearDrive.Length(), 2).ToString();
			
		}
		updateHeading();
	}
	
	public override void _IntegrateForces(PhysicsDirectBodyState2D state) {
		if (Input.IsActionPressed("c")) {
			Vector2 diff = state.LinearVelocity - heading;
			if (state.LinearVelocity.Length() <= 1f) {
				state.LinearVelocity = Vector2.Zero;
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
			float stopRotTime = Math.Abs(AngularVelocity) * Inertia / Math.Abs(TurnSpeed);
			float turnTime = Math.Abs(angle / AngularVelocity);
			
			if (stopRotTime >= turnTime - 1f) {
				ApplyTorque(-AngularVelocity * TurnSpeed);
			} else {
				float rot = 0f;
				if (angle > 0f) {
					rot = 1f;
				} else if (angle < 0f) {
					rot = -1f;
				}
				ApplyTorque(rot * TurnSpeed);
			}
			
			Vector2 dist = target.GlobalPosition + target.LinearVelocity - GlobalPosition;
			dist.X = Math.Abs(dist.X);
			dist.Y = Math.Abs(dist.Y);
			Vector2 ratio = dist.Normalized();
			dist -= new Vector2(1000f * ratio.X, 1000f * ratio.Y);
			
			float angularDiff = 0f;
			//if (LinearVelocity.Length() > 1000f) {
			//	angularDiff = GetAngleTo(LinearVelocity) - angle;
			//	angularDiff = angle - (angularDiff /2f);
			//}
			
			linearDrive = (target.GlobalPosition - GlobalPosition);//.Rotated(angularDiff);
			float newSpeed = Math.Abs(linearDrive.X);
			float curSpeed = Math.Abs(LinearVelocity.X);
			
			float decelTime = curSpeed / Acceleration;
			float traverseTime = (dist.X / curSpeed) - 1f;
			float targetDecelTime =  Math.Abs(target.LinearVelocity.X) / target.Acceleration; 
			
			if (decelTime >= traverseTime + targetDecelTime) {
				if (curSpeed > newSpeed) {
					linearDrive.X = LinearVelocity.Normalized().X - (LinearVelocity.Normalized().X * Acceleration);
				} else {
					linearDrive.X = -LinearVelocity.X;
				} 
			}
			
			newSpeed = Math.Abs(linearDrive.Y);
			curSpeed = Math.Abs(LinearVelocity.Y);
			
			decelTime = curSpeed / Acceleration;
			traverseTime = (dist.Y / curSpeed) - 1f;
			targetDecelTime =  Math.Abs(target.LinearVelocity.Y) / target.Acceleration; 
			
			if (decelTime >= traverseTime + targetDecelTime) {
				if (curSpeed > newSpeed) {
					linearDrive.Y = LinearVelocity.Normalized().Y - (LinearVelocity.Normalized().Y * Acceleration);
				} else {
					linearDrive.Y = -LinearVelocity.Y;
				} 
			}
			
			linearDrive = linearDrive.Normalized() * Acceleration;
			driveLabel.Text = Math.Round(linearDrive.Length(), 2).ToString();
			ApplyCentralForce(linearDrive);
		}
		if (skip == null) {
			updateMeters(state);
		}
	}
	
	private Vector2 prevVel = Vector2.Zero;
	
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
		
		if (linearDrive.Length() != 0f) {
			driveMeter.Frame = 0;
			driveMeter.Rotation = linearDrive.Angle() + 1.5708f;
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
			desiredHeading = star.giveHeading(Name, GlobalPosition, desiredHeading);
			
			Vector2 diff = Game.Instance.XY(desiredHeading) - heading;
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
				if (apply) {
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
			return Vector2.Zero;
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
