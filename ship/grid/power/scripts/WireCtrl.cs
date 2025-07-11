using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static System.Collections.Generic.Dictionary<string,object>;

[GlobalClass]
public partial class WireCtrl : Node2D
{
	/*
	* rq power comes from user input, not wpn/JT
	* transmit cur power to each wirectrl
	* each wirectrl reports load?
	
	* override wirectrl, keep power logic here, add data logic to override?-->only use ovr if jobtarget present
	* override both?
	* how to combine power circuits wo f'in w data circuits?
	* create data/pwr classes, GI has 1 of each not just wctrl
	* override addItem in PC + make 2 sep meths for 
	*		setwirectrl, absorb, init, maybe rmslf, etc
	*			maybe b4 or after, only add data circ if JT present, 
	*			only add pwr circ if pwrsrc present ==>  what to do in RM case
					can prune empty circuits in getCxns end of loop                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  for each neighbor
	
				never connect engine to external dataline (maybe if it only contains engine)
				only connect to engine's dataline if cur dl.count == 0 (not include posts) or dl null
				engine will not connect to other daTaline
				pwr draw dataline always priority over engine
	*/
	
	//circuit
	[Signal]
	public delegate void PowerRQSignalEventHandler(GridItem item);
	public float load = 0;
	public float power = 0;
	public float maxPower = 0;

	//network
	private PostCtrl postCtrl;
	public Color color;
	private Random rnd; 
	
	[Export] private PackedScene wireScene;
	public Dictionary<Vector2I, Wire> wireMap = new Dictionary<Vector2I, Wire>();
	private PowerGrid parent;
	public int count = 0;	

	public void init() {
		//network
		initPostCtrl();
		rnd = new Random(); 
		float r = rnd.Next(256)/256f;
		float g = rnd.Next(256)/256f;
		float b = rnd.Next(256)/256f;
		color = new Color(r, g, b, 1f);
	}
	
	//circuit
	public bool needsPower(float watts) {
		if (this.power - watts < this.load) {
			return true;
		} 
		return false;
	}
	
	//circuit
	public bool possible(float watts) {
		if ((watts > 0) || (this.load - watts < this.maxPower)) {
			return true;
		}
		return false;
	}
	
	//circuit
	public bool overloaded() {
		return this.load > this.power;
	}
	
	//circuit
	public void requestPower(GridItem rq) {
		EmitSignal(nameof(SignalName.PowerRQSignal), rq);
	}
	
	//circuit
	public void addCharge(float watts) {
		if (watts <= 0) {
			this.load -= watts;
		} else {
			this.power += watts;
		}
	}
	
	//circuit
	public void removeCharge(float watts) {
		if (watts <= 0) {
			this.load += watts;
		} else {
			this.power -= watts;
		}
	}
	
	//circuit
	public void addMaxPower(float watts) {
		this.maxPower += watts;
	}
	
	//circuit
	public void removeMaxPower(float watts) {
		this.maxPower -= watts;
		if (this.maxPower < 0) {
			this.maxPower = 0;
		}
	}	
	
		//to determine voltage, you would refer to Ohm’s law and take your current (or amps) 
		//times your resistance (or ohms) which would give you your volts of power
		
		//one amp equals .001 kilowatts per hour!
		//.001 hours is equal to 3.6 seconds
		//if there are not enough amps flowing in one part of an appliance it might result in blown breakers and even fires
		
		//ohms law: V = I * R
		//volts = amps * ohms
		//watts = volts * amps (req from devices)
		//current (amps) = grid property
		//potential (volts) = power src property (ex water pressure)
		//resistance (ohms) = device property  (resist pressure)
		
		//Resistance is the measure of how easily electrons can move through a material, 
		//and it’s measured in ohms. The higher the resistance, the more voltage or amperage 
		//will be needed  
	
	//network
	public void initPostCtrl() {
		postCtrl = new PostCtrl();
		postCtrl.init(this);
		AddChild(postCtrl);
	}
	
	//network
	public PostCtrl getPostCtrl() {
		return this.postCtrl;
	}
	
	public void increment() {
		this.count += 1;
	}
	
	public void decrement() {
		this.count -= 1;
		if (count == 0) {
			GD.Print("GOOT BYE");
			QueueFree();
		}
	}
	
	public int getCount() {
		int count = 0;
		count += postCtrl.getCount();
		var consoleArray = GetChildren()
			.Where(child => child is GridItem) 
			.Select(child => child)          
			.Cast<GridItem>(); 
		count += consoleArray.Count();
		return count;
	}
	
	public void addItem(GridItem item) {
		item.setWireCtrl(this);
	}
	
	public void removeSelf() {
		QueueFree();
	}
}
