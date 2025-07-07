using Godot;

public partial class Torpedo : Node2D
{
	public float Speed = 400.0f;
	public int Damage = 10;
	
	int rightBarrier;
	int leftBarrier;
	int topBarrier;
	int bottomBarrier;
	
	[Export] private GpuParticles2D wake;
	[Export] private CpuParticles2D explosion;
	[Export] private Sprite2D sprite;
	[Export] private CollisionPolygon2D polygon;
	private Vector2 InitialPosition;
	
	private SurfaceMap surfaceMap;
	
	public override void _Ready()
	{
		//ZIndex = -1;
		wake.Amount = 10;
		ParticleProcessMaterial wakeMaterial = (ParticleProcessMaterial) wake.ProcessMaterial;
		wakeMaterial.Gravity = Vector3.Zero;
		wakeMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
		wakeMaterial.EmissionBoxExtents = new Vector3(5, 1, 1);
		wake.Lifetime = 0.5f;
		wakeMaterial.LifetimeRandomness = 1.0f;
		InitialPosition = Position;
		surfaceMap = (SurfaceMap) GetNode("/root/basescene/surface/surfaceviewport/surfacemap");
	}

	public override void _Process(double delta)
	{
		// Move the torpedo in the direction it's facing (based on rotation)
		Position += (Vector2.Right.Rotated(Rotation) * Speed * (float)delta);
		
		rightBarrier = (int) (InitialPosition.X + GetViewport().GetVisibleRect().Size.X/2);
		leftBarrier = (int) (InitialPosition.X - GetViewport().GetVisibleRect().Size.X/2);
		topBarrier = (int) (InitialPosition.Y - GetViewport().GetVisibleRect().Size.Y/2);
		bottomBarrier = (int) (InitialPosition.Y + GetViewport().GetVisibleRect().Size.Y/2);

		// Destroy the torpedo if it goes off screen (optional, you can also use a timer to destroy it after a while)
		if (Position.X > rightBarrier || Position.X < leftBarrier || 
			Position.Y > bottomBarrier || Position.Y < topBarrier)
		{
			QueueFree();
		}
	}
	
	public void _on_torpedo_body_entered(Node body)
	{
		// If the torpedo collides with a fish, apply damage
		surfaceMap._Splash(Position.X,Position.Y,5);
		if (body is Fish fish)
		{
			fish.ApplyDamage(Damage);  // Call the damage method on the fish
			GracefulQF();
		}
	}
	
	public void GracefulQF() 
	{
		explosion.Emitting = true;
		sprite.Visible = false;
		wake.Emitting = false;
		Speed = 0;
		Timer timer = new Timer();
		timer.WaitTime = wake.Lifetime + 1.0f; // Extra buffer
		timer.OneShot = true;
		timer.Timeout += () => QueueFree();
		AddChild(timer);
		timer.Start();
	}
}
