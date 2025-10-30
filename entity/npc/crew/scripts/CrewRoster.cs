using Godot;
using System.Linq;
using System;
using System.Collections.Generic;
using static System.Collections.Generic.Dictionary<string,object>;


public partial class CrewRoster : Node2D
{
	private Ship parent;
	public Dictionary<int,string> firstNameDict = new Dictionary<int,string>();
	public Dictionary<int,string> lastNameDict = new Dictionary<int,string>();
	public LinkedList<ShipSystem> jobBoard = new LinkedList<ShipSystem>();
	
	public Weapon nextOrder = null;
	public Crew maxReady = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		parent = (Ship) GetParent();
		
		// TOPDO --> can get duplucate keys in name dict
		firstNameDict[0] = "Allen";
		firstNameDict[1] = "Bruce";
		firstNameDict[2] = "Charlie";
		firstNameDict[3] = "Daniel";
		firstNameDict[4] = "Edgar";
		firstNameDict[5] = "Frank";
		firstNameDict[6] = "Gerry";
		firstNameDict[7] = "Hank";
		firstNameDict[8] = "Isaac";
		firstNameDict[9] = "John";
		
		lastNameDict[0] = "Abrams";
		lastNameDict[1] = "Bennet";
		lastNameDict[2] = "Cardoz";
		lastNameDict[3] = "Daniels";
		lastNameDict[4] = "Evans";
		lastNameDict[5] = "Fuchs";
		lastNameDict[6] = "Grant";
		lastNameDict[7] = "Hensley";
		lastNameDict[8] = "Iqbal";
		lastNameDict[9] = "Jennings";
		
		initCrew();
		
		ProcessMode = Node.ProcessModeEnum.Always;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// check if first wpn is not avail, prune
		if (jobBoard.First != null) {
			if (jobBoard.First.Value.assignedCrew != null || 
				(jobBoard.First.Value.count() <= 0 && jobBoard.First.Value.getActive() == false)) {
				jobBoard.First.Value.setPosted(false);
				jobBoard.RemoveFirst();
			}
		}
		// check if last wpn is not avail, prune
		if (jobBoard.Last != null) {
			if (jobBoard
				.Last
				.Value
				.assignedCrew != null || 
			(jobBoard.Last.Value.count() <= 0 && jobBoard.Last.Value.getActive() == false)) {
				jobBoard.Last.Value.setPosted(false);
				jobBoard.RemoveLast();
			}
		}
		// if avail try to assign post/crew
		if (jobBoard.Count > 0) {
			if (maxReady != null) {	
				Post post = jobBoard.Last.Value.getPost();
				if (post != null) {
					maxReady.receiveOrder(jobBoard.Last.Value, post);
					maxReady = null;
					jobBoard.RemoveLast();
				} 
			}
		}
	}
	public int rank = 1;
	private void initCrew() {
		
		var crewArray = GetChildren()
			.Where(child => child is Crew) // We only want nodes that we know are Post nodes
			.Select(child => child)          
			.Cast<Crew>();  
			  
		foreach(var crew in crewArray) {
			crew.ship = parent;
			crew.roster = this;
			
			crew.GlobalPosition = parent.GlobalPosition;
			crew.TopLevel = true;
			
			//crew.rank = rank;
			//rank += 1;
			//generate name
			nameCrew(crew);
		//	Random rnd = new Random();
		//	crew.sleep = rnd.Next(5,10);
		}
	}
	
	public void nameCrew(Crew crew) {
		Random rnd = new Random();
		crew.firstName = firstNameDict[rnd.Next(0,9)];
		crew.lastName = lastNameDict[rnd.Next(0,9)];
		crew.setName(crew.firstName, crew.lastName);
	}
	
	public void postJob(ShipSystem job) {
		GD.Print("jopb posted");
		jobBoard.AddFirst(job);
		job.RMSelfSignal += rmSelfWpnEvent;
		job.setPosted(true);
	}
	
	public void kickbackJob(ShipSystem job) {
		jobBoard.AddLast(job);
		job.RMSelfSignal += rmSelfWpnEvent;
		job.setPosted(true);
	}
	
	private void rmSelfWpnEvent(GridItem wpn) {
		if (wpn is Weapon) {
			this.jobBoard.Remove((Weapon)wpn);
		}
	}	
}
