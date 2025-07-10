using Godot;
using System;

public partial class SurfacePt : Sprite2D // ,make pt shader, takes surfacemap + pt pos as uniforms
{
	public float velocity = 0.0f;
	public float force = 0.0f;
	public float height = 0.0f;
	public float targetHeight = 0.0f;
	
	public float current = 0.5f;
	//public bool partOfLine = false;
	//public WaveLine line;
	public WaveCurve curve;
	public int lineIndex = -1;
	
	public WaveCurve curveY;
	public int lineYIndex = -1;
	Timer timer;
	
	GpuParticles2D particles;
	ParticleProcessMaterial particleMaterial;
	ShaderMaterial shaderMaterial;
	Shader shader;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timer = new Timer();
		timer.Timeout += TimeOut;
		timer.OneShot = true; 
		AddChild(timer);
		timer.WaitTime = 0.1f;
		Texture = (Texture2D)GD.Load("res://assets/waterpt.png");
				
		//SetupTimer();
		//particles = new GpuParticles2D();
		//particles.Emitting = false;
		//particles.Amount = 4;
		//particleMaterial = new ParticleProcessMaterial();
		
		//particleMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere;
		//particleMaterial.EmissionSphereRadius = 25f;
		//particleMaterial.Gravity = new Vector3(0, 0, 0);
		//particles.ProcessMaterial = particleMaterial;
		
		//shaderMaterial = new ShaderMaterial();
		//shader = (Shader) GD.Load("res://shaders/particleshader.gdshader");
		
		//shaderMaterial.Shader = shader;
		//particles.Material = shaderMaterial;
		
		//particles.Lifetime = 5.0f;
		//AddChild(particles);
	}
	
	public override void _Process(double delta) {
		//shaderMaterial.SetShaderParameter("gravity", new Vector3(0, (Position.Y - targetHeight)*100, 0));
		//particles.Material = shaderMaterial;
		//targetHeight += current;
		/*Vector2 pos = Position;
		pos.Y += current;
		Position = pos;*/
		//((ParticleProcessMaterial)particles.ProcessMaterial).Gravity = new Vector3(0, (Position.Y - targetHeight)*100, 0);
		
	}
	
	public void SetupTimer() {
		
		
		
		// Set to false if you want it to repeat
		 // Attach it to a parent node
		timer.Start();
	}
	
	public void TimeOut() {
		//Math.Abs(targetHeight - Position.Y) > 1.0
		//height != targetHeight
		
		/*if (Math.Abs(targetHeight - Position.Y) > 1.0) {
			height = targetHeight;
			Position = new Vector2(Position.X, height);
			SetupTimer();														//TODO only run timer if pt is in motion
		} else {
			//GD.Print("TO");
			if (IsInstanceValid(curve) && curve != null) {
				curve.RemoveSp(this);
			}
			_Drop();
		}*/
	}
	
	public void _Water_Update(float spring_constant, float dampening) {
		// hookes law -> F = - K * x
		//Position.Y += current;
		
		height = Position.Y;
		
		float curExt = height - targetHeight;
		float loss = -dampening * velocity;
		force = -spring_constant * curExt + loss;
		velocity += force;
		Vector2 pos = Position;
		pos.Y += velocity;
		Position = pos;
	}
	
	public void _Drop() {
		//line = null; 
		lineIndex = -1;
		
		curveY = null;
		lineYIndex = -1;
		
		curve = null;
	}
	
	public void _Initialize(int x, int y) {
		height = y;
		targetHeight = y;
		Position = new Vector2(x, y);
		velocity = 0;
	}
}
