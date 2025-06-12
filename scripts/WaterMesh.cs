using Godot;
using System;
using System.Collections.Generic;
public partial class WaterMesh : MeshInstance2D
{
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void UpdateMeshFromGrid(List<List<SurfacePt>> gridPoints)
	{
		int cols = gridPoints.Count;
		int rows = gridPoints[0].Count;
		
		// Get the node that has the ShaderMaterial
		ShaderMaterial shaderMat = (ShaderMaterial) this.Material;
		shaderMat.SetShaderParameter("grid_width", rows);  
		shaderMat.SetShaderParameter("grid_width", cols);  
		
	}
}
