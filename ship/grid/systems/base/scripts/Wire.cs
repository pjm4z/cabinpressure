using Godot;
using System;
using System.Collections.Generic;

public partial class Wire : GridItem
{
	private AnimatedSprite2D sprite;
	private AnimatedSprite2D spriteSub;
	public Dictionary<string, GridItem> adjItems = new Dictionary<string, GridItem>(); 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		//spriteSub = (AnimatedSprite2D) GetNode("sprite/sprite");
	}
	
	private Dictionary<string, int> spriteDict = new Dictionary<string, int>();
	
	private string spriteKey = "";
	private string top = "top";
	private string right = "right";
	private string bottom = "bottom";
	private string left = "left";
	
	private void initSprites() {
		// no neighbors
		spriteDict[""] = 0;
		// 1 neighbor
		spriteDict[top] = 1;
		spriteDict[right] = 2;
		spriteDict[bottom] = 3;
		spriteDict[left] = 4;
		// 2 neighbors (straight)
		spriteDict[top+bottom] = 5;
		spriteDict[right+left] = 6;
		// 2 neighbors (90 degrees)
		spriteDict[top+right] = 7;
		spriteDict[right+bottom] = 8;
		spriteDict[bottom+left] = 9;
		spriteDict[top+left] = 10;
		// 3 neighbors 
		spriteDict[top+right+bottom] = 11;
		spriteDict[right+bottom+left] = 12;
		spriteDict[top+bottom+left] = 13;
		spriteDict[top+right+left] = 14;
		// 4 neighbors 
		spriteDict[top+right+bottom+left] = 15;
	}
	
	public override void init(PowerGrid grid, Vector2I tilePos, Vector2 localPos) {
		base.init(grid, tilePos, localPos);
		
		this.sprite = (AnimatedSprite2D)GetNode("sprite");
		initSprites();
		checkSprite();
		if (this.sprite != null) {
			this.sprite.Modulate = network.color; 
		}
	}
	
	public override void setNetwork(Network network) {
		base.setNetwork(network);
		if (this.sprite != null) {
			this.sprite.Modulate = network.color; 
		}
	}
	
	private bool checkItem(string dir) {
		if (this.adjItems[dir] != null) {
			return true;
		}
		return false;
	}
	
	public void checkSprite() {
		getAdjItems();
		spriteKey = "";
		if (checkItem(top)) {
			spriteKey += top;
		}
		if (checkItem(right)) {
			spriteKey += right;
		}
		if (checkItem(bottom)) {
			spriteKey += bottom;
		}
		if (checkItem(left)) {
			spriteKey += left;
		}
		sprite.Frame = spriteDict[spriteKey];
		//spriteSub.Frame = spriteDict[spriteKey];
	}
	
	public void checkSprite(Vector2I tilePos) {
		getAdjItems(tilePos);
		spriteKey = "";
		if (checkItem(top)) {
			spriteKey += top;
		}
		if (checkItem(right)) {
			spriteKey += right;
		}
		if (checkItem(bottom)) {
			spriteKey += bottom;
		}
		if (checkItem(left)) {
			spriteKey += left;
		}
		sprite.Frame = spriteDict[spriteKey];
		//spriteSub.Frame = spriteDict[spriteKey];
	}
	
	private void getAdjItems() {
		this.adjItems[top] = grid.getItem(this.tilePos + new Vector2I(0, -1));
		this.adjItems[right] = grid.getItem(this.tilePos + new Vector2I(1, 0));
		this.adjItems[bottom] = grid.getItem(this.tilePos + new Vector2I(0, 1));
		this.adjItems[left] = grid.getItem(this.tilePos + new Vector2I(-1, 0));
	}
	
	private void getAdjItems(Vector2I tilePos) {
		if (this.tilePos + new Vector2I(0, -1) != tilePos) {
			this.adjItems[top] = grid.getItem(this.tilePos + new Vector2I(0, -1));
		} else {
			this.adjItems[top] = null;
		}
		if (this.tilePos + new Vector2I(1, 0) != tilePos) {
			this.adjItems[right] = grid.getItem(this.tilePos + new Vector2I(1, 0));
		} else {
			this.adjItems[right] = null;
		}
		if (this.tilePos + new Vector2I(0, 1) != tilePos) {
			this.adjItems[bottom] = grid.getItem(this.tilePos + new Vector2I(0, 1));
		} else {
			this.adjItems[bottom] = null;
		}
		if (this.tilePos + new Vector2I(-1, 0) != tilePos) {
			this.adjItems[left] = grid.getItem(this.tilePos + new Vector2I(-1, 0));
		} else {
			this.adjItems[left] = null;
		}
	}
}
