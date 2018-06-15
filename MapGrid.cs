using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MapGrid : MonoBehaviour {

	private Transform myTransform;

	private LineRenderer line;

	public List<EncounterTile> distanceTiles = new List<EncounterTile>();

	// Player
	private Transform player;
	public GameObject playerClosest;
	public float playerOffset = 1.0f;
	public Vector2 playerDirection = Vector2.zero;

	public bool loadBattle = false;

	// Encounters
	private List<GameObject> encounterTiles = new List<GameObject>();
	private int detectionMax = 5;
	private float destroyDistance = 10.0f;
	private float encounterDistance = 2.0f;	

	// Needs
	Global.areaTypes currentArea = Global.areaTypes.plains;
	public int timeNeed = 0;
	public float meatNeed = 0;
	public float plantsNeed = 0;
	public float waterNeed = 0;
	private float maxHeightDifference = 1.0f;
	private float foodMultiply = 0.0f;
	private float waterMultiply = 0.0f;

	// Tiles
	public List<GameObject> tiles;
	private Vector3 tileOffset = new Vector3(3.0f, 0.0f, 1.73f);

	// Path
	public int pathPos = 0;
	public List<MapTile> pathFinal = new List<MapTile>();
	public List<MapTile> pathPlanned = new List<MapTile>();
	private GameObject pathStart;


	void Start() {
		myTransform = transform;
		player = GameObject.FindGameObjectWithTag("Player").transform;
		Global.system.map = this;

		line = myTransform.Find("Line").GetComponent<LineRenderer>();


		// Get tiles
		Transform tileParent = myTransform.Find("Ground");
		for(int i = 0; i < tileParent.childCount; i++) {
			Transform city = tileParent.GetChild(i);
			for(int c = 0; c < city.childCount; c++) {
				Transform type = city.GetChild(c);
				for(int t = 0; t < type.childCount; t++) {
					GameObject tile = type.GetChild(t).gameObject;
					if(tile.GetComponent<MapTile>()) {
						tiles.Add(tile);
					}
				}
			}
		}
		SetCoords();
		PlayerPosition();
		ReloadEncounters();
	}


	void Update() {
		if(!Global.system.isPaused) {
			Inputs();
			ColorPath();
			CheckEncounters();
		}
	}
	

	void Inputs() {
		if(Input.GetKeyDown(KeyCode.R)) {
			if(Global.system.playerMove) {
				Global.system.playerStop = true;
			}
			else {
				ResetPath();
			}
		}
	}
	

	// PLAYER

	public void PlayerPosition() {
		float closestSqr = Mathf.Infinity;
		GameObject closest = null;
		for(int i = 0; i < tiles.Count; i++) {
			Vector3 pos = tiles[i].transform.position;
			float distanceSqr = (pos - player.position).sqrMagnitude;
			if(distanceSqr < closestSqr) {
				closest = tiles[i];
				closestSqr = distanceSqr;
			}
		}
		if(closest != null) {
			playerClosest = closest;
			pathStart = playerClosest.gameObject;
			playerClosest.GetComponent<MapTile>().Indicate(MapTile.indicators.highlight);
			Vector3 playerPos = closest.transform.position;
			playerPos.y += playerOffset;
			player.position = playerPos;
		}
	}


	// TILES AND PATH
	
	void SetCoords() {

		// Set coordinates
		Vector3 start = tiles[0].transform.position;
		Vector3 coords = new Vector3(0, 0, 0);
		for(int i = 0; i < tiles.Count; i++) {

			// Get coordinate x
			float posX = tiles[i].transform.position.x;
			float x = (posX - start.x) / tileOffset.x;
			x = Mathf.Round(x);
			coords.x = x;

			// Get coordinate z
			float posZ = tiles[i].transform.position.z;
			float z = (posZ - start.z) / tileOffset.z;
			z = Mathf.Round(z);
			coords.y = z;

			// Set coordinates
			tiles[i].GetComponent<MapTile>().coords = coords;
		}

		// Move even Y by 1 and divide all by 2
		for(int i = 0; i < tiles.Count; i++) {
			if(Mathf.Abs(tiles[i].GetComponent<MapTile>().coords.y) % 2 == 1) {
				tiles[i].GetComponent<MapTile>().coords.y -= 1;
			}
			if(tiles[i].GetComponent<MapTile>().coords.y != 0) {
				tiles[i].GetComponent<MapTile>().coords.y /= 2;
			}
		}
	}


	public void FindPath(Vector3 goalPos, bool add, bool moveStart) {
		MapTile startTile = pathStart.GetComponent<MapTile>();
		MapTile goalTile = GetTile(goalPos).GetComponent<MapTile>();
		pathPlanned.Clear();
		if(startTile != goalTile) {
			foreach(var t in tiles) {
				MapTile ts = t.GetComponent<MapTile>();
				if(ts.pathState != MapTile.pathStates.obstacle) {
					ts.pathState = MapTile.pathStates.unvisited;
				}
			}
			int steps = 0;
			startTile.pathState = MapTile.pathStates.current;
			List<MapTile> current = new List<MapTile>();
			current.Add(startTile);
			bool pathFound = false;

			int forceExit = 0;
			while(current.Count != 0) {
				foreach(var c in current) {
					if(c != goalTile) {
						foreach(var a in Adjacent(c.coords, true)) {
							MapTile ts = a.GetComponent<MapTile>();
							if(ts.pathState == MapTile.pathStates.unvisited) {
								ts.pathState = MapTile.pathStates.current;
								ts.previous = c;
							}
							c.pathState = MapTile.pathStates.visited;
						}
					}
					else {
						pathFound = true;
						current.RemoveAll(a => a is MapTile);
						pathPlanned.Add(c);
						MapTile temp = c.previous;
						steps++;
						while(temp != startTile) {
							pathPlanned.Add(temp);
							temp = temp.previous;
							steps++;
						}
						break;
					}
					current = new List<MapTile>();
					foreach(var t in tiles) {
						if(t.GetComponent<MapTile>().pathState ==
							MapTile.pathStates.current) {
								current.Add(t.GetComponent<MapTile>());
						}
					}
				}
				forceExit++;
				if(forceExit > tiles.Count * 2) { break; }
			}

			//Debug.Log(steps);
			if(pathFound) {
				pathPlanned.Reverse();
				for(int i = 0; i < pathPlanned.Count; i++) {
					if(pathPlanned[i].GetComponent<MapTile>().indicator != MapTile.indicators.path) {
						pathPlanned[i].GetComponent<MapTile>().Indicate(MapTile.indicators.highlight);
					}
				}
				TravelNeed();

				if(add) {
					bool addToFinal = false;

					// Check that the final part of the path is not repeated
					if (pathFinal.Count >= pathPlanned.Count) {
						pathFinal.Reverse();
						for(int i = 0; i < pathPlanned.Count; i++) {
							if(pathPlanned[i] != pathFinal[i]) {
								addToFinal = true;
							}
						}
						pathFinal.Reverse();
					}
					else {
						addToFinal = true;
					}

					// Add temporary path to final path
					if (addToFinal) {
						for(int p = 0; p < pathPlanned.Count; p++) {
							pathFinal.Add(pathPlanned[p]);
						}

						// Move the starting position
						if(moveStart) {
							pathStart = pathFinal[pathFinal.Count - 1].gameObject;
						}
					}
				}
			}
			else {
				Debug.Log("Path not found");
			}
		}
	}

	public GameObject GetTile(Vector2 coords) {
		foreach(var t in tiles) {
			if(t.GetComponent<MapTile>().coords == coords) {
				return t;
			}
		}
		return null;
	}


	//Return all tiles adjacent to the tile at Vector2 pos as a list
	public List<GameObject> Adjacent(Vector2 pos, bool useHeight) {
		List<GameObject> temp = new List<GameObject>();
		GameObject t = GetTile(pos);

		if(t != null) {
			foreach(GameObject g in tiles) {
				Vector2 coords = g.GetComponent<MapTile>().coords;
				if(coords.x % 2 != 0) {
					if(coords.x == pos.x - 1) {
						if(coords.y == pos.y || coords.y == pos.y - 1) {
							if(CheckHeightDifference(g, t, useHeight)) {
								temp.Add(g);
							}
						}
					}

					if(coords.x == pos.x + 1) {
						if(coords.y == pos.y || coords.y == pos.y - 1) {
							if(CheckHeightDifference(g, t, useHeight)) {
								temp.Add(g);
							}
						}
					}
				}
				else {
					if(coords.x == pos.x - 1) {
						if(coords.y == pos.y || coords.y == pos.y + 1) {
							if(CheckHeightDifference(g, t, useHeight)) {
								temp.Add(g);
							}
						}
					}

					if(coords.x == pos.x + 1) {
						if(coords.y == pos.y || coords.y == pos.y + 1) {
							if(CheckHeightDifference(g, t, useHeight)) {
								temp.Add(g);
							}
						}
					}
				}

				if(Mathf.Abs(coords.y - pos.y) == 1 && coords.x - pos.x == 0) {
					if(CheckHeightDifference(g, t, useHeight)) {
						temp.Add(g);
					}
				}
			}
		}
		return temp;
	}


	bool CheckHeightDifference(GameObject g1, GameObject g2, bool useHeight) {
		if(useHeight) {
			float distance = g1.transform.position.y - g2.transform.position.y;
			distance = Mathf.Abs(distance);
			if(distance >= maxHeightDifference) {
				return false;
			}
			return true;
		}
		else {
			return true;
		}
	}


	public void GridIndicatorClear() {
		foreach(var t in tiles) {
			t.GetComponent<MapTile>().ClearIndicator();
		}
	}

	
	void ColorPath() {

		// Clear all
		for(int i = 0; i < tiles.Count; i++) {
			tiles[i].GetComponent<MapTile>().Indicate(MapTile.indicators.none);
		}

		// Recolor final path
		for(int i = 0; i < pathFinal.Count; i++) {
			pathFinal[i].GetComponent<MapTile>().Indicate(MapTile.indicators.path);
		}

		if(!Global.system.playerMove) {
			// Recolor planned path
			for(int i = 0; i < pathPlanned.Count; i++) {
				pathPlanned[i].GetComponent<MapTile>().Indicate(MapTile.indicators.highlight);
			}
		}

		// Recolor player closest
		playerClosest.GetComponent<MapTile>().Indicate(MapTile.indicators.player);

		if(!Global.system.playerMove) {
			pathStart.GetComponent<MapTile>().Indicate(MapTile.indicators.start);
		}



		List<Transform> pathTemp = new List<Transform>();
		//pathTemp.Add(pathStart.transform);
		for(int fi = 0; fi < pathFinal.Count; fi++) {
			pathTemp.Add(pathFinal[fi].gameObject.transform);
		}
		for(int pi = 0; pi < pathPlanned.Count; pi++) {
			pathTemp.Add(pathPlanned[pi].gameObject.transform);
		}
		line.SetVertexCount(pathTemp.Count);
		for(int li = 1; li < pathTemp.Count; li++) {
			Vector3 pos = pathTemp[li].position;
			pos.y += playerOffset;
			line.SetPosition(li, pos);
		}
	}


	public void ResetPath() {
		GetComponent<MapMovement>().path.Clear();
		pathPlanned.Clear();
		pathFinal.Clear();
		pathStart = playerClosest.gameObject;
	}


	public void MovePlayer() {
		GetComponent<MapMovement>().SetupPath(pathFinal);
		Global.system.playerMove = true;

	}
	

	// TRAVELLING INFO
	void TravelNeed() {
		meatNeed = 0;
		plantsNeed = 0;
		waterNeed = 0;

		// Planned path
		for(int p = 0; p < pathPlanned.Count; p++) {
			currentArea = pathPlanned[p].GetComponent<MapTile>().areaType;
			foodMultiply = AreaResources(currentArea, true, false);
			waterMultiply = AreaResources(currentArea, true, false);
			TamerResources();
			PetResources();
		}

		// Final path
		for(int f = 0; f < pathFinal.Count; f++) {
			currentArea = pathFinal[f].GetComponent<MapTile>().areaType;
			foodMultiply = AreaResources(currentArea, true, false);
			waterMultiply = AreaResources(currentArea, true, false);
			TamerResources();
			PetResources();
		}

		// Time
		timeNeed = (1 + pathPlanned.Count + pathFinal.Count) / 2;
	}

	// GET RESOURCER FOR AREA
	public float AreaResources(Global.areaTypes type, bool food, bool familiar) {
		float value = 0.0f;
		for(int i = 0; i < Global.system.travel.Count; i++) {
			if(Global.system.travel[i].area == type) {
				if(food) {
					if(familiar) {
						value = Global.system.travel[i].foodFamiliar;
					}
					else {
						value = Global.system.travel[i].foodForeign;
					}
				}
				else {
					if(familiar) {
						value = Global.system.travel[i].waterFamiliar;
					}
					else {
						value = Global.system.travel[i].waterForeign;
					}
				}
			}
		}
		return value;
	}

	// RESOURCE USAGE: TAMER
	private void TamerResources() {
		if(Global.system.tamer.foodNeed % 2 == 0) {
			float foodNeed = (float)Global.system.tamer.foodNeed;
			meatNeed += (foodNeed / 2) * foodMultiply;
			plantsNeed += (foodNeed / 2) * foodMultiply;
		}
		else {
			float foodNeed = (float)Global.system.tamer.foodNeed - 1.0f;
			meatNeed += (foodNeed / 2) * foodMultiply;
			plantsNeed += ((foodNeed / 2) + 1) * foodMultiply;
		}
		waterNeed += Global.system.tamer.waterNeed * waterMultiply;
	}

	// RESOURCE USAGE: PETS
	private void PetResources() {
		if(Global.system.pets.Count > 0) {
			for(int i = 0; i < Global.system.pets.Count; i++) {
				if(Global.system.pets[i].herbivore) {
					plantsNeed += Global.system.pets[i].foodNeed * foodMultiply;
				}
				else if(Global.system.pets[i].carnivore) {
					meatNeed += Global.system.pets[i].foodNeed * foodMultiply;
				}
				else {
					if(Global.system.pets[i].foodNeed % 2 == 0) {
						meatNeed += (Global.system.pets[i].foodNeed / 2) * foodMultiply;
						plantsNeed += (Global.system.pets[i].foodNeed / 2) * foodMultiply;
					}
					else {
						float foodNeed = Global.system.pets[i].foodNeed - 1;
						meatNeed += (foodNeed / 2) * foodMultiply;
						plantsNeed += ((foodNeed / 2) + 1) * foodMultiply;
					}
				}
				waterNeed += Global.system.pets[i].waterNeed * waterMultiply;
			}
		}
	}




	// GET ENCOUNTER TILES
	public void EncounterTiles() {
		if(pathFinal.Count > 0 && pathPos < pathFinal.Count) {
			Vector2 coords = pathFinal[pathPos].GetComponent<MapTile>().coords;
			encounterTiles.Clear();
			List<GameObject> encounterTemp = GetEncounterTiles(detectionMax, coords);
			for(int i = 0; i < encounterTemp.Count; i++) {
				Vector3 heading = encounterTemp[i].transform.position - player.position;
				float dot = Vector3.Dot(heading, player.transform.forward);
				if(dot > 0.0f) {
					if(encounterTemp[i] != null && encounterTemp[i].GetComponent<MapTile>()) {
						encounterTiles.Add(encounterTemp[i]);
					}
				}
			}
		}
	}

	// FIND ALL ENCOUNTER TILES
	List<GameObject> GetEncounterTiles(int range, Vector2 coords) {
		foreach(GameObject t in tiles) {
			t.tag = "Untagged";
		}
		List<GameObject> toMark = new List<GameObject>();
		GameObject startTile = GetTile(coords);
		if(startTile == null) {
			return null;
		}
		startTile.transform.tag = "ToMark";
		//toMark.Add(startTile);
		for(int i = 0; i < range; i++) {
			toMark.Clear();
			toMark.Add(startTile);
			for(int t = 0; t < tiles.Count; t++) {
				if(tiles[t].transform.tag == "ToMark") {
					toMark.Add(tiles[t]);
				}
			}

			foreach(var t in toMark) {
				foreach(var a in Adjacent(t.GetComponent<MapTile>().coords, false)) {
					if(a.tag != "ToMark" && a.tag != "Marked") {
						a.tag = "ToMark";
					}
					t.tag = "Marked";
				}
			}

			//	//foreach(var t in toMark) {
				//	foreach(var a in Adjacent(tiles[t].GetComponent<MapTile>().coords, false)) {
				//		if(a.tag != "ToMark" && a.tag != "Marked") {
				//			a.tag = "ToMark";
				//		}
				//		tiles[t].tag = "Marked";
				//	}
			//}
		}

		List<GameObject> marked = new List<GameObject>();
		for(int m = 0; m < tiles.Count; m++) {
			if(tiles[m].transform.tag == "Marked") {
				marked.Add(tiles[m]);
			}
		}
		//List<GameObject> marked = GameObject.FindGameObjectsWithTag("Marked").ToList();
		foreach(var t in marked) {
			t.tag = "Untagged";
		}
		return marked;
	}


	// CREATE NEW ENCOUNTER
	public void CreateEncounter() {
		distanceTiles.Clear();

		//encounterTiles[i].GetComponent<MapTile>().Indicate(MapTile.indicators.dist1);

		for(int i = 0; i < encounterTiles.Count; i++) {
			float distance = Vector3.Distance(player.position, encounterTiles[i].transform.position);
			if(distance < 2) {
				distanceTiles.Add(new EncounterTile(encounterTiles[i], 1));
			}
			else if(distance < 4) {
				distanceTiles.Add(new EncounterTile(encounterTiles[i], 2));
			}
			else if(distance < 6) {
				distanceTiles.Add(new EncounterTile(encounterTiles[i], 3));
			}
			else if(distance < 8) {
				distanceTiles.Add(new EncounterTile(encounterTiles[i], 4));
			}
			else if(distance < 10) {
				distanceTiles.Add(new EncounterTile(encounterTiles[i], 5));
			}
		}

		//Sorting list and check it count
		if(distanceTiles.Count > 0) {
			distanceTiles.Sort(delegate(EncounterTile a, EncounterTile b) {
				return (a.distance).CompareTo(b.distance);
			});
		}

		// Create encounter
		int forceExit = distanceTiles.Count * 10;
		int tileCount = 0;
		int currentDistance = 1;
		while(tileCount < distanceTiles.Count) {

			// Check tiles that are at current distance
			for(int d = 0; d < distanceTiles.Count; d++) {
				if(distanceTiles[d].distance == currentDistance) {

					// Check whether tile is empty
					bool create = true;
					for(int e = 0; e < Global.system.encounters.Count; e++) {
						if(Global.system.encounters[e].coords == distanceTiles[d].tile.GetComponent<MapTile>().coords) {
							create = false;
						}
					}

					// If allowed to create
					if(create) {

						// Check area type
						for(int t = 0; t < Global.system.travel.Count; t++) {
							if(distanceTiles[d].tile.GetComponent<MapTile>().areaType == Global.system.travel[t].area) {

								// Detection distance
								if(distanceTiles[d].distance == Global.system.travel[t].detectionDistance) {
									if(pathPos < pathFinal.Count && pathPos > 0) {
										if(distanceTiles[d].tile.GetComponent<MapTile>().coords != pathFinal[pathPos].coords) {
												
											// Encounter chance
											float chance = Random.Range(0, 100.0f);
											if(chance <= Global.system.travel[t].encounterChance) {
												int count = Random.Range(0, 3);
												Vector3 pos = distanceTiles[d].tile.transform.position;
												pos.y += playerOffset;
												Global.areaTypes type = distanceTiles[d].tile.GetComponent<MapTile>().areaType;

												// If area is not city: create monster
												if(type != Global.areaTypes.city) {
													Vector2 coords = distanceTiles[d].tile.GetComponent<MapTile>().coords;
													Transform spawned = Instantiate(Global.system.MapEnemy(type, -1), pos, Quaternion.identity) as Transform;
													int id = spawned.GetComponent<Stats>().id;
													Encounter newEncounter = new Encounter(spawned, id, count, pos, coords);
													Global.system.encounters.Add(newEncounter);
												}
											}
										}
									}
								}
							}
						}
					}
					tileCount++;
				}
			}
			currentDistance++;

			// Force exit
			forceExit--;
			if(forceExit < 0) {
				Debug.LogWarning("Forced exit");
				break;
			}
		}
	}

	// RELOAD ENCOUNTERS
	void ReloadEncounters() {
		if(Global.system.encounters.Count > 0) {
			for(int i = 0; i < Global.system.encounters.Count; i++) {
				int id = Global.system.encounters[i].id;
				Vector3 pos = Global.system.encounters[i].position;
				Transform spawned = Instantiate(Global.system.MapEnemy(Global.areaTypes.plains, id), pos, Quaternion.identity) as Transform;
			}
		}
	}


	// CLEAR ENCOUNTERS
	public void ClearEncounters() {
		for(int i = 0; i < Global.system.encounters.Count; i++) {
			float distance = Vector3.Distance(player.position, Global.system.encounters[i].enemy.position);
			if(distance > destroyDistance) {
				Destroy(Global.system.encounters[i].enemy.gameObject);
				Global.system.encounters.RemoveAt(i);
			}
		}
	}

	// CHECK ENCOUNTES
	void CheckEncounters() {

		// Check if on path
		if(pathPos < pathFinal.Count - 1) {

			//print(pathPos + "/" + pathFinal.Count);

			if(pathFinal[pathPos + 1].GetComponent<MapTile>()) {
				for(int e = 0; e < Global.system.encounters.Count; e++) {
					Vector2 coordsPath = pathFinal[pathPos + 1].GetComponent<MapTile>().coords;
					Vector2 coordsEnemy = Global.system.encounters[e].coords;
					if(coordsPath == coordsEnemy) {
						Global.system.playerStop = true;
					}
				}
			}
		}

		// Initiate battle
		for (int i = 0; i < Global.system.encounters.Count; i++) {
			float distance = Vector3.Distance(player.position, Global.system.encounters[i].enemy.transform.position);
			if(distance < encounterDistance) {
				Global.system.playerStop = true;
				loadBattle = true;
				
				Global.system.currentEncounterId = Global.system.encounters[i].id;
				Global.system.currentEncounterCount = Global.system.encounters[i].count;

				Destroy(Global.system.encounters[i].enemy.gameObject);
				Global.system.encounters.RemoveAt(i);
				SceneManager.LoadScene("BattleScene"); 
			}
		}
	}
}

// ENCOUNTER TILES
[System.Serializable]
public class EncounterTile {
	public GameObject tile;
	public int distance;

	public EncounterTile(GameObject tile, int distance) {
		this.tile = tile;
		this.distance = distance;
	}
}




// ENCOUNTER ENEMIES
[System.Serializable]
public class Encounter {
	public Transform enemy;
	public int id;
	public int count;
	public Vector3 position;
	public Vector2 coords;
	public Vector2 home;

	public Encounter(Transform enemy, int id, int count, Vector3 position, Vector2 coords) {
		this.enemy = enemy;
		this.id = id;
		this.count = count;
		this.position = position;
		this.coords = coords;
		this.home = coords;
	}
}