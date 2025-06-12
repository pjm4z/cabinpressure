using Godot;
using System;

public partial class CrewProgress : ProgressBar
{
	private Weapon wpn;
	private Crew crew;
	private Timer crewTimer;
	public double deltaElapsed;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		crew = (Crew) GetParent();
		crewTimer = (Timer) crew.GetNode("crewtimer");
		this.Visible = false;
		ProcessMode = Node.ProcessModeEnum.Pausable;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.Visible = false;
		if (crew.sleeping == true) {
			this.Visible = true;
			this.MaxValue = Crew.MAX_SLEEP;
			this.Value = crew.sleep;
			deltaElapsed = 0;
		} else if (crew.working == true) {
			wpn = crew.wpn;
			if (wpn.queuedOrders> 0) {
				this.Visible = true;
				deltaElapsed += delta;
				this.MaxValue = wpn.taskTime * 1000;
				this.Value = deltaElapsed * 1000;
			}
		} else {
			deltaElapsed = 0;
		}
	}
}
