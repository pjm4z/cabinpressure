using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class PostCtrl : Node2D
{
	public Queue<Post> vacantPosts;
//	public int queuedOrders;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		vacantPosts = new Queue<Post>();
		initPosts();
		
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public void initPosts() {
		var consoleArray = GetChildren()
			.Where(child => child is Post) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<Post>(); // TODO --> change to bed when I have bed class                 

		foreach(var console in consoleArray) {
			vacantPosts.Enqueue(console);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (vacantPosts.Peek().assignedCrew != null) {
			vacantPosts.Enqueue(vacantPosts.Dequeue());
		}
	}
	
	public Post givePost() { // TODO must track filled consoles + available consoles
		if (vacantPosts.Peek().assignedCrew == null) {
			return vacantPosts.Peek();
		}
		return null;
	}
}
