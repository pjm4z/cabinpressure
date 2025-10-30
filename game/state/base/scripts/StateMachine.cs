using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass]
public partial class StateMachine : Node
{
	[Export] State startState;
	public State state;
	public Node parent;
	
	// Called when the node enters the scene tree for the first time.
	public void init() {
		parent = GetParent();
		initStates();
		GD.Print(startState.Name + " " + parent.Name);
		changeState(startState);
	}
	
	private void initStates() {
		var states = this.GetChildren()
			.Where(child => child is State) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<State>(); // TODO --> change to bed when I have bed class                 

		foreach(var state in states) {
			state.parent = parent;
			state.ready();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void process(double delta) {
		changeState(state.process(delta));
	}
	
	private void changeState(State newState) {
		if (newState != null) {
			if (state != null) {
				state.exit();
			}
			state = newState;
			state.enter();
		}
	}
}
