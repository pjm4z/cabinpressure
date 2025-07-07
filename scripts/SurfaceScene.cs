using Godot;
using System;

public partial class SurfaceScene : Node2D
{
	
	SubViewport subViewport;
	SurfaceMap surfaceMap;
	//MeshInstance2D surfaceMesh;
	TextureRect textureRect;
	Node2D boat;
	public Camera2D camera;
//	WaterMesh waterMesh;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		boat = (Node2D) GetNode("/root/basescene/surface/boat");
		subViewport = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		textureRect = (TextureRect) GetNode("/root/basescene/surface/surfacerect");
		surfaceMap = (SurfaceMap) GetNode("/root/basescene/surface/surfaceviewport/surfacemap");
		//waterMesh = (WaterMesh) GetNode("/root/basescene/surface/surfacemapscene/surfaceviewport/watermesh");
		camera = (Camera2D) GetNode("/root/basescene/surface/boat/playercamera");
		 // textureRect.StretchMode = TextureRect.StretchModeEnum.Scale;
		ProcessMode = Node.ProcessModeEnum.Pausable;
	// Make sure it always stays at (0,0) and covers the screen
	//textureRect.Size = GetViewport().GetVisibleRect().Size;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GD.Print(DisplayServer.WindowGetSize());
		Vector2 s = DisplayServer.WindowGetSize() * new Vector2(1/0.75f,1/0.75f);
		Vector2 p = camera.GlobalPosition;
		
		p.X -= s.X/2;
		p.Y -= s.Y/2;
		subViewport.Size = (Vector2I)s;
		textureRect.Size = s;
		textureRect.Position = p;
	}
}
