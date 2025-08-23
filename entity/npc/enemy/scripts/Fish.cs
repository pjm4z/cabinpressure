using Godot;
using System;

public partial class Fish : CharacterBody2D
{
	public const float MaxSpeed = 75.0f;  // Max forward speed
	public const float TurnSpeed = 1.0f;  // Rotation speed
	public const float Acceleration = 50.0f;  // Acceleration rate
	public const float ChaseRange = 300.0f;  // Range at which fish will chase the ship

	private Vector2 velocity = Vector2.Zero;

	private Vector2 targetPosition; // Target position for the fish to move toward
	private float changeTargetTimer = 2.0f; // Time between random direction changes
	private Ship ship;  // Reference to the ship node

	private RandomNumberGenerator rng = new RandomNumberGenerator();  // Random number generator

	public int Health = 100;  // Starting health of the fish

	public void applyDamage(int damage) {
		Health -= damage; 
		if (Health <= 0) {
			QueueFree();
		}
	}
	
	public override void _Ready() {
		ZIndex = 1;
		ship = (Ship) GetTree().Root.GetNode("basescene/surface/ship"); //main/svc/sv/
	}

	public override void _PhysicsProcess(double delta)
	{
		
		// Check if the fish is close enough to the ship to start chasing it
		if (Position.DistanceTo(ship.Position) <= ChaseRange)
		{
			targetPosition = ship.Position;
		}
		else
		{
			// Fish is not close enough to chase the ship, so wander randomly
			changeTargetTimer -= (float)delta;
			if (changeTargetTimer <= 0)
			{
				SetNewTargetPosition();
				changeTargetTimer = 2.0f;
			}
		}

		// Move the fish toward the target position (ship or random)
		Vector2 direction = (targetPosition - Position).Normalized();
		velocity = direction * MaxSpeed;

		// Apply movement
		Velocity = velocity;
		MoveAndSlide();
	}
	
	private void SetNewTargetPosition()
	{
		// Randomly generate a new target position within a set range from the fish's current position
		float randomX = rng.RandfRange(-200, 200);  // Random X offset
		float randomY = rng.RandfRange(-200, 200);  // Random Y offset
		targetPosition = Position + new Vector2(randomX, randomY);
	}
}
