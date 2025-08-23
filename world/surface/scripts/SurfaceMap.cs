using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SurfaceMap : Node2D
{
	// initialize vars spring_constant, dampening
	[Export] private float k = 0.0075f;
	[Export] private float d = 0.008f;
	[Export] private float spread = 0.0002f;
	//[Export] private float spread = 0.0012f;
	private int passes = 8;
	
	[Export] public int PointDist = 35;//25;
	[Export] public Vector2 GridPts;
	[Export] public int subdivisions;
	
	float[,] yDiffs = new float[0,0];
	Vector3[] yDiffs1D = new Vector3[10000];
	List<float> leftDeltas; 
	List<float> rightDeltas;
	List<float> topDeltas;
	List<float> bottomDeltas;
	
	public List<List<SurfacePt>> PointsMap = new List<List<SurfacePt>>();
	List<Line2D> xLines = new List<Line2D>();
	//List<Line2D> yLines = new List<Line2D>();
	public float LeftCol = 0;
	public float RightCol = 0;
	public float TopRow = 0;
	public float BottomRow = 0;
	private int maxYIndex = 0;
	private int topRows = 0;
	private int leftCols;
	
	public int rightBarrier;
	public int leftBarrier;
	public int topBarrier;
	public int bottomBarrier;
	private SurfacePt topLeft;
	private SurfacePt topRight;
	private SurfacePt bottomLeft;
	private SurfacePt bottomRight;
	
	private Texture2D ptTexture;
	private Ship ship;
	private WaterMesh waterMesh;
	private ShaderMaterial material;
	private Shader shader;
	private SurfaceRect surfaceRect;
	private SubViewport subViewport;
	public Camera2D camera;
	
	//private ShaderMaterial M


	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		camera = (Camera2D) GetNode("/root/basescene/surface/ship/playercamera");
		subdivisions = 10;
		// import nodes
		Color color = new Color(255, 255, 0, 0.5f);
		Image img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
		img.Fill(color); // Fill with the solid color (including alpha)
		material = GD.Load<ShaderMaterial>("res://shaders/materials/surfacematerial.tres");
		shader = GD.Load<Shader>("res://shaders/water.gdshader"); // Load your shader
		surfaceRect = (SurfaceRect) GetNode("/root/basescene/surface/surfacerect");
		subViewport = (SubViewport) GetNode("/root/basescene/surface/surfaceviewport");
		// Convert the Image to an ImageTexture (which is a Texture2D)
		ImageTexture texture = ImageTexture.CreateFromImage(img);
		ptTexture = texture;//(Texture2D)GD.Load("res://assets/waterpt.png");//texture;//
		ship = (Ship) GetNode("/root/basescene/surface/ship");
		
		_Initialize_Map();
		_Refresh_Barriers();
		//_Check_Borders();
	}
	
	public void _Check_Max_Y_Index() {
		for (int i = 0; i < PointsMap.Count; i++) {
			if (PointsMap[i].Count > maxYIndex) {
				maxYIndex = PointsMap[i].Count;
			}
		}
	}
	
	public void _Initialize_Lines() {
		/*xLines = new List<Line2D>();
		for (int y = 0; y < PointsMap[0].Count; y++) {
			xLines.Add(new Line2D());
			xLines[y].Width = 2;
			AddChild(xLines[y]);
			for (int x = 0; x < PointsMap.Count; x++) {
				xLines[y].AddPoint(PointsMap[x][y].Position);
			}
		}*/
		//for (int x = 0; x < PointsMap.Count; x++) {
			//yLines.Add(new Line2D());
			//yLines[x].Width = 2;
			//AddChild(yLines[x]);
			//for (int y = 0; y < PointsMap.Count; y++) {
			//	yLines[x].AddPoint(PointsMap[x][y].Position);
			//}
		//}
	}
	
	public void _Initialize_Map_Backup() { 
		rightBarrier = (int) (camera.GlobalPosition.X + camera.GetViewport().GetVisibleRect().Size.X/2);
		leftBarrier = (int) (camera.GlobalPosition.X - camera.GetViewport().GetVisibleRect().Size.X/2);
		topBarrier = (int) (camera.GlobalPosition.Y - camera.GetViewport().GetVisibleRect().Size.Y/2);
		bottomBarrier = (int) (camera.GlobalPosition.Y + camera.GetViewport().GetVisibleRect().Size.Y/2);
		
		int gpX = (int)GridPts.X;
		int gpY = (int)GridPts.Y;
		RightCol = (gpX-1) * PointDist;
		BottomRow = (gpY-1) * PointDist;
		
		for (int x = 0; x < gpX; x++) { // 
			if (x * PointDist < rightBarrier) {
				PointsMap.Add(new List<SurfacePt>());
			} else {
				gpX = x;
				break;
			}
			for (int y = 0; y < gpY; y++) {
				if (y * PointDist < bottomBarrier) {
					SurfacePt sp = new SurfacePt();
					Vector2 pos = new Vector2(PointDist * x, PointDist * y);
					int xPos = PointDist * x; // can add offset here later if needed
					int yPos = PointDist * y;
					AddChild(sp);
					PointsMap[x].Add(sp);
					sp._Initialize(xPos, yPos);
				} else {
					gpY = y;	
					break;
				}
			}
		}
		_Initialize_Lines();
	}
	
	int dt = 0;
	int db = 0;
	Vector2 initPos;
	public void _Initialize_Map() { 
		_Refresh_Barriers();
		List<SurfacePt> initLi = new List<SurfacePt>();
		SurfacePt initSp = new SurfacePt();
		AddChild(initSp);
		initLi.Add(initSp);
		initSp._Initialize((int)initPos.X, (int)initPos.Y - (rightBarrier + bottomBarrier));//._Initialize((int)camera.GlobalPosition.X, (int)camera.GlobalPosition.Y);//0, 0);
		PointsMap.Add(initLi);
		initPos = initSp.Position;
		_Refresh_Barriers();
		int i = 0;
		dt = i;
		i = 0;
		while (bottomLeft.Position.X > (leftBarrier + topBarrier)) { //PointsMap[0][PointsMap[0].Count - 1].Position.Y < bottomBarrier && 
			i++;
			SurfacePt sp = new SurfacePt();
			AddChild(sp);
			sp._Initialize((int)PointsMap[0][i-1].Position.X - PointDist, (int)PointsMap[0][i-1].Position.Y + PointDist);//(PointDist * -i, PointDist * i);
			PointsMap[0].Add(sp);
			bottomLeft = sp;
		}
		int cur = 1;
		int prev = 0;
		
		while (topRight.Position.X < (rightBarrier + bottomBarrier)) { //PointsMap[0][PointsMap[0].Count - 1].Position.Y < bottomBarrier && 
			PointsMap.Add(new List<SurfacePt>());
			i = 0;
			while (i < PointsMap[prev].Count) {
				SurfacePt sp = new SurfacePt();
				AddChild(sp);
				sp._Initialize((int)PointsMap[prev][i].Position.X + PointDist, (int)PointsMap[prev][i].Position.Y + PointDist);
				PointsMap[cur].Add(sp);
				i++;
			}
			topRight = PointsMap[cur][0];
			cur++;
			prev++;
		}
		
		db = i;
		//GD.Print(db + " " + dt + " " + ((2*db) * dt));
		//GD.Print(PointsMap[0].Count);
		//_Move_Grid();
	}
	public void _Expand_Top() {
		for (int i = 0; i < PointsMap.Count; i++) { // insert sp at 0 for each list
			SurfacePt sp = new SurfacePt();
			AddChild(sp);
			sp._Initialize((int)PointsMap[i][0].Position.X + PointDist, (int)PointsMap[i][0].Position.Y - PointDist);
			PointsMap[i].Insert(0, sp);
		}
		PointsMap.Insert(0, new List<SurfacePt>());
		for (int i = 0; i < PointsMap[1].Count; i++) { //add new list at [0], shift up prev 1st line
			SurfacePt sp = new SurfacePt();
			AddChild(sp);
			sp._Initialize((int)PointsMap[1][i].Position.X - PointDist, (int)PointsMap[1][i].Position.Y - PointDist);				
			PointsMap[0].Add(sp);
		}
	}
	
	public void _Shrink_Bottom() {
		for (int i = 0; i < PointsMap.Count; i++) {
			SurfacePt sp = PointsMap[i][PointsMap[i].Count - 1];
			if (IsInstanceValid(sp.curve)) {
				sp.curve._Drop_Line();
			}
			if (IsInstanceValid(sp.curveY)) {
				sp.curveY._Drop_Line();
			}
			if (IsInstanceValid(sp)) {
				sp.QueueFree();
			}
			PointsMap[i].RemoveAt(PointsMap[i].Count-1);
		}
		for (int i = 0; i < PointsMap[PointsMap.Count-1].Count; i++) {
			SurfacePt sp = PointsMap[PointsMap.Count - 1][i];
			if (IsInstanceValid(sp.curve)) {
				sp.curve._Drop_Line();
			}
			if (IsInstanceValid(sp.curveY)) {
				sp.curveY._Drop_Line();
			}
			if (IsInstanceValid(sp)) {
				sp.QueueFree();
			}
		}
		PointsMap.RemoveAt(PointsMap.Count - 1);
	}
	
	public void _Shrink_Top(){
		for (int i = 0; i < PointsMap[0].Count; i++) {
			SurfacePt sp = PointsMap[0][i];
			if (IsInstanceValid(sp.curve)) {
				sp.curve._Drop_Line();
			}
			if (IsInstanceValid(sp.curveY)) {
				sp.curveY._Drop_Line();
			}
			if (IsInstanceValid(sp)) {
				sp.QueueFree();
			}
		}
		PointsMap.RemoveAt(0);		
		for (int i = 0; i < PointsMap.Count; i++) {
			SurfacePt sp = PointsMap[i][0];
			if (IsInstanceValid(sp)) {
				sp.QueueFree();
			}
			PointsMap[i].RemoveAt(0);
		}
	}
	
	public void _Expand_Bottom() {
		for (int i = 0; i < PointsMap.Count; i++) { // insert sp at 0 for each list
			SurfacePt sp = new SurfacePt();
			AddChild(sp);
			sp._Initialize((int)PointsMap[i][PointsMap[i].Count-1].Position.X - PointDist, (int)PointsMap[i][PointsMap[i].Count-1].Position.Y + PointDist);
			PointsMap[i].Add(sp);
		}
		/*PointsMap.Add(new List<SurfacePt>());
		for (int i = 0; i < PointsMap[PointsMap.Count-2].Count; i++) { //add new list at [0], shift up prev 1st line
			SurfacePt sp = new SurfacePt();
			AddChild(sp);
			sp._Initialize((int)PointsMap[1][i].Position.X + PointDist, (int)PointsMap[1][i].Position.Y + PointDist);				
			PointsMap[PointsMap.Count-1].Add(sp);
		}*/
	}
	
	public void _Move_Grid() {
		//	MOVE GRID UP
		_Refresh_Barriers();
		if (topLeft.Position.Y > (leftBarrier + topBarrier)) { // expand top
			_Expand_Top();
		}
		_Refresh_Barriers();
		if (bottomRight.Position.Y > (rightBarrier + bottomBarrier)) { // shrink bottom
			_Shrink_Bottom();
		}
		_Refresh_Barriers();
		// MOVE GRID DOWN
		_Refresh_Barriers();
		if (topLeft.Position.Y < (leftBarrier + topBarrier)) { // shrink top
			_Shrink_Top();
		}
		if (bottomRight.Position.Y < (rightBarrier + bottomBarrier)) { // expand bottom
			//_Expand_Bottom();
		}
	}
	
	public void _Refresh_Barriers() {
		rightBarrier = (int) (camera.GlobalPosition.X + camera.GetViewport().GetVisibleRect().Size.X/2);
		leftBarrier = (int) (camera.GlobalPosition.X - camera.GetViewport().GetVisibleRect().Size.X/2);
		topBarrier = (int) (camera.GlobalPosition.Y - camera.GetViewport().GetVisibleRect().Size.Y/2);
		bottomBarrier = (int) (camera.GlobalPosition.Y + camera.GetViewport().GetVisibleRect().Size.Y/2);
		
		if (PointsMap.Count > 0) {
			if (PointsMap[0].Count <= 0) {
				PointsMap[0].Add(new SurfacePt());
			}
			topLeft = PointsMap[0][0];
			topRight = PointsMap[PointsMap.Count - 1][0];
			bottomLeft = PointsMap[0][PointsMap[0].Count - 1];
			bottomRight = PointsMap[PointsMap.Count - 1][PointsMap[PointsMap.Count - 1].Count - 1];
			LeftCol = topLeft.Position.X;
			RightCol = topRight.Position.X;
			TopRow = topLeft.targetHeight;
			BottomRow = bottomRight.targetHeight;
		}
		
		
	}
	

	public void _Check_CurvesY(Vector4 minMax) {
		for (int x = (int)minMax[0]; x < (int)minMax[2]; x++) {
			for (int y = (int)minMax[1]; y < (int)minMax[3]; y++) {
				if (y > 0 && y < PointsMap[x].Count) {
					if (!IsInstanceValid(PointsMap[x][y].curveY)) {
						PointsMap[x][y].curveY = null;
						PointsMap[x][y].lineYIndex = -1;
					}
					if (!IsInstanceValid(PointsMap[x][y-1].curveY)) {
						PointsMap[x][y-1].curveY = null;
						PointsMap[x][y-1].lineYIndex = -1;
					}
					if (/*Math.Abs(*/PointsMap[x][y].targetHeight - PointsMap[x][y].Position.Y > 1.0) { // (&& y == 13)  point is in motion 
						if (PointsMap[x][y] != null && IsInstanceValid(PointsMap[x][y].curveY)) { // if pt part of curve, reset lifetime
							PointsMap[x][y].curveY.timer.WaitTime = PointsMap[x][y].curveY.wt;
							PointsMap[x][y].curveY.timer.Start();
						} 
						if (PointsMap[x][y].curveY == null) { // if curve is null but pt in motion, assign to curve
							if (PointsMap[x][y-1].curveY != null) { //assign pt to prev curve
								WaveCurve wc = PointsMap[x][y-1].curveY;
								wc.AddSpY(PointsMap[x][y]);
							}
							else  {
								if (PointsMap[x][y-1].curveY == null && PointsMap[x][y-1].curveY == null) { //create new curve, add pt
									WaveCurve wc = new WaveCurve(); 
									AddChild(wc);
									wc.AddSpY(PointsMap[x][y-1]);
									wc.AddSpY(PointsMap[x][y]);
									if (y+1 < PointsMap[x].Count) {
										wc.AddSpY(PointsMap[x][y+1]);
									}
									
								}
							}
						}
						else if (PointsMap[x][y-1].curveY != null && PointsMap[x][y].curveY != PointsMap[x][y-1].curveY) { // --> curve connecting pt
							y += ConnectCurvesY(PointsMap[x][y-1].curveY, PointsMap[x][y].curveY, x, y);
						}
					} 
				}
			}
		}
	}

	public void _Check_Curves(Vector4 minMax) {
		//wcs = new List<WaveCurve>();
		for (int y = (int)minMax[1]; y < (int)minMax[3]; y++) {
			for (int x = (int)minMax[0]; x < (int)minMax[2]; x++) {
				if (x > 0 && x < PointsMap.Count && y > 0 && y < PointsMap[x].Count) {
					if (!IsInstanceValid(PointsMap[x][y].curve)) {
						PointsMap[x][y].curve = null;
						PointsMap[x][y].lineIndex = -1;
					}
					if (!IsInstanceValid(PointsMap[x-1][y].curve)) {
						PointsMap[x-1][y].curve = null;
						PointsMap[x-1][y].lineIndex = -1;
					}
					if (/*Math.Abs(*/PointsMap[x][y].targetHeight - PointsMap[x][y].Position.Y > 1.0) { // (&& y == 13)  point is in motion 
						if (PointsMap[x][y] != null && IsInstanceValid(PointsMap[x][y].curve)) { // if pt part of curve, reset lifetime
							PointsMap[x][y].curve.timer.WaitTime = PointsMap[x][y].curve.wt;
							PointsMap[x][y].curve.timer.Start();
						} 
						if (PointsMap[x][y].curve == null) { // if curve is null but pt in motion, assign to curve
							if (PointsMap[x-1][y].curve != null) { //assign pt to prev curve
								WaveCurve wc = PointsMap[x-1][y].curve;
								wc.AddSp(PointsMap[x][y]);
							}
							else {
								if (y < PointsMap[x+1].Count) {
								if (PointsMap[x-1][y].curve == null && PointsMap[x+1][y].curve == null) { //create new curve, add pt
									WaveCurve wc = new WaveCurve(); 
									AddChild(wc);
									wc.AddSp(PointsMap[x-1][y]);
									wc.AddSp(PointsMap[x][y]);
									wc.AddSp(PointsMap[x+1][y]);
								} 
								}
							}
						}
						else if (/*(PointsMap[x][y].lineIndex + PointsMap[x][y].curve.pointAdjustment) == 0 &&*/ PointsMap[x-1][y].curve != null && PointsMap[x][y].curve != PointsMap[x-1][y].curve) { // --> curve connecting pt
							x += ConnectCurvesX(PointsMap[x-1][y].curve, PointsMap[x][y].curve, x, y);
						}
					} //else { // point is still
						//if (PointsMap[x][y].curve != null) { // remove pt from its curve 
							//WaveCurve wc = PointsMap[x][y].curve;
							//GD.Print("RM");
							//wc.RemoveSp(PointsMap[x][y]); 
						//}
					//}
				}
			}
		}
	}	
	
	private int ConnectCurvesY(WaveCurve lineA, WaveCurve lineB, int x, int y) { // keep line a, remove line b
		if (lineA == null || lineB == null) return 0;
		int yAddition = 0;
		
		int test1 = lineB.curve.GetPointCount();
		while (y < PointsMap[x].Count) {
			if (PointsMap[x][y].curveY != null) {
				lineB.RemoveSpY(PointsMap[x][y]);
				lineA.AddSpY(PointsMap[x][y]);
				
				test1--;
				lineA.spc++;
				lineB.spc--;
			}
			y++;
			yAddition++;
		}
		if (test1 > 0) {
			lineB.spc = 0;
			lineB.curve.ClearPoints();
		}
		
		lineB.QueueFree();
		return yAddition;
	}	
	
	private int ConnectCurvesX(WaveCurve lineA, WaveCurve lineB, int x, int y) { // keep line a, remove line b
		if (lineA == null || lineB == null) return 0;
		int xAddition = 0;
		
		int test1 = lineB.curve.GetPointCount();
		while (x < PointsMap.Count) {
			if (y < PointsMap[x].Count) {
				if (PointsMap[x][y].curve != null) { // index err here
					lineB.RemoveSp(PointsMap[x][y]);
					lineA.AddSp(PointsMap[x][y]);
				
					test1--;
					lineA.spc++;
					lineB.spc--;
				}
			}
			x++;
			xAddition++;
		}
		if (test1 > 0) {
			lineB.spc = 0;
			lineB.curve.ClearPoints();
		}
		
		lineB.QueueFree();
		return xAddition;
	}	

	Vector4 minMax;
	///List<WaveCurve> wcs;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {		
		
		_Refresh_Barriers();
		//_Check_Borders();
		_Move_Grid();
		minMax = _Apply_Force();
		_Check_Curves(minMax);
		_Check_CurvesY(minMax);
		//foreach (WaveCurve wc in wcs) {
			//SmoothCurve(wc);					// address ths
		//}
		//_Check_Lines(_Apply_Force());
		//
		//GD.Print(PointsMap.Count * PointsMap[0].Count);
	}

	public Vector4 _Apply_Force() {
		leftDeltas = new List<float>();
		rightDeltas = new List<float>();
		topDeltas = new List<float>();
		bottomDeltas = new List<float>();
		
		for (int i = 0; i < PointsMap.Count; i++) {
			leftDeltas.Add(0);
			rightDeltas.Add(0);
		}
		if (PointsMap.Count == 0) {
			PointsMap.Add(new List<SurfacePt>());
		}
		for (int i = 0; i < PointsMap[0].Count; i++) {
			topDeltas.Add(0);
			bottomDeltas.Add(0);
		}
		
		int xMin = PointsMap.Count;
		int yMin = PointsMap[0].Count;
		int xMax = 0; 
		int yMax = 0;
		for (int i = 0; i < 1; i++) { // apply force 8 passes per frame
			for (int x = 0; x < PointsMap.Count; x++) {
				for (int y = 0; y < PointsMap[x].Count; y++) { 
					if (i == 0) {
						PointsMap[x][y]._Water_Update(k,d); //float spring_constant, float dampening
					}
					/*Random random = new Random(); // Create a Random instance
	   				if (random.Next(0, 1000) == 0) // random.Next(0, 2) generates 0 or 1
					{
						if (PointsMap[x][y].velocity >= 0)
							PointsMap[x][y].velocity += random.Next(0, 10)/10.0f;//0.5f;
						else if (PointsMap[x][y].velocity > 0)
							PointsMap[x][y].velocity -= random.Next(0, 10)/10.0f;//0.5f;
	  				}*/
					yDiffs1D[(x * PointsMap[0].Count) + y] = new Vector3(PointsMap[x][y].Position.X, PointsMap[x][y].targetHeight, (PointsMap[x][y].Position.Y - PointsMap[x][y].targetHeight));
					
					//GD.Print(spread + " " +  Math.Sin(Math.Abs()));
					leftDeltas[x] = spread * (PointsMap[x][y].height - PointsMap[x][y].targetHeight);
					if (x > 0) {
						PointsMap[x-1][y].velocity += leftDeltas[x];
					}
					rightDeltas[x] = spread * (PointsMap[x][y].height - PointsMap[x][y].targetHeight);
					if (x < PointsMap.Count - 1) { // // error here 2 jagged 2d list --> list of hashmaps w targ position as key, sp as val??
						if (y < PointsMap[x+1].Count-1) {
							PointsMap[x+1][y].velocity += +-rightDeltas[x];
						}
					}
					topDeltas[y] = spread * (PointsMap[x][y].height - PointsMap[x][y].targetHeight);
					if (y > 0) {
						PointsMap[x][y-1].velocity += topDeltas[y];
					}
					bottomDeltas[y] = spread * (PointsMap[x][y].height - PointsMap[x][y].targetHeight);
					if (y < PointsMap[x].Count - 1) {
						PointsMap[x][y+1].velocity += bottomDeltas[y];
					}
					/*if (PointsMap[x][y].line != null && IsInstanceValid(PointsMap[x][y].line)) {
						if (PointsMap[x][y].lineIndex + PointsMap[x][y].line.pointAdjustment >= 0 && 
							PointsMap[x][y].lineIndex + PointsMap[x][y].line.pointAdjustment < PointsMap[x][y].line.GetPointCount() - 1) {
								
								PointsMap[x][y].line.SetPointPosition(PointsMap[x][y].lineIndex + PointsMap[x][y].line.pointAdjustment, PointsMap[x][y].Position);
						} 
					}*/ 
					//QueueRedraw();
					if (PointsMap[x][y].curve != null && IsInstanceValid(PointsMap[x][y].curve)) {
						if (PointsMap[x][y].lineIndex + PointsMap[x][y].curve.pointAdjustment >= 0 && 
							PointsMap[x][y].lineIndex + PointsMap[x][y].curve.pointAdjustment < PointsMap[x][y].curve.curve.GetPointCount() - 1) {
								PointsMap[x][y].curve.curve.SetPointPosition(PointsMap[x][y].lineIndex + PointsMap[x][y].curve.pointAdjustment, PointsMap[x][y].Position);
								if (Math.Abs(PointsMap[x][y].Position.Y - PointsMap[x][y].targetHeight) > 1.0f) {
									PointsMap[x][y].curve.timer.WaitTime = PointsMap[x][y].curve.wt;
									PointsMap[x][y].curve.timer.Start();
								}
								
						} 
					}
					if (PointsMap[x][y].curveY != null && IsInstanceValid(PointsMap[x][y].curveY)) {
						if (PointsMap[x][y].lineYIndex + PointsMap[x][y].curveY.pointAdjustment >= 0 && 
							PointsMap[x][y].lineYIndex + PointsMap[x][y].curveY.pointAdjustment < PointsMap[x][y].curveY.curve.GetPointCount() - 1) {
								PointsMap[x][y].curveY.curve.SetPointPosition(PointsMap[x][y].lineYIndex + PointsMap[x][y].curveY.pointAdjustment, PointsMap[x][y].Position);
								if (Math.Abs(PointsMap[x][y].Position.Y - PointsMap[x][y].targetHeight) > 1.0f) {
									PointsMap[x][y].curveY.timer.WaitTime = PointsMap[x][y].curveY.wt;
									PointsMap[x][y].curveY.timer.Start();
								}
								
						} 
					} 
					if ((y < yMin || y > yMax) && (Math.Abs(topDeltas[y]) > 0 || Math.Abs(bottomDeltas[y]) > 0)) {
						if (y < yMin) {
							yMin = y;
						} if (y > yMax) {
							yMax = y;
						}
					}
					if ((x < xMin || x > xMax) && (Math.Abs(leftDeltas[x]) > 0 || Math.Abs(rightDeltas[x]) > 0)) {
						if (x < xMin) {
							xMin = x;
						} if (x > xMax) {
							xMax = x;
						}
					}
				}
			}
			_Prepare_Shader();
		}
		return new Vector4(xMin, yMin, xMax, yMax);
	}

	
	
	
	public void _Prepare_Shader() {
		_Refresh_Barriers();
		float xOffset = LeftCol - leftBarrier;
		float yOffset = TopRow - topBarrier;
		material.SetShaderParameter("gpX", PointsMap.Count);
		material.SetShaderParameter("gpY", PointsMap[0].Count);
		material.SetShaderParameter("subviewport_texture", subViewport.GetTexture());
		material.SetShaderParameter("xOffset", xOffset);
		material.SetShaderParameter("yOffset", yOffset);
		
		
		float x = (camera.Position.X - xOffset) / PointDist;
		if ((x % 1) > 0.5) {
			x = (camera.Position.X + PointDist - xOffset) / PointDist;
		}
		float y = (camera.Position.Y - yOffset) / PointDist;
		if ((y % 1) > 0.5) {
			y = (camera.Position.Y + PointDist - yOffset) / PointDist;
		}
		float xDist = (x % 1) * PointDist;
		int xInd = (int) x;
		float yDist = (y % 1) * PointDist;
		int yInd = (int) y;
		//float effect_radius = yDiffs1D[(xInd * PointsMap[0].Count) + yInd].Z;
		//float dist = (float) Math.Sqrt((xDist * xDist) + (yDist * yDist));

		
		material.SetShaderParameter("screen_size", GetViewportRect().Size);
		material.SetShaderParameter("yDiffs", yDiffs1D);
		//material.SetShaderParameter("t", GetGlobalMousePosition().X / (rightBarrier - leftBarrier));
		surfaceRect.Material = material;
	}
	
	public void _Splash_Backup(float xPos, float yPos, float speed) {	
		float xDistFactor = (xPos / PointDist) % 1;							//dist of splash pos from nearest pt (x)
		float yDistFactor = (yPos / PointDist) % 1;							//dist of splash pos from nearest pt (y)
		int x = (int) xPos / PointDist;										//x index, not factoring grid offset
		int y = (int) yPos / PointDist;										//y index, not factoring grid offset
		int yOffset = (int) PointsMap[0][0].targetHeight / PointDist;  		//dist of grid from top barrier
		int xOffset = (int) PointsMap[0][0].Position.X / PointDist;			//dist of grid from left barrier
		if ((x - xOffset) >= 0 && (x - xOffset) < PointsMap.Count - 1 && 
			(y - yOffset) >= 0 && (y - yOffset) < PointsMap[x - xOffset].Count - 1) { // if valid indexes with offsets, do splash
				PointsMap[x - xOffset][y - yOffset].velocity -= speed * (1 - xDistFactor) * (1 - yDistFactor);
				PointsMap[x + 1 - xOffset][y - yOffset].velocity -= speed * xDistFactor * (1 - yDistFactor);
				PointsMap[x - xOffset][y + 1 - yOffset].velocity -= speed * (1 - xDistFactor) * yDistFactor;
				PointsMap[x - xOffset][y + 1 - yOffset].velocity -= speed * xDistFactor * yDistFactor;
				//GD.Print("SPLASH" + " " + (x - xOffset) + " " + (y - yOffset));
		}
	}
	
	public void _Splash(float xPos, float yPos, float speed) {	
		float xDistFactor = (xPos / PointDist) % 1;							//dist of splash pos from nearest pt (x)
		float yDistFactor = (yPos / PointDist) % 1;							//dist of splash pos from nearest pt (y)
		int x = (int) xPos / PointDist + (int)(yPos / PointDist) - 1;										//x index, not factoring grid offset
		int y = (int) yPos / PointDist - (int)(xPos / PointDist) - 1;										//y index, not factoring grid offset
		int yOffset = (int) PointsMap[0][0].targetHeight / PointDist;  		//dist of grid from top barrier
		int xOffset = (int) PointsMap[0][0].Position.X / PointDist;			//dist of grid from left barrier
		if ((x - xOffset) >= 0 && (x - xOffset) < PointsMap.Count - 1 && 
			(y - yOffset) >= 0 && (y - yOffset) < PointsMap[x - xOffset].Count - 1) { // if valid indexes with offsets, do splash
				PointsMap[x - xOffset][y - yOffset].velocity -= speed * (1 - xDistFactor) * (1 - yDistFactor);
				PointsMap[x + 1 - xOffset][y - yOffset].velocity -= speed * xDistFactor * (1 - yDistFactor);
				PointsMap[x - xOffset][y + 1 - yOffset].velocity -= speed * (1 - xDistFactor) * yDistFactor;
				PointsMap[x - xOffset][y + 1 - yOffset].velocity -= speed * xDistFactor * yDistFactor;
				//GD.Print("SPLASH" + " " + (x - xOffset) + " " + (y - yOffset));
		}
	}
}
