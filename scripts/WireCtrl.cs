using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static System.Collections.Generic.Dictionary<string,object>;

public partial class WireCtrl : Node2D
{
	private PostCtrl postCtrl;
	//private WeaponCtrl wpnCtrl;
	
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {

	}
	
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
	
	public void initPostCtrl() {
		postCtrl = new PostCtrl();
		postCtrl.init(this);
		AddChild(postCtrl);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	
	public void increment() {
		this.count += 1;
	}
	
	public void decrement() {
		this.count -= 1;
		if (count == 0) {
			//GD.Print("GOOT BYE");
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
