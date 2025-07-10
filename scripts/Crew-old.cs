/*using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class CrewX : CharacterBody2D
{
	public string firstName;
	public string lastName;
	
	[Export] public NavigationAgent2D nav;
	[Export] public NavigationRegion2D navRegion;
	public Queue<Post> jobQueue = new Queue<Post>();
	public Post post;
	private Post lastPost;
	public Weapon wpn;
	private bool navReady = false;
	private Boat boat;
	private CrewRoster roster;
	public Furniture bed; // --> TODO change to bed when i have bed class
	private CrewProgress crewProgress;
	private Sprite2D sprite;
	
	private Dictionary<string, RayCast2D> raycasts = new Dictionary<string, RayCast2D>();
	
	// multipliers/vars
	public double readiness = 0;
	
	public static float MAX_HEALTH = 10f;
	public static float MAX_HUNGER = 10f;
	public static float MAX_SLEEP = 10f;
	public static float MAX_SPEED = 100f;
	
	public float health = MAX_HEALTH;
	public float hunger = MAX_HUNGER;
	public float sleep = MAX_SLEEP;
	public float speed = MAX_SPEED;
	
	public float hunger_multiplier = 0.2f;
	public float sleep_multiplier = 0.033f;
	
	private Label nameplate;
	private Label label;
	private VBoxContainer crewPanel;
	
	public override void _Ready() {
		nav = (NavigationAgent2D) GetNode("nav");
		crewProgress = (CrewProgress) GetNode("progress");
		roster = (CrewRoster) GetParent();
		boat = (Boat) roster.GetParent();
		nameplate = (Label) GetNode("nameplate");
		ProcessMode = Node.ProcessModeEnum.Always;
		sprite = (Sprite2D) GetNode("sprite");
		InitializeRaycasts();
		
		crewPanel = (VBoxContainer) GetNode("/root/basescene/HUD/crewcontainer/crewpanel");
	}
	
	
	private void InitializeRaycasts() {
		
		var raycastArray = this.sprite.GetChildren()
			.Where(child => child is RayCast2D) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<RayCast2D>(); // TODO --> change to bed when I have bed class                 

		foreach(var ray in raycastArray) {
			raycasts.Add(ray.GetName(), ray);
		}
	}
	
	public bool seekingBed = false;
	public bool seekingJob = false;
	public bool seekingFood = false;
	public bool working = false;
	public bool sleeping = false;
	
	public Item inventory;
	public Item soughtItem;
	Color red = new Color(1.0f,0.0f,0.0f,1.0f);
	Color white = new Color(1.0f,1.0f,1.0f,1.0f);
	
	private void updateLabels() {
		if (working == true || seekingJob == true) {
			nameplate.Set("theme_override_colors/font_color",red);
			label.Set("theme_override_colors/font_color",red);
		} else {
			nameplate.Set("theme_override_colors/font_color",white);
			label.Set("theme_override_colors/font_color",white);
		}
	}
	
	public override void _PhysicsProcess(double delta) {
		updateLabels();



		if (health <= 0) {
			QueueFree();
		}
		
		checkPriorities();
		reportReadiness();
		
		if (!GetTree().Paused) {
			Vector2 dir;
			
			if (navReady == true) {
				if (sleeping) { 
					dir = GetGlobalPosition();
				} else if (seekingBed) {
					dir = seekBed();
				} else if (seekingFood) {
					dir = seekFood();
				} else if (working) {
					dir = GetGlobalPosition();
				} else if (seekingJob) {
					dir = seekJob();
				} else {
					nav.TargetPosition = boat.GetGlobalPosition();
					dir = nav.GetNextPathPosition();
				}
			} else { // rest
				dir = boat.GetGlobalPosition();
			}
			
			float rotation = (float) boat.GlobalRotation;
			sprite.Rotation = GetAngleTo(dir) + 1.5708f;
			Vector2 intendedVel = adjustRotation(dir, rotation) * speed;
			nav.SetVelocity(intendedVel);
		}
	}
	
	public void reportReadiness() {
		if (!seekingJob && !working && !seekingFood) {
			readiness = hunger + sleep;
			if (roster.maxReady != null) {
				if (readiness > roster.maxReady.readiness) {
					roster.maxReady = this;
				}
			} else {
				roster.maxReady = this;
			}
		} else {
			readiness = 0;
			if (roster.maxReady == this) {
				roster.maxReady = null;
			}
		}
	}
	
	public void setName(string fn, string ln) {
		nameplate.Text = ln;
		label = new Label();
		label.Text = fn + " " + ln;
		crewPanel.AddChild(label);
	}
	
	public void checkPriorities() {
		checkJobStatus(); // lowest priority (overridden by all others)
		checkHunger();
		checkSleep();	// top priority (overrides others)
	}
	
	public void checkSleep() {
		if (sleep <= 2 && sleeping == false) {
			seekingBed = true;
			working = false;
			seekingJob = false;
			seekingFood = false;
		} else if (sleep >= MAX_SLEEP) {
			sleeping = false;
			seekingBed = false;
			sleep = MAX_SLEEP;
		}
	}
	
	public void checkHunger() {
		if (hunger <= 5) {
			if (inventory != null) {
				inventory.use(this);
				seekingFood = false;
			} else {
				working = false;
				seekingFood = true;
			}
		} else {
			seekingFood = false;
		}
	}
	
	public void checkJobStatus() {
		if (working == false && post != null  && wpn != null) { 
			if (post.assignedCrew == this && wpn.assignedCrew == this) {
				if (wpn.active == true || wpn.queuedOrders > 0) {
					seekingJob = true;
				} else {
					//GD.Print("Assigned post with no queued orders and not active? " + lastName);
					//GD.Print(working, seekingJob, (post==null),(wpn==null),wpn.posted,wpn.queuedOrders,wpn.active);
					detachOrders();
				}
				
			} else {
				if (post.assignedCrew != null) {
					//GD.Print("ASSIGNED SOMEONE ELSE'S JOB??" + lastName+post.assignedCrew.lastName+wpn.assignedCrew.lastName);
					//GD.Print(post.assignedCrew.lastName);
					//GD.Print(post.assignedCrew.post);
					//GD.Print(this.post);
					detachOrders();
				} 
			}
		}
	}
	
	public async void doJob() {
		while (working == true && (wpn.queuedOrders > 0 || wpn.active == true)) {
			if (wpn.queuedOrders > 0) {
				await post.doJob(wpn);
				crewProgress.deltaElapsed = 0;
			} else {
				await Task.Delay(1);
			}
		}
		if ((wpn != null) && (wpn.queuedOrders > 0 || wpn.active == true)) {
			//GD.Print("KB " + lastName);
			kickbackOrders();
		} else {
			//GD.Print("DT " + lastName);
			detachOrders();
		}
	}
	
	
	public Vector2 seekFood() {		// handle case of no food
		if (this.soughtItem == null) {
			var food = boat.GetChildren()
				.Where(child => child is Item) // We only want nodes that we know are Post nodes
				.Select(child => child)          
				.Cast<Item>(); 
				
			foreach (Item i in food) {
				if (i.crew == null) {
					soughtItem = i;
					soughtItem.crew = this;
					break;
				}
			}
		}
		Vector2 dir = new Vector2(boat.GlobalPosition.X, boat.GlobalPosition.Y + 25f);
		
		if (soughtItem != null) {
			nav.TargetPosition = soughtItem.GlobalPosition;				// set nav path to job pos
			dir = nav.GetNextPathPosition();
					
			if (Math.Abs(GlobalPosition.X - soughtItem.GlobalPosition.X) < nav.TargetDesiredDistance &&
				Math.Abs(GlobalPosition.Y - soughtItem.GlobalPosition.Y) < nav.TargetDesiredDistance) {			// if at job location, dequeue job
				soughtItem.pickUp(this);
			} else {
			}
		} 
		return dir;
	}
	
	public Vector2 seekJob() {
		// if at job location, dequeue job
		if (Math.Abs(GlobalPosition.X - post.GlobalPosition.X) < nav.TargetDesiredDistance &&
			Math.Abs(GlobalPosition.Y - post.GlobalPosition.Y) < nav.TargetDesiredDistance) {
			if (working == false) {
				seekingJob = false;
				working = true;
				doJob();
			}
		} 
		Vector2 dir;
		nav.TargetPosition = post.GlobalPosition;
		dir = nav.GetNextPathPosition();
		return dir;
	}
	
	public Vector2 seekBed() {
		if (bed == null) {
			boat.giveBed(this);
		} if (bed == null) {
			sleeping = true;
			seekingBed = false;
			return GetGlobalPosition();
		}
		Vector2 dir;
		nav.TargetPosition = bed.GlobalPosition;				// set nav path to job pos
		dir = nav.GetNextPathPosition();
				
		if (Math.Abs(GlobalPosition.X - bed.GlobalPosition.X) < nav.TargetDesiredDistance &&
			Math.Abs(GlobalPosition.Y - bed.GlobalPosition.Y) < nav.TargetDesiredDistance) {			// if at job location, dequeue job
			sleeping = true;
			seekingBed = false;
		} 
		return dir;
	}
	
	public Vector2 adjustRotation(Vector2 globalDir, float globalRotation) {
		float rotation = globalRotation/3.14159862f;			// normalize rotation btwn -1 and 1
		Vector2 dir = ToLocal(globalDir).Normalized();			// make global dir local, normalize btwn 0 and 1
		float x = 0, y = 0;
		
		// Adjust dir to rotation
		if (rotation < -0.5f) {	// topright 
			float omr = (0 - rotation);				// rotation dist from top axis
			float w1 = (omr - 0.5f) * 2;			// normalized 0-1
			
			float hpr = (0.5f + rotation);			// rotation dist from right axis
			float w2 = 1+(hpr*2);					// normalized 0-1
			
			x = (dir.X * w1) + (-dir.Y * w2);		// get avg btwn x1 and x2 based on dist from axes
			y = (dir.Y * w1) + (dir.X * w2);		// get avg btwn y1 and y2 based on dist from axes
		} else if (rotation > -0.5f && rotation < 0) { // bottomright
			float n1mr = (-1 - rotation);
			float w1 = (n1mr + 0.5f) * -2;
			float hmr = (0.5f - rotation);
			float w2 = (hmr*2) - 1;
			x = (-dir.Y * w2) + (-dir.X * w1);
			y = (dir.X * w2) + (-dir.Y * w1);
		} else if (rotation < 0.5f && rotation > 0) { // bottomleft
			float n1pr = (-1 + rotation);
			float w1 = (n1pr + 0.5f) * -2;
			float hpr = (0.5f + rotation);
			float w2 = (hpr*2) - 1;
			x = (dir.Y * w2) + (-dir.X * w1);
			y = (-dir.X  * w2) + (-dir.Y * w1);
		} else if (rotation > 0.5f) { // topleft
			float opr = (0 + rotation);
			float w1 = (opr - 0.5f) * 2;
			float hmr = (0.5f - rotation);
			float w2 = 1+(hmr*2);
			x = (dir.X * w1) + (dir.Y * w2);
			y = (dir.Y * w1) + (-dir.X * w2);
		}
		dir = new Vector2(x, y);
		return dir;
	}
	
	public void receiveOrder(Weapon wpn, Post post) {
		//GD.Print("aye aye capn");
		detachOrders();
		this.wpn = wpn;
		this.wpn.posted = false;
		this.wpn.assignedCrew = this;
		if (this.lastPost == null) {
			this.post = post;
		} else {
			if (this.lastPost.assignedCrew == null || this.lastPost.assignedCrew == this) { 
				if (GlobalPosition.DistanceTo(this.lastPost.GlobalPosition) >= GlobalPosition.DistanceTo(post.GlobalPosition)) {
					this.post = post;
				} else {
					this.post = lastPost;
				}
			} else {
				this.post = post;
			}
		} 
		this.post.assignedCrew = this; 
	}
	
	public void kickbackOrders() {
		if (this.post != null) {
			this.post.assignedCrew = null;
			//GD.Print(this.post.assignedCrew);
			this.lastPost = post;
			this.post = null;
		}
		if (this.wpn != null) {
			this.wpn.posted = true;
			roster.jobBoard.AddLast(wpn);
			this.wpn.assignedCrew = null;
			this.wpn = null;
		}
		working = false;
		seekingJob = false;
	}
	
	public void detachOrders() {
		//GD.Print("DT " + lastName);
		if (this.post != null) {
			this.post.assignedCrew = null;
			//GD.Print(this.post.assignedCrew);
			this.lastPost = post;
			this.post = null;
		}
		if (this.wpn != null) {
			this.wpn.posted = false;
			this.wpn.assignedCrew = null;
			this.wpn = null;
		}
		working = false;
		seekingJob = false;
	}
	
	
	public void _on_crewtimer_timeout() {
		hunger -= hunger_multiplier;
		if (sleeping == false)
			sleep -= sleep_multiplier;
		else {
			sleep += sleep_multiplier * 3;
		}
		
		if (hunger < 0) {
			hunger = 0;
			health -= hunger_multiplier;
		}
		if (sleep < 0) {
			sleep = 0;
			health -= sleep_multiplier;
		}
	}
	
	public void _on_navtimer_timeout() {
		navReady = true;
	}
	
	public void _on_nav_velocity_computed(Vector2 velocity) {
		Velocity = velocity;
		MoveAndSlide();
	}
}
*/
