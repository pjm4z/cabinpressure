using Godot;
using System;

public partial class Health : Node
{
	
	[Export] protected float health = 10;
	protected float MAX_HP;
	protected Node parent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.parent = GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void init(float hp) {
		health = hp;
		MAX_HP = hp;
	}
	
	public void heal(float hp) {
		this.health += hp;
		if (this.health > MAX_HP) {
			this.health = MAX_HP;
		}
	}
	
	public void damage(float damage) {
		this.health -= damage;
		if (damage <= 0) {
			parent.QueueFree();
		}
	}
}
