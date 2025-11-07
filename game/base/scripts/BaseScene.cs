using Godot;
using System;
using Godot.Collections;
//using System.Collections.Generic;

public partial class BaseScene : Node2D
{
	[Export] Ship ship;
	Camera2D mainCamera;
	Camera2D bgCamera;
	SubViewport subViewport;
	TextureRect textureRect;
	bool paused = false;
	CelestialBody star;
	CelestialBody planet;
	CelestialBody moon;
	Material material;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mainCamera = (Camera2D) GetNode("ship/playercamera");
		bgCamera = (Camera2D) GetNode("background/bgcamera");
		subViewport = (SubViewport) GetNode("background");
		textureRect = (TextureRect) GetNode("bgtexture");
		star = (CelestialBody) GetNode("star");
		planet = (CelestialBody) GetNode("star/planet");
		moon = (CelestialBody) GetNode("star/planet/moon");
		ship = (Ship) GetNode("ship");
		material = textureRect.Material;
		//ProcessMode = Node.ProcessModeEnum.Always;
		//Godot.Engine.TimeScale =.5f;
	}

	private void PrintSceneTree(Node node, int indent = 0)
	{
		string indentation = new string(' ', indent);
		GD.Print(indentation + node.Name);  // Print the current node's name
	
		// Recursively print the child nodes
		foreach (Node child in node.GetChildren())
		{
			PrintSceneTree(child, indent + 2);  // Increase indentation for children
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		
		if (mainCamera != null && bgCamera != null) {
			bgCamera.GlobalPosition = mainCamera.GlobalPosition / new Vector2(10,10); // ship.GlobalPosition;
			bgCamera.Zoom = new Vector2(1,1);//mainCamera.Zoom;
		}
		
		Vector2 s = DisplayServer.WindowGetSize() * new Vector2(1/0.75f,1/0.75f);
		Vector2 p = mainCamera.GlobalPosition;
		
		p.X -= s.X/(2 * mainCamera.Zoom.X);
		p.Y -= s.Y/(2 * mainCamera.Zoom.X);
		subViewport.Size = (Vector2I)s;
		textureRect.Size = s / mainCamera.Zoom;
		textureRect.Position = p;
		
		if (material is ShaderMaterial shaderMaterial) {
			Godot.Collections.Array bodies = new Godot.Collections.Array();
			bodies.Add(new Vector3(star.realPos.X, star.realPos.Y, star.mass));
			bodies.Add(new Vector3(planet.realPos.X, planet.realPos.Y, planet.mass));
			bodies.Add(new Vector3(moon.realPos.X, moon.realPos.Y, moon.mass));
			// ... proceed to set parameters
			shaderMaterial.SetShaderParameter("bodies", bodies);
			shaderMaterial.SetShaderParameter("elements", 2);
			shaderMaterial.SetShaderParameter("scale", ship.star.Scale.X);
			shaderMaterial.SetShaderParameter("speed", ship.LinearVelocity.Length());
			shaderMaterial.SetShaderParameter("zoom", mainCamera.Zoom.X);
		}
		else {
			GD.PrintErr("Material is not a ShaderMaterial!");
		}
	}
	
	public override void _Input(InputEvent inputEvent) {
		if (Input.IsActionJustReleased("space")) { //todo nav doesnt pause (crew in motion)
													// maybe from nav callback in crew script calling movenadslide?
			GetTree().Paused = !GetTree().Paused;
			//this.GlobalPosition = ship.GlobalPosition;
		}
		if (Input.IsActionJustReleased("m")) { //todo nav doesnt pause (crew in motion)
													// maybe from nav callback in crew script calling movenadslide?
			GetTree().Paused = !GetTree().Paused;
			GD.Print("map");
			mainCamera.Zoom = new Vector2(0.001f, 0.001f);
		}
	}
}
