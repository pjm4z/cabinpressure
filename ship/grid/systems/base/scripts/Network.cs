using Godot;
using System;
using System.Collections.Generic;

public partial class Network : WireCtrl
{
	public Dictionary<Vector2I, ShipSystem> jobs = new Dictionary<Vector2I, ShipSystem>();
	public Dictionary<Vector2I, Engine> engines = new Dictionary<Vector2I, Engine>();
	private PostCtrl postCtrl;
	public Color color;
	private Random rnd; 
	
	
	public override void init(PowerGrid grid) {
		base.init(grid);
		initPostCtrl();
		rnd = new Random(); 
		float r = rnd.Next(256)/256f;
		float g = rnd.Next(256)/256f;
		float b = rnd.Next(256)/256f;
		color = new Color(r, g, b, 1f);
	}
	
	public void initPostCtrl() {
		postCtrl = new PostCtrl();
		postCtrl.init(this);
		AddChild(postCtrl);
	}
	
	public PostCtrl getPostCtrl() {
		return this.postCtrl;
	}
	
	public void addItem(GridItem item) {
		item.setNetwork(this);
	}
	
	public void clearJobCount() {
		this.postCtrl.clearJobCount();
	}
	
	public int jobCount() {
		return this.postCtrl.getJobCount();
	}
	
	public int engineCount() {
		return this.postCtrl.getEngineCount();
	}
	
	public void addEngine(Engine engine) {
		engines[engine.getTilePos()] = engine;
	}
	
	public void removeEngine(Engine engine) {
		engines.Remove(engine.getTilePos());
	}
	
	public void addJob(ShipSystem job) {
		jobs[job.getTilePos()] = job;
	}
	
	public void removeJob(ShipSystem job) {
		jobs.Remove(job.getTilePos());
	}
	
	public void reportToJobs(ref HashSet<Vector2I> visited) {
		HashSet<Vector2I> visitedJobs = new HashSet<Vector2I>();
		foreach (Vector2I key in this.jobs.Keys) {
			if (!visitedJobs.Contains(key)) {
				Network nw = this.grid.newEmptyNetwork();
				jobs[key].networkReportEvent(ref visited, ref visitedJobs, nw);
			}
		}
	}
	
	public void reportToEngines(ref HashSet<Vector2I> visited) {
		HashSet<Vector2I> visitedEngines = new HashSet<Vector2I>();
		foreach (Vector2I key in this.engines.Keys) {
			if (!visitedEngines.Contains(key)) {
				//visitedEngines.Add(key);
				Network nw = this.grid.newEmptyNetwork();
				engines[key].networkReportEvent(ref visited, ref visitedEngines, nw);
			}
		}
	}
}
