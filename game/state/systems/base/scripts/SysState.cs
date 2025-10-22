using Godot;
using System;

[GlobalClass]
public partial class SysState : State
{
	protected ShipSystem sys;
	
	protected int load_factor = 10;
	
	public override void ready() {
		base.ready();
		sys = (ShipSystem) base.parent;
	}
	
	public override State process(double delta) {
		handleLoad((float)delta * load_factor);
		return base.process(delta);
	}
	
	protected virtual void handleLoad(float delta) {
		if (sys.hasLoad()) {
			sys.removeLoad(delta);
		}
	}
}
