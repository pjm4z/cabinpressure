using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static System.Collections.Generic.Dictionary<string,object>;

[GlobalClass]
public partial class WireCtrl : Node2D
{
	[Export] private PackedScene wireScene;
	public Dictionary<Vector2I, Wire> wireMap = new Dictionary<Vector2I, Wire>();
	protected PowerGrid grid;
	public int count = 0;
	
	public virtual void init(PowerGrid grid) {
		this.grid = grid;
	}
	
	public void increment() {
		this.count += 1;
	}
	
	public virtual int getCount() {
		return this.count;
	}
	
	public void decrement() {
		this.count -= 1;
		if (count == 0) {
			GD.Print("GOOT BYE " + Name);
			QueueFree();
		}
	}
}
