using Godot;
using System.Linq;
using System;
using System.Collections.Generic;
using static System.Collections.Generic.Dictionary<string,object>;


public partial class CrewRoster : Node2D
{
	public Dictionary<int,string> firstNameDict = new Dictionary<int,string>();
	public Dictionary<int,string> lastNameDict = new Dictionary<int,string>();
	public Dictionary<string,Crew> crewDict = new Dictionary<string,Crew>();
	public LinkedList<Crew> crewList;
	public Queue<Crew> crewQueue;
	//public LinkedList<Post> jobBoard = new LinkedList<Post>();
	public LinkedList<Weapon> jobBoard = new LinkedList<Weapon>();
	
	public Crew maxReady = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		crewList = new LinkedList<Crew>();
		crewQueue = new Queue<Crew>();
		
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
		if (jobBoard.First != null) {
			if (jobBoard.First.Value.assignedCrew != null || 
			(jobBoard.First.Value.queuedOrders <= 0 && jobBoard.First.Value.active == false)) {
				jobBoard.First.Value.posted = false;
				jobBoard.RemoveFirst();
			}
		}
		if (jobBoard.Last != null) {
			if (jobBoard.Last.Value.assignedCrew != null || 
			(jobBoard.Last.Value.queuedOrders <= 0 && jobBoard.Last.Value.active == false)) {
				jobBoard.Last.Value.posted = false;
				jobBoard.RemoveLast();
			}
		}
		if (jobBoard.Count > 0) {
			if (maxReady != null) {	
				if (maxReady.post == null && maxReady.wpn == null) {
					//if (!seekingJob && !working && !seekingFood) {
					Post post = jobBoard.Last.Value.postCtrl.givePost();
					if (post != null) {
						maxReady.receiveOrder(jobBoard.Last.Value, post);
						maxReady = null;
						jobBoard.RemoveLast();
					}
				} else {
					maxReady = null;
				}										// TODO compare each crew w maxReady in rpoc funct, make readiness rating, re add to end of LL if worker cant fnish job
				
			}
		}
	}
	
	private void initCrew() {
		Random rnd = new Random();
		var crewArray = GetChildren()
			.Where(child => child is Crew) // We only want nodes that we know are Post nodes
			.Select(child => child)          
			.Cast<Crew>();    
		foreach(var crew in crewArray) {
			crewQueue.Enqueue(crew);
			//generate name
			crew.firstName = firstNameDict[rnd.Next(0,9)];
			crew.lastName = lastNameDict[rnd.Next(0,9)];
			crew.sleep = rnd.Next(5,10);
			crewDict.Add(crew.firstName + " " + crew.lastName, crew);
			crew.setNamePlate(crew.firstName, crew.lastName);
			GD.Print(":)________");
			GD.Print(crew.firstName + " " + crew.lastName + " " + crew.sleep);
		}
	}
	
	public void postJob(Weapon /*Post*/ post) {
		jobBoard.AddFirst(post);
		post.posted = true;
	}
	
}
