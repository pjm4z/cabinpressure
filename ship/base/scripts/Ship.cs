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
	 
//	[Export] private GpuParticles2D wake;
	private Node2D surface;
	private SubViewport underwater;
	private SurfaceMap surfaceMap;
	public Vector2 InitialPosition;
	//public Skip skip;
	
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
	
	public override void _Ready() {
		ZIndex = 1;
		//surface = (Node2D)GetParent();
		//underwater = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		//surfaceMap = GetNode<SurfaceMap>("/root/basescene/surface/surfaceviewport/surfacemap");
		InitialPosition = Position;
		rotationSpeed = 0;
		initBeds();
	//	if (shieldEnabled != null) {
		//shield = (Shield) GetNode("shield");
		//shield.ship = this;
		shield.init(this);
			//shield.active = shieldEnabled;
	//	}
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
	
	
	
	/*public void initWeaponSlots() {
		var wpnArray = GetChildren()
			.Where(child => child is WeaponSlot) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<WeaponSlot>(); // TODO --> change to bed when I have bed class                 

		int i = 1;
		foreach(var wpn in wpnArray) {
			//wpn.key = i.ToString();
			this.weaponSlots.Add(wpn);
			i += 1;
		}
	}
	*/
	public void giveBed(Crew crew) {
		if (availableBeds.Count > 0) {
			Furniture bed = availableBeds.Dequeue();
			crew.bed = bed;
			bed.crew = crew;
			takenBeds.Add(bed);
		} 
	}
	
	/*public void initPosts() {
		var postArray = GetChildren()
			.Where(child => child is Post) // We only want nodes that we know are Post nodes
			.Select(child => child)          
			.Cast<Post>();                 

		foreach(var post in postArray) {
			postList.Add(post);
		}
	}*/


	bool l1 = true;
	public override void _Input(InputEvent inputEvent) {
		if (!active) {
			//RemoveChild(camera);
			//skip.AddChild(camera);
		} else {
			
		}
	}
	
	public void _on_area_body_entered(Node body) {
		//GD.Print(body.Name);
	}
	
	public Vector2 inputMap = new Vector2(0,0);
	
	public override void _PhysicsProcess(double delta) {
		//GD.Print(Name + " " + GlobalPosition);
		velocity = inputMap;//new Vector2(0f,0f);//LinearVelocity; //
		rotationSpeed = AngularVelocity;
		float speed = LinearVelocity.Length();
		
		if (!active) {
			
			//RemoveChild(camera);
			//skip.AddChild(camera);
		}
		//else 
		if (skip != null) { // Handle rotation (turning the ship)
			if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
				if (Input.IsActionPressed("shift")) {
					if (MaxSpeed > LinearVelocity.Length()) {
						speed = MaxSpeed;
					}
					GD.Print(LinearVelocity.Normalized());
					velocity = velocity.MoveToward(
						//new Vector2(0, speed).Rotated(Rotation - (float)(Math.PI/2)), 
						new Vector2(velocity.X, speed).Rotated(GlobalRotation - (float)Math.PI/2.0f), 
						Acceleration * (float)delta);
				} else {
					//rotationSpeed -= TurnAcceleration * (float)(delta);
					//if (Math.Abs(rotationSpeed) > TurnSpeed) {
						rotationSpeed = -1 * TurnSpeed;
					//}
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
					//rotationSpeed += TurnAcceleration * (float)delta;
					//if (rotationSpeed > TurnSpeed) {
						rotationSpeed = TurnSpeed;
					//}
				}
			}
			ApplyTorque(rotationSpeed * 100); //
			//Rotation += rotationSpeed;
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
		} else {
			//LookAt(new Vector2(0,0));
		}
		
		
		/*if (GetSlideCollisionCount() > 0) {
			KinematicCollision2D collision = GetLastSlideCollision();
			Object obj = collision.GetCollider();
			if (obj is Crew) {
				GD.Print("ROADKILL");
				Crew crew = (Crew) obj;
				Vector2 pushDirection = collision.GetNormal(); 
				
				// Apply push to the other character
				crew.Velocity += (pushDirection * 50f);
				crew.MoveAndSlide();
			} else if (obj is Ship) {
				Ship ship = (Ship) obj;
				int m2 = ship.mass;
				
				float rat = mass / m2;
				Vector2 ratio = new Vector2(rat, rat);
				
				Vector2 pushDirection = collision.GetNormal(); 
				
				// Apply push to the other character
				ship.LinearVelocity += (pushDirection * 50f);
				
				//velocity = ship.Velocity;
				//ship.Velocity += collision.GetColliderVelocity();//velocity;
				//new Vector2(-50, 0);
				//Velocity = (Velocity * ratio) + (ship.Velocity * (new Vector2I(1,1)/ratio)); 
				//ship.Velocity = new Vector2(10, 10);
				//ship.MoveAndSlide();
				GD.Print("!!!!!! COLLIDED " + Name + " " +  collision.GetPosition() + " " + (collision.GetColliderVelocity()  + " " + velocity +  " " + ship.LinearVelocity) + " " + obj);
			}
		}*/
		ApplyCentralForce(velocity * new Vector2(100f,100f)); 
		this.delta = delta;
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
	
	private double delta;
	public override void _IntegrateForces(PhysicsDirectBodyState2D state) {
		if (Input.IsActionPressed("c")) {
			state.LinearVelocity -= new Vector2(state.LinearVelocity.X / 100f,state.LinearVelocity.Y / 100f);
		}
		if (skip == null) {
			state.AngularVelocity = GetAngleTo(new Vector2(0,0)) - 1.5708f;
		}
		
		/*velocity = state.LinearVelocity;
		float speed = state.LinearVelocity.Length();
		
		if (!active) {
			//RemoveChild(camera);
			//skip.AddChild(camera);
		}
		else { // Handle rotation (turning the ship)
			if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
				if (Input.IsActionPressed("shift")) {
					if (MaxSpeed > LinearVelocity.Length()) {
						speed = MaxSpeed;
					}
					GD.Print(LinearVelocity.Normalized());
					velocity = state.LinearVelocity.MoveToward(
						//new Vector2(0, speed).Rotated(Rotation - (float)(Math.PI/2)), 
						new Vector2(velocity.X, speed).Rotated(GlobalRotation - (float)Math.PI/2.0f), 
						Acceleration * (float)delta);
				} else {
					rotationSpeed = -0.0001f * TurnSpeed;
				}
			}
			if (Input.IsActionPressed("ui_right") || Input.IsActionPressed("d")) {
				if (Input.IsActionPressed("shift")) {
					if (MaxSpeed > LinearVelocity.Length()) {
						speed = MaxSpeed;
					}
					velocity = state.LinearVelocity.MoveToward(
						new Vector2(velocity.X, speed).Rotated(GlobalRotation + (float)Math.PI/2.0f), 
						Acceleration * (float)delta);
				} else {
					rotationSpeed = 0.0001f * TurnSpeed;
				}
			}
			state.AngularVelocity += rotationSpeed;
			//Rotation += rotationSpeed;
			// Handle forward and reverse movement with acceleration
			if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
				// Accelerate forward based on the ship's rotation
				if (MaxSpeed > state.LinearVelocity.Length()) {
					speed = MaxSpeed;
				}
				velocity = velocity.MoveToward(
					new Vector2(0, speed).Rotated(Rotation), 
					Acceleration * (float)delta);
			}
			else if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
				// Accelerate in reverse based on the ship's rotation
				if (MaxReverseSpeed > state.LinearVelocity.Length()) {
					speed = MaxReverseSpeed;
				}
				velocity = velocity.MoveToward(
					new Vector2(0, speed).Rotated(Rotation + (float)Math.PI), 
					ReverseAcceleration * (float)delta);
			}
			
			state.LinearVelocity = velocity;
		}*/
	}

	public void damageOuter(Vector2 gPos, double radius, int damage) {
		mapCtrl.damageOuter(gPos, radius, damage);
	}
	
	public void damageInner(Vector2 gPos, double radius, int damage) {
		mapCtrl.damageInner(gPos, radius, damage);
	}
	
	public int hp = 1000;
	public void changeHP(int hp) {
		this.hp += hp;
		if (this.hp < 0) {
			QueueFree();
		}
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
		/*
	private void _Track_Boat() {
		int x = (int) GlobalPosition.X / surfaceMap.PointDist;
		int y = (int) GlobalPosition.Y / surfaceMap.PointDist;
		int yOffset = (int) surfaceMap.PointsMap[0][0].targetHeight / surfaceMap.PointDist;
		int xOffset = (int) surfaceMap.PointsMap[0][0].Position.X / surfaceMap.PointDist;
		int xInd = x - xOffset;
		int yInd = y - yOffset;
		
		surfaceMap._Splash(Position.X, Position.Y, -0.02f);
		//GD.Print();
		//GD.Print((x - xOffset) + ", " + (y - yOffset));
		//GD.Print("v=" + surfaceMap.PointsMap[x - xOffset][y - yOffset].velocity + " f=" + surfaceMap.PointsMap[x - xOffset][y - yOffset].force);
		List<List<SurfacePt>> pm = surfaceMap.PointsMap;
		if (surfaceMap.PointsMap[x - xOffset][y - yOffset].velocity > 0 ||
			surfaceMap.PointsMap[x - xOffset][y - yOffset].force > 0) {
				//GD.Print();
				//GD.Print(pm[xInd - 1][yInd - 1].velocity + "  " + pm[xInd][yInd - 1].velocity + "  " + pm[xInd + 1][yInd - 1].velocity);
				//GD.Print(pm[xInd - 1][yInd].velocity + "  " + pm[xInd][yInd].velocity + "  " + pm[xInd + 1][yInd].velocity);
				//GD.Print(pm[xInd - 1][yInd + 1].velocity + "  " + pm[xInd][yInd + 1].velocity + "  " + pm[xInd + 1][yInd + 1].velocity);
		}
	}
	*/
}
