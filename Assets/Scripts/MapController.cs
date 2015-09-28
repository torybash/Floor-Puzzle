using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class MapController : MonoBehaviour {

	[SerializeField] Transform wallPrefab;
	[SerializeField] Transform boxPrefab;

	[SerializeField] Renderer floorRend;

	Map currMap;

	Vector2 currMapOffset;


	List<Transform> wallTransforms;

	GameController gameCtrl;

	List<TileObject> tileObjects;
	

	TileLibrary tileLib;
	MapLibrary mapLib;

	void Awake(){
		gameCtrl = GetComponent<GameController>();
		tileLib = GameObject.FindGameObjectWithTag("Library").GetComponent<TileLibrary>();
		mapLib = GameObject.FindGameObjectWithTag("Library").GetComponent<MapLibrary>();

	}

	public void Init(){

	}


	public Map LoadMap(int id){
		if (id >= mapLib.GetLevelCount()) return null;

//		Map map = new Map ();
//		Stream stream = File.Open(levels[id]. , FileMode.Open);
//		BinaryFormatter bformatter = new BinaryFormatter();
//		bformatter.Binder = new VersionDeserializationBinder(); 
//		Debug.Log ("Reading Data");
//		data = (SaveData)bformatter.Deserialize(stream);
//		stream.Close();

		
		Map map = mapLib.GetMap(id);;
		map.ReadTileList();
		print ("loaded map - map: "+ map + ", name: " + map.title + ", width: "+ map.width + ", height: " + map.height);
//		string mapPath = Application.persistentDataPath + "/" + levelsReferences[id] + ".dat" ;
//		MemoryStream memStream = new MemoryStream();
//		byte[] bytes = System.Convert.FromBase64String(levels[id].text);
//		memStream.Write(bytes, 0, bytes.Length);     
//		BinaryFormatter bformatter = new BinaryFormatter();
//		bformatter.Binder = new VersionDeserializationBinder();
//		Debug.Log ("Reading Data");
//		map = (Map)bformatter.Deserialize(memStream);
//		memStream.Close();

//		if (File.Exists(mapPath)){
//			BinaryFormatter bf = new BinaryFormatter();
//			FileStream file = File.Open(Application.persistentDataPath + "/FileName.dat", FileMode.Open);
//			map = (Map)bf.Deserialize(file);
//			file.Close();
//		}

		return map;
	}


	public void InitMap(Map map){

		//Destroy existing objects
		if (tileObjects != null){
			foreach (var item in tileObjects) {
				GameObject.Destroy(item.boxObject.gameObject);
			}
			tileObjects.Clear();
		}
		//Load map
		//TEST MAP
//		map.width = 7;
//		map.height = 8;
//		map.map = new Tile[map.width,map.height];
//		for (int x = 0; x < map.width; x++) {
//			for (int y = 0; y < map.height; y++) {
//				map.map[x,y] = new Tile(TileType.EMPTY);
//
//				if (x == 0 || x == map.width - 1 || y == 0 || y == map.height - 1) map.map[x,y] = new Tile(TileType.WALL);
//			}
//		}
//		map.map[1,4] = new Tile(TileType.WALL);
//		map.map[4,2] = new Tile(TileType.WALL);
//		map.map[5,5] = new Tile(TileType.WALL);
//
//		map.map[2,3] = new Tile(TileType.BOX);
//        map.map[5,4] = new Tile(TileType.BOX);
		//TEST MAP
		currMap = map;

		currMapOffset = new Vector2(0.5f, 0.5f);

		//Create objects
		wallTransforms = new List<Transform>();
		tileObjects = new List<TileObject>();
		for (int x = 0; x < map.width; x++) {
			for (int y = 0; y < map.height; y++) {
				Tile tile = map.map[x,y];
				if (tile.type == TileType.EMPTY) continue;

				Vector2 pos = new Vector2(x, y) + currMapOffset;

				Transform prefab = tileLib.GetTilePrefab(tile.type);
				Transform objT = (Transform) Instantiate(prefab, pos, Quaternion.identity);

				TileObject to = objT.gameObject.AddComponent<TileObject>();
				to.Init(objT, tile.type, tile.keyType, tile.direction, x, y);
				tileObjects.Add (to);

				//Should be painted?
				if (tile.type == TileType.DOOR || tile.type == TileType.KEY){
					objT.GetComponent<SpriteRenderer>().color = tileLib.GetTileColor(tile.keyType);
				}
				
				//Rotate
				objT.rotation = Quaternion.AngleAxis(90f * (int) tile.direction, Vector3.forward);

			}
		}

		floorRend.transform.localScale = new Vector3(map.width, map.height, 1);
		floorRend.transform.localPosition = new Vector3(map.width / 2f, map.height / 2f, 0.1f);
		floorRend.material.SetTextureScale("_MainTex", new Vector2(map.width, map.height));
		gameCtrl.camCtrl.MoveTo(new Vector2(map.width/2f, map.height/2f));
	}



	public void MoveFloor(Vector2 input){
		StartCoroutine(MoveFloorRoutine(input));
	}

	private IEnumerator MoveFloorRoutine(Vector2 input){

		//Find boxes that can move and them to list
		List<TileObject> boxesToMove = new List<TileObject>();
		foreach (TileObject obj in tileObjects) {
			if (!obj.IsMoveable()) continue;
			if (currMap.CanMoveInDirection(obj, input)){
				boxesToMove.Add(obj);
			}
		}

		float duration = 0.4f;
		float time = 0;
		Vector2 floorTexOffset = new Vector2();
		while (time < duration){

			float frac = time/duration;

			//Scroll floor
			floorTexOffset = -input * frac;
			floorRend.material.SetTextureOffset("_MainTex", floorTexOffset);

			//Move boxes
			foreach (TileObject box in boxesToMove) {
				Vector2 newPos = new Vector2(box.x + frac * input.x, box.y + frac * input.y) + currMapOffset;
				box.boxObject.position = newPos;
			}


			time += Time.deltaTime;
			yield return null;
		}

		//Set floor exact
		floorRend.material.SetTextureOffset("_MainTex", new Vector2());

		//Set moved boxes exact
		foreach (TileObject box in boxesToMove) {
			Vector2 newPos = new Vector2(box.x + 1 * input.x, box.y + 1 * input.y) + currMapOffset;
			box.boxObject.position = newPos;
		}

		FloorMoved(input, boxesToMove);
			                                

	}

	private void FloorMoved(Vector2 input, List<TileObject> objsMoved){
		//Update map.map
		foreach (TileObject obj in objsMoved) {
			currMap.map[obj.x, obj.y] = new Tile(TileType.EMPTY);
		}
		foreach (TileObject obj in objsMoved) {
			obj.x += (int)input.x;
			obj.y += (int)input.y;
			currMap.map[obj.x, obj.y] = new Tile(obj.type, obj.dir, obj.keyType);
		}

		//Reset game state
		gameCtrl.FloorMoved();

		TileObject playerObj = null;
		List<TileObject> exitObjs = new List<TileObject>();
		List<TileObject> keyObjs = new List<TileObject>();
		List<TileObject> doorObjs = new List<TileObject>();
		foreach (TileObject to in tileObjects) {
			if (to.type == TileType.PLAYER) playerObj = to;
			else if (to.type == TileType.EXIT) exitObjs.Add(to);
			else if (to.type == TileType.KEY) keyObjs.Add(to);
			else if (to.type == TileType.DOOR) doorObjs.Add(to);
		}

		//Open doors
		List<TileObject> destroyedObjects = new List<TileObject>();
		foreach (TileObject key in keyObjs) {
			foreach (TileObject door in doorObjs) {
				if (key.x == door.x && key.y == door.y){
					GameObject.Destroy(key.gameObject);
					GameObject.Destroy(door.gameObject);
					destroyedObjects.Add(key);
					destroyedObjects.Add(door);
					currMap.map[key.x, key.y] = new Tile(TileType.EMPTY);
				}
			}
		}
		foreach (var item in destroyedObjects) tileObjects.Remove(item);

		//Check for victory
		print ("has won? - player pos: " + playerObj.x + ", " +playerObj.y); 
		if (playerObj == null) return;
		foreach (TileObject to in exitObjs) {
			print ("has won? - exit pos: " + to.x + ", " +to.y);
			if (playerObj.x == to.x && playerObj.y == to.y){
				gameCtrl.WonLevel();
				break;
			}
		}
	}


	


	public void CheckTiles(){
		foreach (TileObject item in tileObjects) {
			//is x, y same as pos.x,y?
			if (item.x != (int) item.boxObject.transform.position.x || item.y != (int) item.boxObject.transform.position.y){
				print ("CheckTiles - X AND Y NOT SAME AS POS.X, Y - x, y: " + item.x + ", " + item.y + ", trans.pos: " + item.boxObject.transform.position);
			}

			//is type same as map
			if (currMap.map[item.x, item.y].type != item.type){
				print("CheckTiles - TYPE NOT ON CURRMAP - type: " + item.type + ", map type: "+ currMap.map[item.x, item.y].type);
			}
		}
	}
}




