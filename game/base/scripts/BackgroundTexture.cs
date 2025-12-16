using Godot;
using System;

public partial class BackgroundTexture : TextureRect
{
	SubViewport subViewport;
	MeshInstance2D surfaceMesh;
	private Node2D ship;
	Vector2 pos = new Vector2();
	
	public override void _Ready()
	{
		// Create a SubViewport and add it to the parent Node2D
		subViewport = (SubViewport) GetNode("%background");
		Texture = subViewport.GetTexture(); 
		
	}
	public override void _Draw() {
		Texture = subViewport.GetTexture();
	}
}
