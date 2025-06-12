using Godot;
using System;
using System.Collections.Generic;
public partial class WaveCurve : Path2D						// have combined waveline and smoothpath code into single class. now we operate on curve instead of line, 
															// class implements path2d instead of line2d, so may need to interact w it slightlyt diff in class but should be mostly same. 
															// try to replace waveline w wavecurve in map. everything else is same.
{
	public Curve2D curve = new Curve2D();
	
	public int pointAdjustment = 0;
	public int spc = 0;
	public Timer timer;
	public float Width;
	public float wt = 0.1f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		curve = new Curve2D();
		timer = new Timer();
		timer.WaitTime = wt;
		timer.Timeout += OnTimerTimeout;
		timer.OneShot = true; // Set to false if you want it to repeat
		AddChild(timer); // Attach it to a parent node
		timer.Start();
		Width = 0.1f;
		//curve.Visible = true;
	}
	
	public void OnTimerTimeout() {
		//GD.Print("TO" + " " + curve.GetPointCount() + " " + spc);
		_Drop_Line();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Smooth = true;
		QueueRedraw();
		//AddChild(wc);
		if (curve.GetPointCount() == 0 || spc <= 0) { 
			_Drop_Line();
		}
	}
	
	public void _Drop_Line() {
		//GD.Print("DL");
		curve.ClearPoints();
		QueueFree();
	}
	
	public void AddSp(SurfacePt sp) {
		curve.AddPoint(sp.Position);
		sp.lineIndex = curve.GetPointCount() - 1 - pointAdjustment;
		sp.curve = this;
		spc++;
	}
	
	public void AddSpY(SurfacePt sp) {
		curve.AddPoint(sp.Position);
		sp.lineYIndex = curve.GetPointCount() - 1 - pointAdjustment;
		sp.curveY = this;
		spc++;
	}	
	
	public void RemoveSp(SurfacePt sp) {
		//GD.PrintErr("RM" + curve.GetPointCount());
		spc--;
		if (curve.GetPointCount() == 0 || spc == 0 || sp.lineIndex + pointAdjustment <= 0) {
			_Drop_Line();
		}
		if (sp.lineIndex + pointAdjustment == 0 && curve.GetPointCount() > 0) {
			curve.RemovePoint(0);
			pointAdjustment--;
		} else if (sp.lineIndex + pointAdjustment == curve.GetPointCount() - 1) {
			curve.RemovePoint(curve.GetPointCount() - 1);
		}
	}
	
	public void RemoveSpY(SurfacePt sp) {
		spc--;
		if (curve.GetPointCount() == 0 || spc == 0 || sp.lineYIndex + pointAdjustment <= 0) {
			_Drop_Line();
		}
		if (sp.lineYIndex + pointAdjustment == 0 && curve.GetPointCount() > 0) {
			curve.RemovePoint(0);
			pointAdjustment--;
		} else if (sp.lineYIndex + pointAdjustment == curve.GetPointCount() - 1) {
			curve.RemovePoint(curve.GetPointCount() - 1);
		}
	}	
	
	
	// START SMOOTHPATH CODE
	
	
	
		[Export] public float SplineLength = 8;
	private bool _smooth;
	private bool _straighten;
	[Export] public Color Color = new Color(1, 1, 1, 1);

	[Export]
	public bool Smooth
	{
		get => _smooth;
		set
		{
			_smooth = value;
			if (_smooth)
				ApplySmooth();
		}
	}

	[Export]
	public bool Straighten
	{
		get => _straighten;
		set
		{
			_straighten = value;
			if (_straighten)
				ApplyStraighten();
		}
	}

	private void ApplyStraighten()
	{
		GD.Print("ST");
		int pointCount = curve.GetPointCount();
		for (int i = 0; i < pointCount; i++)
		{
			curve.SetPointIn(i, Vector2.Zero);
			curve.SetPointOut(i, Vector2.Zero);
		}
	}

	private void ApplySmooth()
	{
	//	GD.Print("SM");
		int pointCount = curve.GetPointCount();
		for (int i = 0; i < pointCount; i++)
		{
			if (i > 0 && i < pointCount - 1)
			{
				Vector2 spline = GetSpline(i);
				curve.SetPointIn(i, -spline);
				curve.SetPointOut(i, spline);
			}
		}
	}

	private Vector2 GetSpline(int i)
	{
		Vector2 lastPoint = GetPoint(i - 1);
		Vector2 nextPoint = GetPoint(i + 1);
		Vector2 spline = lastPoint.DirectionTo(nextPoint) * SplineLength;
		return spline;
	}

	private Vector2 GetPoint(int i)
	{
		int pointCount = curve.GetPointCount();
		i = WrapIndex(i, 0, pointCount);

		if (i > 1 && i < pointCount - 1)
		{
			return curve.GetPointPosition(i);
		}
		else if (i <= 1)
		{
			return new Vector2(curve.GetPointPosition(1).X - SplineLength, curve.GetPointPosition(1).Y);
		}
		else if (i >= pointCount - 1)
		{
			return new Vector2(curve.GetPointPosition(pointCount - 1).X + SplineLength, curve.GetPointPosition(pointCount - 1).Y);
		}

		return Vector2.Zero;
	}

	private int WrapIndex(int i, int min, int max)
	{
		return (i < min) ? max : (i > max) ? min : i;
	}

	public override void _Draw()
	{
		Vector2[] points = curve.GetBakedPoints();
		if (points.Length >= 2)
		{
			DrawPolyline(points, Color, Width, true);
		}
	}
}
