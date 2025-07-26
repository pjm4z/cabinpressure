using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Crew : CharacterBody2D
{
	// Nodes
	// parents
	public Ship ship;
	private CrewRoster roster;

	// siblings
	private Post lastPost;
	public Post post;
	public Weapon wpn;
	public JobTarget job;
	public Furniture bed;
	
	// children
	private StateMachine brain;
	private VBoxContainer crewPanel;
	private Label nameplate;
	private Label label;
	public NavigationAgent2D nav;
	private bool navReady = false;
	public Sprite2D sprite;
	public CrewProgress crewProgress;

	// class vars
	public string firstName;
	public string lastName;
	public int rank;
	public double readiness = 0;
	
	public static float MAX_HEALTH = 10f;
	public static float MAX_HUNGER = 10f;
	public static float MAX_SLEEP = 10f;
	public static float MIN_SLEEP = 5f;
	public static float MAX_SPEED = 100f;
	
	public float health = MAX_HEALTH;
	public float hunger = MAX_HUNGER;
	public float sleep = MAX_SLEEP;
	public float speed = MAX_SPEED;
	
	public float hunger_rate = 0.05f;
	public float sleep_rate = 0f;
	
	public override void _Ready() {
		brain = (StateMachine) GetNode("brain");
		sprite = (Sprite2D) GetNode("sprite");
		roster = (CrewRoster) GetParent();
		nav = (NavigationAgent2D) GetNode("nav");
		crewProgress = (CrewProgress) GetNode("progress");
		nameplate = (Label) GetNode("nameplate");
		
		ProcessMode = Node.ProcessModeEnum.Always;
		
		brain.parent = this;
		brain.init();
		
		crewPanel = (VBoxContainer) GetNode("/root/basescene/hudcanvas/HUD/crew/crewpanel");
	}
	
	public bool seekingBed = false;
	public bool seekingJob = false;
	public bool seekingFood = false;
	public bool working = false;
	public bool sleeping = false;
	
	public Item inventory;
	public Item soughtItem;
	public Item soughtFood;
	
	Color red = new Color(1.0f,0.0f,0.0f,1.0f);
	Color white = new Color(1.0f,1.0f,1.0f,1.0f);
	
	private void updateLabels() {
		label.Text = nameplate.Text;
		if (job != null) {
			label.Text += job.count();
		} 
		
		if (this.post != null) {
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
	
		if (!GetTree().Paused) {
			brain.process(delta);
		}
	}
	
	public void move(Vector2 target) {
		nav.TargetPosition = target;
		target = nav.GetNextPathPosition();
		float rotation = (float) ship.GlobalRotation;
		sprite.Rotation = GetAngleTo(target) + 1.5708f;
		target = adjustRotation(target, rotation) * speed;
		if (nav.AvoidanceEnabled == true) {
			nav.SetVelocity(target);
		} else {
			_on_nav_velocity_computed(target);
		}
	}
	
	public void reportReadiness() {
		readiness = hunger + sleep;
		if (roster.maxReady != null) {
			if (readiness > roster.maxReady.readiness) {
				roster.maxReady = this;
			}
		} else {
			roster.maxReady = this;
		}
	}
	
	public void setName(string fn, string ln) {
		nameplate.Text = ln;
		label = new Label();
		label.Text = fn + " " + ln;
		crewPanel.AddChild(label);
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
	
	public bool checkWork() {
		if (post != null && job != null) {
			if (post.isOccupied && (job.count() > 0 || job.getActive() == true)) {
				return true;
			}
		}
		return false;
	}
	
	public bool checkSleep() {
		if (sleeping) {
			if (sleep < MAX_SLEEP) {
				return true;
			} else {
				return false;
			}
		}
		return ((sleep <= 0) || (sleep < MIN_SLEEP && atLocation(bed)));
	}
	
	public bool checkSeekBed() {
		if (sleep <= MIN_SLEEP) {
			if (bed == null) {
				ship.giveBed(this);
			}
			return (bed != null);
		} 
		return false; 
	}
	
	public bool checkWakeUp() {
		if (sleep >= MAX_SLEEP) {
			sleep = MAX_SLEEP;
			return true;
		}
		return false;
	}
	
	public bool checkSeekFood() {
		if (hunger <= 5) {
			if (soughtFood == null) {
				soughtFood = findFood();
			}
			return (soughtFood != null);
		} 
		return false;
	}
	
	public bool checkSeekJob() {
		if (post != null && job != null) { 
			if (post.assignedCrew == this && job.assignedCrew == this) {
				if (job.getActive() == true || job.count() > 0) {
					return true;
				} else {
					GD.Print("Assigned post with no queued orders and not active? " + lastName);
				}
			} else {
				if (post.assignedCrew != null) {
					GD.Print("ASSIGNED SOMEONE ELSE'S JOB??" + lastName+post.assignedCrew.lastName+job.assignedCrew.lastName);
				} 
			}
		}
		detachOrders();
		return false;
	}
	
	public bool isNavReady() {
		return navReady;
	}
	
	private Item findFood() {
		var food = ship.GetChildren()
			.Where(child => child is Item) // We only want nodes that we know are Post nodes
			.Select(child => child)          
			.Cast<Item>(); 
				
		foreach (Item i in food) {
			if (i.crew == null) {
				i.crew = this;
				return i;
			}
		}
		return null;
	}
	
	public bool atLocation(Node2D node) {
		if (node != null) {
			if (Math.Abs(GlobalPosition.X - node.GlobalPosition.X) < nav.TargetDesiredDistance &&
				Math.Abs(GlobalPosition.Y - node.GlobalPosition.Y) < nav.TargetDesiredDistance) {			// if at job location, dequeue job
				return true;
			} 
		}
		return false;
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
	
	public void receiveOrder(JobTarget job, Post post) {
		GD.Print("aye aye capn: " + job.Name + " " + post.Name);
		detachOrders();
		this.job = job;
		this.job.setPosted(false);
		this.job.assignedCrew = this;
		if (this.lastPost == null) {
			this.post = post;
		} else {
			if (this.lastPost.assignedCrew == null || 
				this.lastPost.assignedCrew == this) { 
				if (GlobalPosition.DistanceTo(this.lastPost.GlobalPosition) >= 
					GlobalPosition.DistanceTo(post.GlobalPosition)) {
					this.post = post;
				} else {
					this.post = lastPost;
				}
			} else {
				this.post = post;
			}
		} 
		this.post.assignedCrew = this; 
		this.post.RMSelfSignal += rmSelfPostEvent;
		this.job.RMSelfSignal += rmSelfJobEvent;
	}
	
	public void kickbackOrders() {
		if (this.post != null) {
			this.post.assignedCrew = null;
			this.post = null;
		}
		if (this.job != null) {
			this.job.setPosted(true);
			this.roster.kickbackJob(job);
			this.job.assignedCrew = null;
			this.job = null;
		}
		working = false;
		seekingJob = false;
	}
	
	public void detachOrders() {
		if (this.post != null) {
			this.post.assignedCrew = null;
			this.lastPost = post;
			lastPost.RMSelfSignal += rmSelfLastPostEvent;
			this.post = null;
		}
		if (this.job != null) {
			this.job.setPosted(false);
			this.job.assignedCrew = null;
			this.job = null;
		}
		working = false;
		seekingJob = false;
	}
	
	private void rmSelfJobEvent(GridItem job) {
		this.job = null;
		detachOrders();
	}

	private void rmSelfPostEvent(GridItem post) {
		this.post = null;
		kickbackOrders();
	}
	
	private void rmSelfLastPostEvent(GridItem post) {
		//GD.Print("RM SELF LAST POST");
		this.lastPost = null;
	}
	
	
	public void _on_crewtimer_timeout() {
		float temp = sleep;
		if (sleeping == false) {
			sleep -= sleep_rate;
			if (sleep < 0) {
				sleep = 0;
				health -= sleep_rate;
			}
		}
		else {
			sleep += sleep_rate * 2;
		}
		
		hunger -= hunger_rate;
		if (hunger < 0) {
			hunger = 0;
			health -= hunger_rate;
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
