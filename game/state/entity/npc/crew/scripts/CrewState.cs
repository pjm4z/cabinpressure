using Godot;
using System;

[GlobalClass]
public partial class CrewState : State
{
	protected Crew crew;
	
	public override void ready() {
		base.ready();
		crew = (Crew) base.parent;
	}
}
