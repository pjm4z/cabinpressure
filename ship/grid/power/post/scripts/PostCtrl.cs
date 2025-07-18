using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static System.Collections.Generic.Dictionary<string,object>;

public partial class PostCtrl : Node2D
{
	private Network network;
	public Post maxReady;
	private int jobCount = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		ProcessMode = Node.ProcessModeEnum.Always;
		this.Name = "postCtrl";
	}
	
	public void init(Network network) {
		this.network = network;
	}
	
	public void clearJobCount() {
		this.jobCount = 0;
	}
	
	public int getJobCount() {
		return jobCount;
	}
	
	private int engineCount = 0;
	
	public void addEngine(Engine engine) {
		this.engineCount += 1;
		this.network.addEngine(engine);
	}
	
	public void removeEngine(Engine engine) {
		this.engineCount -= 1;
		this.network.removeEngine(engine);
	}
	
	public int getEngineCount() {
		return this.engineCount;
	}
	
	public void addJob(JobTarget job) {
		this.jobCount += 1;
		if (this.engineCount > 0) {
			GD.Print("REPORTING ");
			HashSet<Vector2I> visited = new HashSet<Vector2I>();
			this.network.reportToEngines(ref visited);
		}
	}
	
	public void removeJob(JobTarget job) {
		this.jobCount -= 1;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (maxReady != null) {
			if (maxReady.assignedCrew != null) {
				maxReady = null;
			}
		}
	}
	
	public Post givePost() { 
		if (maxReady != null) {
			if (maxReady.assignedCrew == null) {
				return maxReady;
			}
		}
		return null;
	}
	
	public int getCount() {
		var consoleArray = GetChildren()
			.Where(child => child is GridItem) 
			.Select(child => child)          
			.Cast<GridItem>(); 
		return consoleArray.Count();
	}
	
	public Post getMaxReady() {
		return maxReady;
	}
	
	public void setMaxReady(Post post) {
		this.maxReady = post;
		if (this.maxReady != null) {
			this.maxReady.RMSelfSignal += RMSelfMaxReady;
		}
	}	
	
	private void RMSelfMaxReady(GridItem mr) {
		this.maxReady = null;
	}
}
