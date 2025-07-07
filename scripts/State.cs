using Godot;
using System;

[GlobalClass]
public partial class State : Node
{
	
	public CharacterBody2D parent;
	
	public virtual void ready() {
		
	}
	
	public virtual State process(double delta) {
		return null;
	}
	
	public virtual void enter() {
		
	}
	 
	public virtual void exit() {
		
	}
	
	public virtual State checkPriorities() {
		return null;
	}
}
