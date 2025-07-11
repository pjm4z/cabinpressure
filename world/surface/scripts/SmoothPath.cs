using Godot;
using System;

[Tool]
public partial class SmoothPath : Path2D // does curve 2d have setptpos? may replace line2d w curve, cut out the middle man :3
{
	[Export] public float SplineLength = 8;
	private bool _smooth;
	private bool _straighten;
	[Export] public Color Color = new Color(1, 1, 1, 1);
	public float Width = 8;
	public Curve2D curve;

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
		int pointCount = curve.GetPointCount();
		for (int i = 0; i < pointCount; i++)
		{
			curve.SetPointIn(i, Vector2.Zero);
			curve.SetPointOut(i, Vector2.Zero);
		}
	}

	private void ApplySmooth()
	{
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
		if (points.Length > 0)
		{
			DrawPolyline(points, Color, Width, true);
		}
	}
}
