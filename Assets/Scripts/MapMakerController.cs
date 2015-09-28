#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

public class MapMakerController : MonoBehaviour {


	[SerializeField] Renderer floorRend;
	CameraController camCtrl;

	[SerializeField] CanvasRenderer newMapPanel;
	[SerializeField] InputField newMapNameInput;
	[SerializeField] InputField newMapWidthInput;
	[SerializeField] InputField newMapHeightInput;

	Map currMap;

	Tile currTileBrush;

	GameObject[,] currObjects;

	TileLibrary tileLib;

	Action act;

	string folderPath = "Assets/Maps/";

	MapLibrary mapLib;



	void Awake(){
		tileLib = GameObject.FindGameObjectWithTag("Library").GetComponent<TileLibrary>();
		camCtrl = GetComponent<CameraController>();

		act = MenuUpdate;

		mapLib = GameObject.FindGameObjectWithTag("Library").GetComponent<MapLibrary>();
	}


	void Update(){
		act();
	}

	void Start(){
		if (GameManager.I.levelToPlay == -1){
			string path = folderPath + "testmap.asset";

			UnityEngine.Object outputMapAsset = AssetDatabase.LoadMainAssetAtPath (path);
//			if (outputMapAsset != null){
//				EditorUtility.CopySerialized(outputMapAsset, currMap);
//			}else{
//				outputMapAsset = ScriptableObject.CreateInstance<Map>();
//			}
			print ("outputMapAsset: " + outputMapAsset + ", path: " + path);
//			Map loadedMap = ScriptableObject.CreateInstance<Map>(); // = (Map) outputMapAsset;
//			EditorUtility.CopySerialized(loadedMap, outputMapAsset);

			currMap = ScriptableObject.CreateInstance<Map>();
			EditorUtility.CopySerialized(outputMapAsset, currMap);
			
			currMap.ReadTileList();
			
			currObjects = new GameObject[currMap.width,currMap.height];
			CreateMapObjects();
			SetFloorAndCamera();
			act = MapUpdate;
		}
	}

	private void MenuUpdate(){

	}

	private void MapUpdate(){
		BrushSwitching();
		Painting();
	}

	private void BrushSwitching(){
		int numberPressed = -1;
		if (Input.GetKeyDown(KeyCode.Alpha1)){
			numberPressed = 1;
		}else if (Input.GetKeyDown(KeyCode.Alpha2)){
			numberPressed = 2;
		}else if (Input.GetKeyDown(KeyCode.Alpha3)){
			numberPressed = 3;
		}else if (Input.GetKeyDown(KeyCode.Alpha4)){
			numberPressed = 4;
		}else if (Input.GetKeyDown(KeyCode.Alpha5)){
			numberPressed = 5;
		}else if (Input.GetKeyDown(KeyCode.Alpha6)){
			numberPressed = 6;
		}else if (Input.GetKeyDown(KeyCode.Alpha7)){
			numberPressed = 7;
		}else if (Input.GetKeyDown(KeyCode.Alpha8)){
			numberPressed = 8;
		}else if (Input.GetKeyDown(KeyCode.Alpha9)){
			numberPressed = 9;
		}else if (Input.GetKeyDown(KeyCode.Alpha0)){
			numberPressed = 0;
		}

		if (numberPressed >= 0){
			TileType type = (TileType) numberPressed;
			currTileBrush = new Tile(type);
		}

		if (Input.GetKeyDown(KeyCode.Q)){
			Rotate(true);
		}else if (Input.GetKeyDown(KeyCode.E)){
			Rotate(false);
		}

		if (Input.GetKeyDown(KeyCode.R)){
			currTileBrush.keyType = KeyType.RED;
		}else if (Input.GetKeyDown(KeyCode.G)){
			currTileBrush.keyType = KeyType.GREEN;
		}else if (Input.GetKeyDown(KeyCode.B)){
			currTileBrush.keyType = KeyType.BLUE;
		}
	}

	public void Rotate(bool clockwise){
		if (currTileBrush == null) return;
		if (clockwise){
			currTileBrush.direction = (TileDirection)(((int)currTileBrush.direction + 1) % ((int)TileDirection.AMOUNT));
		}else{
			int newDir = (int)currTileBrush.direction - 1;
			if (newDir < 0) newDir = (int)TileDirection.AMOUNT - 1;
			currTileBrush.direction = (TileDirection)newDir;
		}
	}


	private void Painting(){

		if (Input.GetMouseButton(0) && currTileBrush != null){
			//Is inside map bounds?
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			if (mousePos.x <  0 || mousePos.x > currMap.width || mousePos.y < 0 || mousePos.y > currMap.height) return;
			int x = (int) mousePos.x; int y = (int) mousePos.y;

			//Change map
			currMap.map[x, y] = currTileBrush.Copy();

			//Update objects
			UpdateObjectsAt(x, y, currTileBrush.type, currTileBrush.keyType, currTileBrush.direction);


		}
	}

