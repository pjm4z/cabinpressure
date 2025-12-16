using Godot;
using System;
using System.Collections.Generic;

public partial class Map : Panel
{
	[Signal]
	public delegate void ShowMapSignalEventHandler(Ship ship);
	
	private Dictionary<string, CelestialBody> bodyMap = new Dictionary<string, CelestialBody>();
	private Dictionary<string, Sprite2D> pinMap = new Dictionary<string, Sprite2D>();

	[Export] Ship ship;
	Space space;
	
	SubViewport viewport;
	TextureRect canvas;
	Sprite2D sprite;
	Camera2D mapCamera;
	
	Texture2D pinTexture;
	
	Vector2 window = Vector2.Zero;
	Vector2 offset = Vector2.Zero;
	float aspect = 0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		viewport = (SubViewport) GetNode("viewport");
		canvas = (TextureRect) GetNode("canvas");
		sprite = (Sprite2D) GetNode("viewport/sprite");
		mapCamera = (Camera2D) GetNode("viewport/mapCamera");
		pinTexture = (Texture2D) GD.Load("res://misc/assets/icon.svg");
		GetViewport().SizeChanged += refresh;
		
		space = (Space) GetNode("%space");
		
		addPin(ship);
		
		this.Visible = false;
	}	
	
	private void refresh() {
		hide();
		show();
	}
	
	private void hide() {
		List<Sprite2D> sprites = Game.Instance.CollectChildren<Sprite2D>(this.viewport);
		foreach(Sprite2D sprite in sprites) {
			GD.Print(sprite.Name);
			//sprite.QueueFree();
		}
	}
	
	private void show() {
		//List<CelestialBody> bodies = Game.Instance.CollectChildren<CelestialBody>(this.space);
		
		window = DisplayServer.WindowGetSize();// - new Vector2I(100,100);
		viewport.Size = (Vector2I) window;
		canvas.Size = (Vector2I) window;
		
		float length = 5000000f;
		offset = window/length;
		aspect = offset.X / offset.Y;
		offset.X /= aspect;
		
		if (bodyMap.Values.Count != space.bodies.Count) {
			foreach(CelestialBody body in space.bodies.Values) {
				if (!bodyMap.ContainsKey(body.Name)) {
					bodyMap[body.Name] = body;
				}
				Vector2 size = body.getSize();
				//texture.Size *= offset;
				addPin(body);
				drawItem(body, body.realPos, body.getSize());
				//List<CelestialBody> planets = Game.Instance.CollectChildren<CelestialBody>(star);
			}
		}
		
		drawItem(ship, ship.GlobalPosition, (new Vector2(1f,1f)/offset) * 10f);
		//(ship.GlobalPosition * offset) 
	}
	
	private void drawItem(Node2D item, Vector2 pos, Vector2 size) {
		Sprite2D pin = pinMap[item.Name];
		pin.Position = ((pos - ship.GlobalPosition) * offset) + (window/2);
		pin.Scale = (size * offset) / pinTexture.GetSize();// * 10f;// / 10f;'
		if (pin.Scale.X != pin.Scale.Y) {
			pin.Scale = new Vector2(pin.Scale.Y, pin.Scale.Y);
		}
		if (pin.Scale.Length() < 0.01f) {
			pin.Scale = new Vector2(0.01f,0.01f);
		}
	}
	
	private void addPin(Node2D body) {
		Sprite2D pin = new Sprite2D();
		pin.Texture = pinTexture;
		viewport.AddChild(pin);
		pinMap[body.Name] = pin;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		mapCamera.Position = window / 2;
		canvas.Texture = viewport.GetTexture();
		//GD.Print("!!!!!");
		foreach(CelestialBody body in bodyMap.Values) {
			//GD.Print("!! " + body.realPos);
			drawItem(body, body.realPos, body.getSize());
		}
		drawItem(ship, ship.GlobalPosition, (new Vector2(1f,1f)/offset) * 10f);
//		GlobalPosition = Vector2.Zero;
	}
	
	public override void _Input(InputEvent inputEvent) {
		if (Input.IsActionJustReleased("m")) { //todo nav doesnt pause (crew in motion)
													// maybe from nav callback in crew script calling movenadslide?
			this.Visible = !this.Visible;
			if (this.Visible) {
				show();
			} else {
				hide();
			}
		}
		
		Vector2 zoomSpeed = new Vector2(1f,1f);
		if (Input.IsActionPressed("scrollup")) {
			//if (mapCamera.Zoom + zoomSpeed < new Vector2(4f,4f)) {
				mapCamera.Zoom = mapCamera.Zoom + zoomSpeed;
			//}
			/*else {
				mapCamera.Zoom = new Vector2(4f,4f);
			}*/
			GD.Print("^^^^^^ " + mapCamera.Zoom);
		}
		if (Input.IsActionPressed("scrolldown")) {
			if (mapCamera.Zoom - zoomSpeed >= new Vector2(0.1f,0.1f)) {
				mapCamera.Zoom = mapCamera.Zoom - zoomSpeed;
			} 
			GD.Print("vvvvvv " + mapCamera.Zoom);
		}
	}
}
