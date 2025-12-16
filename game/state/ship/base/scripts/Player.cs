using Godot;
using System;

public partial class Player : ShipState
{
	[Export] private ShipState idle;
	[Export] private ShipState follow;
	
	private Skip skip;
	
	private float TurnSpeed;
	private float Acceleration; 
	private float GlobalRotation;
	
	private float angularDrive;
	private Vector2 linearDrive;
	private Vector2 heading;
	
	public override void enter() {
		this.skip = ship.skip;
		
		linearDrive = Vector2.Zero;
		angularDrive = 0f;
		heading = Vector2.Zero;
		
		TurnSpeed = ship.TurnSpeed;
		Acceleration = ship.Acceleration; 
		GlobalRotation = 0f;
	}
		
	public override State process(double delta) {
		ShipState newState = checkPriorities();
		if (newState != null) {
			return newState;
		}
		player();
		return base.process(delta);
	}
	
	private void player() {
		linearDrive = Vector2.Zero;
		angularDrive = 0f;//ship.AngularVelocity;
		GlobalRotation = ship.GlobalRotation;
		float yOff = 0f;
		float xOff = 0f;	
			
		// Handle forward and reverse movement with acceleration
		if (Input.IsActionPressed("ui_up") || Input.IsActionPressed("w")) {
			linearDrive += new Vector2(0f,1f).Rotated(GlobalRotation);
			yOff -= 5f;
		}
		if (Input.IsActionPressed("ui_down") || Input.IsActionPressed("s")) {
			linearDrive += new Vector2(0f,1f).Rotated(GlobalRotation + (float)Math.PI);
			yOff += 5f;
		}
			
		// Handle rotation (turning the ship)
		if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("a")) {
			if (Input.IsActionPressed("shift")) {
				linearDrive += new Vector2(0f,1f).Rotated(GlobalRotation - (float)Math.PI/2f);
				yOff /= 2f;
				xOff -= 2.5f;
			} else {
				angularDrive = -TurnSpeed;
			}
		}
		if (Input.IsActionPressed("ui_right") || Input.IsActionPressed("d")) {
			if (Input.IsActionPressed("shift")) {
				linearDrive += new Vector2(0f,1f).Rotated(GlobalRotation + (float)Math.PI/2f);
				yOff /= 2f;
				xOff += 2.5f;
			} else {
				angularDrive = TurnSpeed;
			}
		}
		
		if (yOff != 0f) {
			xOff /= 2f;
		}
			

		ship.ApplyTorque(angularDrive);
		//ship.GlobalRotation += angularDrive * ship.delta;
		Vector4 result = ship.limitVelocity(linearDrive.Normalized() * Acceleration);
		linearDrive = new Vector2(result.X, result.Y);
		
		if (result.Z == 0f) {
			ship.ApplyCentralForce(linearDrive);//, new Vector2(xOff, yOff).Rotated(ship.GlobalRotation));
		} else if (result.Z == 1f) {
			ship.ApplyCentralImpulse(linearDrive);//, new Vector2(xOff, yOff).Rotated(ship.GlobalRotation));
		}
		
		if (result.W == 0f) {
			ship.linearDrive = linearDrive;
		}
	}
	float cons = 200f; // move to appliued heading, or use total calculated linear drive?
	float v = 200f;
	Vector2 prevVel = Vector2.Zero;
	
	public override ShipState checkPriorities() {
		if (skip == null) {
			return idle;
		}
		return null;
	}
}
