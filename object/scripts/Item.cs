using Godot;
using System;

public partial class Item : Area2D	 				//TODO  --:> inventory sys, ship should handle logic to give items to crew
{
	[Export] public float weight;
	public Crew crew;
	private bool pickedUp = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.crew != null && pickedUp == true) {
			this.Visible = false;
			this.GlobalPosition = this.crew.GlobalPosition;
		}
	}
	
	public void pickUp(Crew crew) {
		this.crew = crew;
		this.pickedUp = true;
		crew.soughtItem = null;
		crew.inventory = this;
	}
	
	public void use(Crew crew) {
		crew.hunger = 10f;
		crew.sleep = 10f;
		crew.sleeping = false;
		crew.inventory = null;
		QueueFree();
	}
}
