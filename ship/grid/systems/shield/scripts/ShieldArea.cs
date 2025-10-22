using Godot;
using System;

public partial class ShieldArea : Area2D
{
	public Ship ship;
	
	private Shield parent;
	private Boolean enabled;
	private CollisionShape2D shape;
	private float TurnSpeed = 10f;
	private Sprite2D sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//barrier = (Shield) Getbarrier();
		//ship = barrier.ship;
		//enabled = barrier.enabled;
		sprite = (Sprite2D) GetNode("sprite");
		shape = (CollisionShape2D) GetNode("shape");
		sprite = (Sprite2D) GetNode("sprite");
	}
	
	public void setEnabled(bool enabled) {
		this.enabled = enabled;
		
		if (!enabled) {
			shape.Disabled = true;
			sprite.Visible = false;
		} else {
			shape.Disabled = false;
			sprite.Visible = true;
		}
	}
	
	public void init(Shield shield) {
		parent = shield;
		ship = parent.ship;
		Color currentModulate = sprite.Modulate; // Get the current color and alpha
		currentModulate.A = 0.5f; // Set the alpha component to 0.5 (50% transparency)
		sprite.Modulate = currentModulate;
		GlobalPosition = ship.GlobalPosition;
	}
	
	public void damage(float damage) {
		parent.damage(damage);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//LookAt(GetGlobalMousePosition());
	}
}