	private void UpdateObjectsAt(int x, int y, TileType type, KeyType keyType, TileDirection dir){
		if (currObjects[x,y] != null) GameObject.Destroy(currObjects[x,y]);
		if (type == TileType.EMPTY) return; //works as eraser
		Transform newObjT = (Transform) Instantiate(tileLib.GetTilePrefab(type), TileLibrary.GetTilePos(new Vector2(x, y)), Quaternion.identity);
		currObjects[x,y] = newObjT.gameObject;

		//Should be painted?
		if (type == TileType.DOOR || type == TileType.KEY){
			newObjT.GetComponent<SpriteRenderer>().color = tileLib.GetTileColor(keyType);
		}
		
		//Rotate
		newObjT.rotation = Quaternion.AngleAxis(90f * (int) dir, Vector3.forward);
	}

	public void CreateMapObjects(){
		for (int x = 0; x < currMap.width; x++) {
			for (int y = 0; y < currMap.height; y++) {
				Tile tile = currMap.map[x,y];
				if (tile.type == TileType.EMPTY) continue;
				UpdateObjectsAt(x, y, tile.type, tile.keyType, tile.direction);
			}
		}
	}

	public void OpenNewMapPanel(){
		newMapPanel.gameObject.SetActive(true);

	}


	private void CleanMap(){
		if (currMap != null){
			for (int x = 0; x < currMap.width; x++) {
				for (int y = 0; y < currMap.height; y++) {
					GameObject.Destroy(currObjects[x,y]);
				}
			}
			GameObject.Destroy(currMap);
		}
	}

	public void CreateNewMap(){

		//Clean last map
		CleanMap();


//		currMap = new Map();
		currMap = ScriptableObject.CreateInstance<Map>();
		currMap.title = newMapNameInput.text;
		currMap.width = int.Parse(newMapWidthInput.text);
		currMap.height = int.Parse(newMapHeightInput.text);
		currMap.map = new Tile[currMap.width,currMap.height];

		currObjects = new GameObject[currMap.width,currMap.height];

		newMapPanel.gameObject.SetActive(false);

		act = MapUpdate;
		SetFloorAndCamera();

	}


	private void SetFloorAndCamera(){
		floorRend.transform.localScale = new Vector3(currMap.width, currMap.height, 1);
		floorRend.transform.localPosition = new Vector3(currMap.width / 2f, currMap.height / 2f, 0);
		floorRend.material.SetTextureScale("_MainTex", new Vector2(currMap.width, currMap.height));
		camCtrl.MoveTo(new Vector2(currMap.width/2f, currMap.height/2f));
	}

	public void SaveMap(){
//		Stream stream = File.Open(Application.persistentDataPath + "/" + currMap.name + ".map", FileMode.Create);
//		BinaryFormatter bformatter = new BinaryFormatter();
//		bformatter.Binder = new VersionDeserializationBinder(); 
//		Debug.Log ("Writing Information");
//		bformatter.Serialize(stream, currMap);
//		stream.Close();

		currMap.MakeTileList();

//		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (currMap.name + ".asset");

		string path = folderPath + currMap.title + ".asset";
		//does already exists?
		UnityEngine.Object outputMapAsset = AssetDatabase.LoadMainAssetAtPath (path);
		if (outputMapAsset != null){
			EditorUtility.CopySerialized(currMap, outputMapAsset);
		}else{
			outputMapAsset = ScriptableObject.CreateInstance<Map>();
			EditorUtility.CopySerialized(currMap, outputMapAsset);
			AssetDatabase.CreateAsset(outputMapAsset, path);
		}

		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh();
//		EditorUtility.FocusProjectWindow ();
	}



	public void LoadMap(){

		if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(Map)){
			CleanMap();

			Map loadedMap = (Map) Selection.activeObject;

			currMap = ScriptableObject.CreateInstance<Map>();
			EditorUtility.CopySerialized(loadedMap, currMap);

			currMap.ReadTileList();

			currObjects = new GameObject[currMap.width,currMap.height];
			CreateMapObjects();
			SetFloorAndCamera();
			act = MapUpdate;
		}else{
			Debug.LogError("LoadMap failed - selection not allowed");
		}
	}



	public void TestMap(){
		GameManager.I.levelToPlay = -1;

		currMap.MakeTileList();

		mapLib.testLvlObject = currMap;


		
		//		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (currMap.name + ".asset");
		
		string path = folderPath + "testmap.asset";
		//does already exists?
		UnityEngine.Object outputMapAsset = AssetDatabase.LoadMainAssetAtPath (path);
		if (outputMapAsset != null){
			EditorUtility.CopySerialized(currMap, outputMapAsset);
		}else{
			outputMapAsset = ScriptableObject.CreateInstance<Map>();
			EditorUtility.CopySerialized(currMap, outputMapAsset);
			AssetDatabase.CreateAsset(outputMapAsset, path);
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh();
		//		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (currMap.name + ".asset");


		Application.LoadLevel("Main");
	}
}



public sealed class VersionDeserializationBinder : SerializationBinder 
{ 
	public override Type BindToType( string assemblyName, string typeName )
	{ 
		if ( !string.IsNullOrEmpty( assemblyName ) && !string.IsNullOrEmpty( typeName ) ) 
		{ 
			Type typeToDeserialize = null; 
			
			
			assemblyName = Assembly.GetExecutingAssembly().FullName; 
			// The following line of code returns the type. 
			typeToDeserialize = Type.GetType( String.Format( "{0}, {1}", typeName, assemblyName ) ); 
			return typeToDeserialize; 
		} 
		return null; 
	} 
	
}


#endif
