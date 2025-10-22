using Godot;
using System;

public partial class BaseScene : Node2D
{
  	[Export] private CharacterBody2D ship;  // Reference to the ship node
	Camera2D mainCamera;
	Camera2D surfaceCamera;
	private bool paused = false;
	//[Export] private SurfaceMap surfaceMap;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mainCamera = (Camera2D) GetNode("/root/basescene/surface/ship/playercamera");
		surfaceCamera = (Camera2D) GetNode("/root/basescene/surface/surfaceviewport/surfacecamera");
		
		ProcessMode = Node.ProcessModeEnum.Always;
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
		if (mainCamera != null && surfaceCamera != null) {
			surfaceCamera.GlobalPosition = mainCamera.GlobalPosition; // ship.GlobalPosition;
			surfaceCamera.Zoom = mainCamera.Zoom;
		}
	}
	
	public override void _Input(InputEvent inputEvent) {
		if (Input.IsActionJustReleased("space")) { //todo nav doesnt pause (crew in motion)
													// maybe from nav callback in crew script calling movenadslide?
			GetTree().Paused = !GetTree().Paused;
		}
		if (Input.IsActionJustReleased("m")) { //todo nav doesnt pause (crew in motion)
													// maybe from nav callback in crew script calling movenadslide?
			GetTree().Paused = !GetTree().Paused;
			GD.Print("map");
		}
	}
}
