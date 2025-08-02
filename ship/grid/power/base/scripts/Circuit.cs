using Godot;
using System;

public partial class Circuit : WireCtrl
{
	[Signal]
	public delegate void PowerRQSignalEventHandler(GridItem item);
	public float load = 0;
	public float power = 0;
	public float maxPower = 0;
	private Network powerNetwork;
	
	public void setPowerNetwork(Network powerNetwork) {
		this.powerNetwork = powerNetwork;
	}
	
	public Network getPowerNetwork() {
		if (!IsInstanceValid(this.powerNetwork)) {
			return null;
		}
		return this.powerNetwork;
	}
	
	public bool needsPower(float watts) {
		if (this.power - watts < this.load) {
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
		GD.Print("REQ PWR");
		EmitSignal(nameof(SignalName.PowerRQSignal), rq);
	}
	
	public void addCharge(float watts) {
		if (watts <= 0) {
			this.load -= watts;
		} else {
			this.power += watts;
		}
		GD.Print("ADD: power "  + this.power + " load " + this.load);
	}
	
	public void removeCharge(float watts) {
		if (watts <= 0) {
			this.load += watts;
		} else {
			this.power -= watts;
		}
		GD.Print("REMOVE: power "  + this.power + " load " + this.load);
	}
	
	public void addMaxPower(float watts) {
		this.maxPower += watts;
		GD.Print("MP ADD: " + this.maxPower);
	}
	
	public void removeMaxPower(float watts) {
		this.maxPower -= watts;
		if (this.maxPower < 0) {
			this.maxPower = 0;
		}
		GD.Print("MP RM: " + this.maxPower);
	}
	
	public void addItem(GridItem item) {
		item.setCircuit(this);
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
}
