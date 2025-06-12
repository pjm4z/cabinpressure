using Godot;
using System;

public partial class Fish : CharacterBody2D
{
	public const float MaxSpeed = 75.0f;  // Max forward speed
	public const float TurnSpeed = 1.0f;  // Rotation speed
	public const float Acceleration = 50.0f;  // Acceleration rate
	public const float ChaseRange = 300.0f;  // Range at which fish will chase the boat

	private Vector2 velocity = Vector2.Zero;

	private Vector2 targetPosition; // Target position for the fish to move toward
	private float changeTargetTimer = 2.0f; // Time between random direction changes
	private CharacterBody2D boat;  // Reference to the boat node

	private RandomNumberGenerator rng = new RandomNumberGenerator();  // Random number generator

	public int Health = 100;  // Starting health of the fish

	public void ApplyDamage(int damage)
	{
		Health -= damage;  // Decrease the health by the damage amount
		GD.Print("Fish health: " + Health);

		// Optionally, add more logic for when the fish is dead (e.g., destroy the fish)
		if (Health <= 0)
		{
			GD.Print("Fish is dead!");
			QueueFree();  // Destroy the fish if its health reaches 0
		}
	}
	
	public override void _Ready()
	{
		ZIndex = 1;
		boat = (CharacterBody2D) GetTree().Root.GetNode("basescene/surface/boat");
		GD.Print("Boat" + boat);
		// Create a ShaderMaterial and assign the shader to it

	}

	public override void _PhysicsProcess(double delta)
	{
		
		// Check if the fish is close enough to the boat to start chasing it
		if (Position.DistanceTo(boat.Position) <= ChaseRange)
		{
			targetPosition = boat.Position;
		}
		else
		{
			// Fish is not close enough to chase the boat, so wander randomly
			changeTargetTimer -= (float)delta;
			if (changeTargetTimer <= 0)
			{
				SetNewTargetPosition();
				changeTargetTimer = 2.0f;
			}
		}

		// Move the fish toward the target position (boat or random)
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
