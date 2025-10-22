using Godot;
using System;
using System.Collections.Generic;

public partial class ShieldBarrier : RigidBody2D
{
	private float TurnSpeed = 10f;
	[Export] private CollisionPolygon2D[] barrier;
	
	[Export] public Ship ship;
	private Boolean enabled;
	private Shield parent;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	
	public void setEnabled(bool enabled) {
		this.enabled = enabled;
		
		if (!enabled) {
			foreach (CollisionPolygon2D q in barrier) {
				q.Disabled = true;
			}
		} else {
			foreach (CollisionPolygon2D q in barrier) {
				q.Disabled = false;
			}
		}
	}
	
	public void init(Shield parent) {
		this.parent = parent;
		ship = parent.ship;
		GlobalPosition = ship.GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//LookAt(GetGlobalMousePosition());
		float angle = GetAngleTo(GetGlobalMousePosition()) - 1.5708f;
		
		float rot = 0f;
		
		if (angle > 0) {
			rot = TurnSpeed;
		}
		if (angle < 0) {
			rot = -TurnSpeed;
		}
		if (Math.Abs(TurnSpeed) < 1) {
			rot *= -(1/TurnSpeed);
		}

		//ApplyTorque(rot);
		
		//GD.Print(angle);
	}
}
