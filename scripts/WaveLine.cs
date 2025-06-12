/*using Godot;
using System;
using System.Collections.Generic;
public partial class WaveLine : Line2D
{
	
	public int pointAdjustment = 0;
	public int spc = 0;
	public Timer timer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timer = new Timer();
		timer.WaitTime = .5f;
		timer.Timeout += OnTimerTimeout;
		timer.OneShot = true; // Set to false if you want it to repeat
		AddChild(timer); // Attach it to a parent node
		timer.Start();
		Width = 0.5f;
		Visible =  true;
		JointMode = Line2D.LineJointMode.Round;
		EndCapMode = Line2D.LineCapMode.Round;
	}
	
	public void OnTimerTimeout() {
		_Drop_Line();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GetPointCount() == 0 || spc <= 0) { 
			_Drop_Line();
		}
	}
	
	public void _Drop_Line() {
		ClearPoints();
		QueueFree();
	}
	
	public void AddSp(SurfacePt sp) {
		AddPoint(sp.Position);
		sp.lineIndex = GetPointCount() - 1 - pointAdjustment;
		sp.line = this;
		spc++;
	}
	
	public void AddSpY(SurfacePt sp) {
		AddPoint(sp.Position);
		sp.lineYIndex = GetPointCount() - 1 - pointAdjustment;
		sp.curveY = this;
		spc++;
	}	
	
	public void RemoveSp(SurfacePt sp) {
		spc--;
		if (GetPointCount() == 0 || spc == 0 || sp.lineIndex + pointAdjustment <= 0) {
			_Drop_Line();
		}
		if (sp.lineIndex + pointAdjustment == 0 && GetPointCount() > 0) {
			RemovePoint(0);
			pointAdjustment--;
		} else if (sp.lineIndex + pointAdjustment == GetPointCount() - 1) {
			RemovePoint(GetPointCount() - 1);
		}
	}
	
	public void RemoveSpY(SurfacePt sp) {
		spc--;
		if (GetPointCount() == 0 || spc == 0 || sp.lineYIndex + pointAdjustment <= 0) {
			_Drop_Line();
		}
		if (sp.lineYIndex + pointAdjustment == 0 && GetPointCount() > 0) {
			RemovePoint(0);
			pointAdjustment--;
		} else if (sp.lineYIndex + pointAdjustment == GetPointCount() - 1) {
			RemovePoint(GetPointCount() - 1);
		}
	}	
}*/