[System.Serializable]
public enum TileType{
	EMPTY = 0,
	WALL = 1,
	BOX = 2,
	KEY = 3,
	DOOR = 4,
	BUTTON = 5,
	EXIT = 6,
	PLAYER = 7,
	LASER = 8,
	WATER = 9,
	AMOUNT = 10
	
}

[System.Serializable]
public enum TileDirection{
	UP = 0,
	RIGHT = 1,
	DOWN = 2,
	LEFT = 3,
	AMOUNT = 4
}

[System.Serializable]
public enum KeyType{
	RED = 0,
	GREEN = 1,
	BLUE = 2,
	AMOUNT = 3
}

[System.Serializable]
public class Tile {
	public int tileID;
	public TileType type;
	public TileDirection direction;
	public KeyType keyType;
	public List<int> buttonConnections;

	public Tile(TileType type, TileDirection direction = TileDirection.UP, KeyType keyType = KeyType.RED, List<int> buttonConnections = null){
		this.type = type;
		this.direction = direction;
		this.keyType = keyType;
		this.buttonConnections = buttonConnections;
	}

	public Tile Copy(){
		Tile copy = new Tile(type, direction, keyType);
		if (buttonConnections != null){
			copy.buttonConnections = new List<int>();
			foreach (int item in buttonConnections) {
				copy.buttonConnections.Add(item);
			}
		}
		return copy;
	}
}


public class TileObject : MonoBehaviour{
	public Transform boxObject;
	public TileType type;
	public KeyType keyType;
	public TileDirection dir;
	public int x, y;

//	public TileObject(Transform boxObject, TileType type, int x, int y){
//		this.boxObject = boxObject;
//		this.type = type;
//		this.x = x;
//		this.y = y;
//	}

	public void Init(Transform boxObject, TileType type, KeyType keyType, TileDirection dir, int x, int y){
		this.boxObject = boxObject;
		this.type = type;
		this.keyType = keyType;
		this.x = x;
		this.y = y;
	}

	public bool IsMoveable(){
		if (type == TileType.BOX || type == TileType.KEY || type == TileType.LASER || type == TileType.PLAYER) return true;
		return false;
	}
}

