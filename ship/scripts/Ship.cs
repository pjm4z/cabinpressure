using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ship : CharacterBody2D
{
	[Signal]
	public delegate void JobCompletedEventHandler();
	
	public const float MaxSpeed = 1000.0f;  // Max forward speed
	public const float MaxReverseSpeed = 20.0f;  // Max reverse speed
	public const float TurnAcceleration = 0.01f;  // Rotation speed (turning speed)
	public const float TurnSpeed = 0.01f;
	public const float Acceleration = 100.0f;  // Acceleration rate
	public const float ReverseAcceleration = 10.0f;  // Reverse cceleration rate
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
	//private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
	//private WireCtrl wireCtrl;
	[Export] public Camera2D camera;
	[Export] private TileMapLayer hullMap;
	[Export] private PowerGrid powerGrid;
	[Export] public CrewRoster defaultRoster;
	public float rotationSpeed;
	
	public override void _Ready() {
		ZIndex = 1;
		surface = (Node2D)GetParent();
		
		underwater = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		surfaceMap = GetNode<SurfaceMap>("/root/basescene/surface/surfaceviewport/surfacemap");

		//camera = (Camera2D) GetNode("playercamera");
		//defaultRoster = (CrewRoster) GetNode("crewroster");
		//hullMap = (TileMapLayer) GetNode("hullmap");
		//skip = (Skip) GetNode("/root/basescene/surface/shipscene/skip");

		InitialPosition = Position;
		rotationSpeed = 0;
		initBeds();
		//initWeaponSlots();
	}
	
	public PowerGrid getPowerGrid() {
		return powerGrid;
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
	
	public override void _PhysicsProcess(double delta) {
		velocity = Velocity;
		float speed = Velocity.Length();
		
		if (!active) {
			//RemoveChild(camera);
			//skip.AddChild(camera);
		}
		else { // Handle rotation (turning the ship)
			if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {

				rotationSpeed -= TurnAcceleration * (float)(delta);
				if (Math.Abs(rotationSpeed) > TurnSpeed) {
					rotationSpeed = -1 * TurnSpeed;
				}
			}
			if (Input.IsActionPressed("ui_right") || Input.IsActionPressed("d")) {
				rotationSpeed += TurnAcceleration * (float)delta;
				if (rotationSpeed > TurnSpeed) {
					rotationSpeed = TurnSpeed;
				}
			}
			Rotation += rotationSpeed;
			// Handle forward and reverse movement with acceleration
			if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
				// Accelerate forward based on the ship's rotation
				if (MaxSpeed > Velocity.Length()) {
					speed = MaxSpeed;
				}
				velocity = velocity.MoveToward(
					new Vector2(0, speed).Rotated(Rotation), 
					Acceleration * (float)delta);
			}
			else if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
				// Accelerate in reverse based on the ship's rotation
				if (MaxReverseSpeed > Velocity.Length()) {
					speed = MaxReverseSpeed;
				}
				velocity = velocity.MoveToward(
					new Vector2(0, speed).Rotated(Rotation + (float)Math.PI), 
					ReverseAcceleration * (float)delta);
			}
			
			Velocity = velocity;
			MoveAndSlide();
		}
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
		
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
}
