using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ship : RigidBody2D
{
	[Export] public Camera2D camera;
	[Export] public CrewRoster defaultRoster;
	[Export] private MapCtrl mapCtrl;
	[Export] private StateMachine brain;

	public float TurnSpeed = 10000f;
	[Export] public float Acceleration = 1000.0f;
	[Export] public float cons = 2000f;
	public float v = 0f;
	private float angularDrive = 0f;
	public Vector2 linearDrive = Vector2.Zero;
	public Vector2 calculatedDrive = Vector2.Zero;
	public Vector2 heading = Vector2.Zero;

	[Export] public CelestialBody star;
	[Export] public Ship target;
	[Export] public Skip skip;
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
		brain.init();
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

	public float delta = 0f;
	public override void _PhysicsProcess(double delta) {
		this.delta = (float) delta;
		base._PhysicsProcess(delta);

		updateHeading();
		//ApplyCentralForce(new Vector2(200f, 0f));
		brain.process(delta);
		
		if (!timer) {
		//	ApplyCentralForce(new Vector2(100f, 0f));
		}


		if (skip != null) {
			//GD.Print(Mathf.RadToDeg(linearDrive.Angle() ) + " " + Mathf.RadToDeg(GlobalRotation));
			//GD.Print("!! " + Mathf.RadToDeg(linearDrive.Angle()));// - GlobalRotation ));-//linearDrive.Angle()));
			if (f == 10) {
				displayVel = LinearVelocity.Length();
				displayDrive = v;
				displayHead = heading;
				f = 0;
			} else {
				f++;
			}
			velLabel.Text = Math.Round(displayVel, 2).ToString();
			driveLabel.Text = Math.Round(displayDrive, 2).ToString();
			headLabel.Text =  Math.Round(displayHead.Length(), 2).ToString();
		}
	}
	int f  = 0;
	float displayVel = 0f;
	float displayDrive = 0f;//Vector2.Zero;
	Vector2 displayHead = Vector2.Zero;

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

		if (skip != null) {
			updateMeters(state);
		}
	}

	public void move(Vector2 targ) {
		move(targ, 0f, Vector2.Zero, 0f);
	}

	public void move(Vector2 targ, float desiredDist) {
		move(targ, desiredDist, Vector2.Zero, 0f);
	}

	Vector2 prevPos = Vector2.Zero;
	float angle;
	int i = 0;

	public void move(Vector2 targ, float desiredDist, Vector2 targetVelocity, float targetAcceleration) {
		
		float interceptTime = 0f;
		if (LinearVelocity.Length() != 0f) {
			interceptTime = (targ - GlobalPosition).Length()/(LinearVelocity).Length();
		}
		if (interceptTime < 0f) {
			interceptTime = 0f;
		}
		
		// + (targetVelocity * interceptTime)
		Vector2 drive = ((targ + (targetVelocity * interceptTime)) - GlobalPosition);
		
		Vector2 vel = LinearVelocity;
		if ((LinearVelocity + drive).Length() > LinearVelocity.Length()) {
			vel = drive.Normalized();  //LinearVelocity.Normalized() +
		} else {
			//vel = Vector2.Zero;
		}
		
		
		GD.Print(interceptTime);

		Vector2 dist = targ - GlobalPosition;
		Vector2 interceptDist = targ + (targetVelocity * interceptTime) - GlobalPosition;
		dist.X = Math.Abs(dist.X);
		dist.Y = Math.Abs(dist.Y);
		interceptDist.X = Math.Abs(interceptDist.X);
		interceptDist.Y = Math.Abs(interceptDist.Y);
		Vector2 ratio = dist.Normalized();
		dist -= new Vector2(desiredDist * ratio.X, desiredDist * ratio.Y);
		
		Vector2 interceptRatio = interceptDist.Normalized();
		interceptDist -= new Vector2(desiredDist * interceptRatio.X, desiredDist * interceptRatio.Y);

		
		float newSpeed = Math.Abs(drive.X);
		float curSpeed = Math.Abs(LinearVelocity.X);


		float accel = Acceleration;
		if (accel > targetAcceleration) {
			accel = targetAcceleration;
		}
		float decelTime = curSpeed / accel;
		float traverseTime = (dist.X / curSpeed) - 1f;
		interceptTime = (interceptDist.X / curSpeed) - 1f;

		float targetDecelTime = 0f;
		if (targetAcceleration != 0) {
			targetDecelTime = Math.Abs(targetVelocity.X) / targetAcceleration;
		}

		if (decelTime >= traverseTime + targetDecelTime
			|| decelTime >= interceptTime + targetDecelTime) {
			if (curSpeed > newSpeed) {
				drive.X = LinearVelocity.Normalized().X - (LinearVelocity.Normalized().X * Acceleration);
			} else {
				drive.X = -LinearVelocity.X;
			}
		} 

		newSpeed = Math.Abs(drive.Y);
		curSpeed = Math.Abs(LinearVelocity.Y);
		
		decelTime = curSpeed / accel;
		traverseTime = (dist.Y / curSpeed) - 1f;
		interceptTime = (interceptDist.Y / curSpeed) - 1f;

		if (targetAcceleration != 0) {
			targetDecelTime = Math.Abs(targetVelocity.Y) / targetAcceleration;
		}

		if (decelTime >= traverseTime + targetDecelTime
			|| decelTime >= interceptTime + targetDecelTime) {
			if (curSpeed > newSpeed) {
				drive.Y = LinearVelocity.Normalized().Y - (LinearVelocity.Normalized().Y * Acceleration);
			} else {
				drive.Y = -LinearVelocity.Y;
			}
		}
		
		float offset = 1.5708f;
		
		angle = ((vel.Angle() - GlobalRotation)) - offset;
		if (vel.Angle() < 0f && GlobalRotation > 0f) { // Math.Abs(angle) > 3.1416) {
			angle = ((-4.71239f - angle) * -1f) + offset;
		}
		
		float turnTime = Math.Abs(angle / AngularVelocity);
		
		float dir = 0f;
		if (AngularVelocity > 0f) {
			dir = -1f;
		} else if (AngularVelocity < 0f) {
			dir = 1f;
		}
		float stopRotTime = Math.Abs(AngularVelocity/Math.Abs((dir * TurnSpeed)/Inertia));
		
		if (stopRotTime >= turnTime) {
			ApplyTorque(dir * TurnSpeed);
		} else {
			dir = 0f;
			if (angle > 0f) {
				dir = 1f;
			} else if (angle < 0f) {
				dir = -1f;
			}
			ApplyTorque(dir * TurnSpeed);
		}
		
		driveLabel.Text = Math.Round(drive.Length(), 2).ToString();
		
		Vector4 result = limitVelocity(drive.Normalized() * Acceleration);
		drive = new Vector2(result.X, result.Y);
		if (result.Z == 0f) {
			ApplyCentralForce(drive);
		} else if (result.Z == 1f) {
			ApplyCentralImpulse(drive);
		}
		
		if (result.W == 0f) {
			linearDrive = drive;
		}
	}

	Vector2 orbitOffset = Vector2.Zero;
	public Vector4 limitVelocity(Vector2 force) {
		float impulse = 0f;
		float limited = 0f;
		// orbiting planet
		if (desiredHeading.Z != 0f) { 
			Vector2 dh = new Vector2(desiredHeading.X,desiredHeading.Y);
			if (v <= cons + dh.Length()) { // currently under speed limit
				v = ((LinearVelocity.Normalized() * cons) + dh).Length();
				orbitOffset = dh;
			} else if ((Math.Abs(force.Angle() - LinearVelocity.Angle()) > 1.5708f &&
					 	LinearVelocity.Length() < v)) { 		// accel past speed limit (from heading?)
						/*if (LinearVelocity.Length() > (LinearVelocity - orbitOffset).Length()) {
							v = (LinearVelocity - orbitOffset).Length();
						} else {
							v = (LinearVelocity + orbitOffset).Length();
						}*/
					//	GD.Print("!!! " + v + " " /*+ heading + " " + heading.Length() + " "*/ + orbitOffset.Length());
					//	Vector2 newOrbitOffset = ( heading); //LinearVelocity.Normalized() +
						v = ((LinearVelocity - orbitOffset) + dh).Length(); // - orbitOffset) + heading
					//	v = (LinearVelocity).Length();
						orbitOffset = dh;

						if (v < LinearVelocity.Length()) {

						//	ApplyCentralForce(-LinearVelocity.Normalized() * Math.Abs(v - LinearVelocity.Length()));
						}//orbitOffset
						/*if (LinearVelocity.Length() > (LinearVelocity - orbitOffset).Length()) {
							v += orbitOffset.Length();
						} else {
							v -= orbitOffset.Length();
						}*/
			}
		} 
		// free space
		else { 
			orbitOffset = Vector2.Zero;
		if (v <= cons && LinearVelocity.Length() <= cons) {	// currently under speed limit
			v = ((LinearVelocity.Normalized() * cons) + ((heading * delta) / Mass)).Length();
		} else if ((Math.Abs(force.Angle() - LinearVelocity.Angle()) > 1.5708f && LinearVelocity.Length() < v)  // target vel > 90 deg of velocity (decrease limit)						 	// heading decreases velocity
			 || LinearVelocity.Length() > v) {																	// or, speed exceeds limit (increase limit)
				if (LinearVelocity.Length() - cons < Acceleration * delta) {
					v = cons;
				} else {
					v = (LinearVelocity).Length();
				}
			}
		}

		
		// apply damping	
		if ((LinearVelocity + ((force * delta) / Mass)).Length() >= v 								// force will exceed speed limit
			&& (LinearVelocity + ((force * delta) / Mass)).Length() > LinearVelocity.Length()) {	// force will accelerate ship
			
			//float angularDiff = Mathf.RadToDeg(LinearVelocity.Normalized().DistanceTo(force.Normalized()));
			Vector2 proposedForce = force;
			float xOff = Math.Abs(LinearVelocity.Normalized().X) - Math.Abs(force.Normalized().X);
			float yOff = Math.Abs(LinearVelocity.Normalized().Y) - Math.Abs(force.Normalized().Y);

			Vector2 absForce = new Vector2(Math.Abs(force.X), Math.Abs(force.Y));
			Vector2 absVel = new Vector2(Math.Abs(LinearVelocity.X), Math.Abs(LinearVelocity.Y));
			float xDest = LinearVelocity.X + (force.X / Mass);// LinearVelocity.X + ((force.X * delta) / Mass);
			float yDest = LinearVelocity.Y + (force.Y / Mass);

			float curSpeed = v;

			// cleaning up values for xDest + yDest (decreases likelihood generated force will exceed speed limit)
			int dir = 1;
			if (Math.Abs(xDest) > curSpeed || curSpeed - Math.Abs(xDest) < 1f) {
				if (force.X < 0f) {
					dir = -1;
				}
				xDest = curSpeed * dir;
			} else if (Math.Abs(xDest) < 1f) {
				xDest = 0f;
			}

			dir = 1;
			if (Math.Abs(yDest) > curSpeed || curSpeed - Math.Abs(yDest) < 1f) {
				if (force.Y < 0f) {
					dir = -1;
				}
				yDest = curSpeed * dir;
			} else if (Math.Abs(yDest) < 1f) {
				yDest = 0f;
			}


			if (xOff <= 0f && yOff >= 0f) {
				limited = 1f;
				force.X = (xDest - LinearVelocity.X) * Mass;
				int dirY = 1;
				if (force.Y < 0f) {
					dirY = -1;
				}
				yDest = dirY * (float) Math.Sqrt(Math.Pow(curSpeed, 2) - Math.Pow(xDest, 2));	// necessary dest of y to maintain speed (pythagorean theorum)
				force.Y = (yDest - LinearVelocity.Y) * Mass;
			} else if (xOff >= 0f && yOff <= 0f) {
				limited = 1f;
				force.Y = (yDest - LinearVelocity.Y) * Mass;
				int dirX = 1;
				if (force.X < 0f) {
					dirX = -1;
				}
				xDest = dirX * (float) Math.Sqrt(Math.Pow(curSpeed, 2) - Math.Pow(yDest, 2));	// necessary dest of x to maintain speed (pythagorean theorum)
				force.X = (xDest - LinearVelocity.X) * Mass;
			}
			
			
			if (((force * delta) + (heading * delta) + LinearVelocity).Length() != curSpeed) {
				Vector2 dest = ((force * delta) / Mass) + LinearVelocity;
				dest = dest.Normalized() * curSpeed;
				force = (dest - LinearVelocity) * Mass; // decreases as u approach speed

				if (force.Length() <= Acceleration * delta * 10f) {
					impulse = 1f;
				}
			}
			float headDiff = (LinearVelocity + (heading * delta)).Length() - v;
			if ((LinearVelocity + heading).Length() > (LinearVelocity + heading + force).Length()
				&& (proposedForce + heading).Length() > proposedForce.Length()) {
				if (headDiff > 0) {
					v += headDiff;
					GD.Print("!!!!!");
				}
			}
		}
		return new Vector4(force.X, force.Y, impulse, limited);
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

	Vector3 desiredHeading = Vector3.Zero;

	public Vector2 updateHeading() {
		if (star != null && this.timer) {
			desiredHeading = Vector3.Zero;
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
				//ensures we dont push past planets velocity
				if (apply) {
					if (((heading.X < 0) && (x + vel.X < heading.X)) ||
						((heading.X > 0) && (x + vel.X > heading.X))) {
							x = heading.X - vel.X;
					}
					//GD.Print("!! " + (x * 100f));
					ApplyCentralForce(new Vector2(x * 100f, 0f));// * desiredHeading.Z);
					heading.X = x * 100f;
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
				//ensures we dont push past planets velocity
				if (apply) {
					if (((heading.Y < 0) && (y + vel.Y < heading.Y)) ||
						((heading.Y > 0) && (y + vel.Y > heading.Y))) {
						y =  heading.Y - vel.Y;
					}
					//GD.Print("?? " + (y * 100f));
					ApplyCentralForce(new Vector2(0f, y * 100f));// * desiredHeading.Z);
					heading.Y = 100f * y;
				}
			} else {
				ApplyCentralForce(heading);
			}
			appliedHeading += heading * delta;
		//	GD.Print(appliedHeading.Length());
		//	GD.Print(calculatedDrive.Length());
		//	GD.Print((calculatedDrive + appliedHeading).Length());
			return diff;
		} else {
			return Vector2.Zero;
		}

	}
	Vector2 appliedHeading = Vector2.Zero;

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
 
