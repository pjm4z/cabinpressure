using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static System.Collections.Generic.Dictionary<string,object>;

public partial class PostCtrl : Node2D
{
	private WireCtrl wireCtrl;
	//private Dictionary<int, Post> postMap = new Dictionary<int, Post>();
	public Post maxReady;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public void init(WireCtrl wireCtrl) {
		this.wireCtrl = wireCtrl;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (maxReady != null) {
			if (maxReady.assignedCrew != null) {
				maxReady = null;
			}
		}
	}
	
	public Post givePost() { // TODO must track filled consoles + available consoles
		if (maxReady != null) {
			if (maxReady.assignedCrew == null) {
				//GD.Print("Returning " + maxReady);
				return maxReady;
			}
		}
		//GD.Print("Returning null");
		return null;
	}
	

	
/*	public Post givePost(int groupId) { // TODO must track filled consoles + available consoles
		Post value;
		if (postMap.TryGetValue(groupId, out value)) {
			if (value != null) {
				if (value.assignedCrew == null) {
					return value;
				}
			}
		}
		//GD.Print("Returning null"); // todo --> dont call givePost if all posts are taken
		return null;
	}*/
	
	public int getCount() {
		var consoleArray = GetChildren()
			.Where(child => child is GridItem) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<GridItem>(); // TODO --> change to bed when I have bed class                 
		
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
