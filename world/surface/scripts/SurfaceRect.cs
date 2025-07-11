using Godot;
using System;

public partial class SurfaceRect : TextureRect
{
	SubViewport subViewport;
	SurfaceMap surfaceMap;
	WaterMesh waterMesh;
	MeshInstance2D surfaceMesh;
	private Node2D ship;
	Vector2 pos = new Vector2();
	
	public override void _Ready()
	{
		// Create a SubViewport and add it to the parent Node2D
		subViewport = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		surfaceMap = (SurfaceMap) GetNode("/root/basescene/surface/surfaceviewport/surfacemap");
		//swaterMesh = (WaterMesh) GetNode("/root/basescene/surface/surfacemapscene/surfaceviewport/watermesh");
		ship = (Node2D) GetNode("/root/basescene/surface/ship");
		surfaceMap.Visible = true;
		//waterMesh.Visible = true;
		Texture = subViewport.GetTexture(); 

		// Apply a shader to the TextureRect
		ShaderMaterial material = GD.Load<ShaderMaterial>("res://shaders/materials/watermaterial.tres");
		material.Shader = GD.Load<Shader>("res://shaders/water.gdshader"); // Load your shader
		Material = material;
		
	}
	public override void _Draw() {
				Texture = subViewport.GetTexture();
				
	}
}
