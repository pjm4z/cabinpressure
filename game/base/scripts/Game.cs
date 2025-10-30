using Godot;
using System;

public partial class Game : Node
{
	public static Game Instance;
	public PlayerCamera camera;
	
	public Vector2 zero = new Vector2(0,0);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {}
	
	
	public bool HasMethod(object objectToCheck, string methodName) {
		var type = objectToCheck.GetType();
		return type.GetMethod(methodName) != null;
	} 
	
	public bool HasProperty(object objectToCheck, string propertyName) {
		var type = objectToCheck.GetType();
		return type.GetProperty(propertyName) != null;
	} 
	
	/*public bool dictContains(Dictionary<string, object> objectToCheck, string propertyName) {
		
	}*/
}
