using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Boat : CharacterBody2D
{
	[Signal]
	public delegate void JobCompletedEventHandler();
	
	public const float MaxSpeed = 1000.0f;  // Max forward speed
	public const float MaxReverseSpeed = 20.0f;  // Max reverse speed
	public const float TurnSpeed = 0.33f;  // Rotation speed (turning speed)
	public const float Acceleration = 100.0f;  // Acceleration rate
	public const float Deceleration = 0.995f;  // Deceleration rate
	private Vector2 velocity = Vector2.Zero;
	 
	[Export] private GpuParticles2D wake;
	private Node2D surface;
	private SubViewport underwater;
	private SurfaceMap surfaceMap;
	public Vector2 InitialPosition;
	//public Skip skip;
	public Camera2D camera;
	public bool active = true;
	private CrewRoster crewRoster;
	public Queue<Furniture> availableBeds = new Queue<Furniture>();  // TODO --> change to bed when i have bed class
	private List<Furniture> takenBeds = new List<Furniture>();
	private PostCtrl postCtrl;
	private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
	
	List<Post> postList = new List<Post>();
	[Export] Post leftTorpedo;
	[Export] Post leftTorpedo2;
	[Export] Post rightTorpedo;
	
	
	public override void _Ready() {
		ZIndex = 1;
		surface = (Node2D)GetParent();
		
		underwater = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		surfaceMap = GetNode<SurfaceMap>("/root/basescene/surface/surfaceviewport/surfacemap");
		postCtrl = (PostCtrl) GetNode("postctrl");
		leftTorpedo = (Post) GetNode("postctrl/post");
		rightTorpedo = (Post) GetNode("postctrl/post2");
		camera = (Camera2D) GetNode("playercamera");
		crewRoster = (CrewRoster) GetNode("crewroster");
		//skip = (Skip) GetNode("/root/basescene/surface/boatscene/skip");
		wake.Lifetime = 0.5f;
		ParticleProcessMaterial wakeMaterial = (ParticleProcessMaterial) wake.ProcessMaterial;
		wakeMaterial.LifetimeRandomness = 1.0f;
		wake.Amount = 50;
		wake.Emitting = true;
		InitialPosition = Position;
		
		//initPosts();
		//initCrew();
		initBeds();
		initWeaponSlots();
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
	
	public void initWeaponSlots() {
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
	
	public void giveBed(Crew crew) {
		if (availableBeds.Count > 0) {
			GD.Print("givinbed");
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
		//_Track_Boat();
		// Get the current velocity of the boat
		/*if (leftTorpedo.assignedCrew == null) {
			GD.Print("LT: " + leftTorpedo.queuedOrders + " " + leftTorpedo.assignedCrew);
		} else {
			GD.Print("LT: " + leftTorpedo.queuedOrders + " " + leftTorpedo.assignedCrew.firstName + " " + leftTorpedo.assignedCrew.sleep);
		}
		
		if (rightTorpedo.assignedCrew == null) {
			GD.Print("RT: " + rightTorpedo.queuedOrders + " " + rightTorpedo.assignedCrew);
		} else {
			GD.Print("RT: " + rightTorpedo.queuedOrders + " " + rightTorpedo.assignedCrew.firstName+ " " + rightTorpedo.assignedCrew.sleep);
		}
		GD.Print();*/
		
		velocity = Velocity;
		
		if (!active) {
			//RemoveChild(camera);
			//skip.AddChild(camera);
		}
		else { // Handle rotation (turning the boat)
			if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
				if (Velocity.Length() > 0) {
					Rotation -= (Velocity.Length()/100 + TurnSpeed) * (float)delta;  // Rotate left
				}
			
			}
			if (Input.IsActionPressed("ui_right") || Input.IsActionPressed("d")) {
				if (Velocity.Length() > 0) {
					Rotation += (Velocity.Length()/100 +  TurnSpeed) * (float)delta;  // Rotate right
				}
			}

			// Handle forward and reverse movement with acceleration
			if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
				// Accelerate forward based on the boat's rotation
				velocity = velocity.MoveToward(new Vector2(0, MaxSpeed).Rotated(Rotation), Acceleration * (float)delta);
			}
			else if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
				// Accelerate in reverse based on the boat's rotation
				velocity = velocity.MoveToward(new Vector2(0, -MaxReverseSpeed).Rotated(Rotation), (Acceleration) * (float)delta);
			}
			else {
				velocity = velocity.MoveToward(new Vector2(0, Velocity.Length() * Deceleration).Rotated(Rotation), Acceleration * (float)delta);
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
