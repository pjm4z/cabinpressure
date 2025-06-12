using Godot;
using System;

public partial class BoatNavRegion : NavigationRegion2D
{
	private bool needsBake = true;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		//BakeNavigationPolygon();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (IsBaking() == false && needsBake == true) {
			BakeNavigationPolygon();
			needsBake = false;
		}
	}
}
