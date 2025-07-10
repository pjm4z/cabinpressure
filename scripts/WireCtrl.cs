using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static System.Collections.Generic.Dictionary<string,object>;

public partial class WireCtrl : Node2D
{
	
	[Signal]
	public delegate void PowerRQSignalEventHandler(GridItem item);
	
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
	*			only add pwr circ if pwrsrc present
	*/
	
	private PostCtrl postCtrl;
	
	public float load = 0;
	public float power = 0;
	public float maxPower = 0;
	
	private PackedScene wireScene;
	public Dictionary<Vector2I, Wire> wireMap = new Dictionary<Vector2I, Wire>();

	private int wireGroupSeq;
	private PowerGrid parent;
	private int alertDevicesSeq = 0;
	private int checkCxnsSeq = 0;
	private Dictionary<string, int> adMap = new Dictionary<string, int>();
	private Dictionary<int, int> groupRemaps = new Dictionary<int, int>();
	
	private Random rnd; 
	public Color color;
	
	public int count = 0;

	public void init() {
		initPostCtrl();
		rnd = new Random(); 
		float r = rnd.Next(256)/256f;
		float g = rnd.Next(256)/256f;
		float b = rnd.Next(256)/256f;
		color = new Color(r, g, b, 1f); //new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256));
		//GD.Print("RGB " + r + " " + g + " " + b);
	//	parent = (PowerGrid) GetParent();
		wireScene = GD.Load<PackedScene>("res://scenes/wire.tscn");
	}
	
	public bool needsPower(float watts) {
		if (this.power - watts < this.load) {		// power coming as 0 after 
			// state exiting + taking power off grid, makes condition pass when it shouldnt
			//GD.Print(power + " " + load + "; " + watts);
			return true;
		} 
		return false;
	}
	
	public bool possible(float watts) {
		if ((watts > 0) || (this.load - watts < this.maxPower)) {
			return true;
		}
		return false;
	}
	
	public bool overloaded() {
		return this.load > this.power;
	}
	
	public void requestPower(GridItem rq) {
		EmitSignal(nameof(SignalName.PowerRQSignal), rq);
	}
	
	public void addCharge(float watts) {
		if (watts <= 0) {
			this.load -= watts;
		} else {
			this.power += watts;
		}
	}
	
	public void removeCharge(float watts) {
		if (watts <= 0) {
			this.load += watts;
		} else {
			this.power -= watts;
		}
	}
	
	public void addMaxPower(float watts) {
		this.maxPower += watts;
	}
	
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
	
	public void initPostCtrl() {
		postCtrl = new PostCtrl();
		postCtrl.init(this);
		AddChild(postCtrl);
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
			.Where(child => child is GridItem) // TODO --> change to bed when I have bed class
			.Select(child => child)          
			.Cast<GridItem>(); // TODO --> change to bed when I have bed class                 
					   
		count += consoleArray.Count();
		return count;
	}
	
	public void addItem(GridItem item) {
		//GD.Print(item + " " + this);
		item.setWireCtrl(this);
	}	
	
	public PostCtrl getPostCtrl() {
		return this.postCtrl;
	}
	
	public void removeSelf() {
		QueueFree();
	}
}
