using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLibrary : MonoBehaviour {

	[SerializeField] List<TilePrefab> tilePrefabs;
	private Dictionary<TileType, Transform> tilePrefabsDict = new Dictionary<TileType, Transform>();


	private Dictionary<KeyType, Color> keyColorDict = new Dictionary<KeyType, Color>();

	static TileLibrary I;

	void Awake(){

		if (I == null){
			I = this;
			DontDestroyOnLoad(gameObject);
		}else{
			GameObject.Destroy(gameObject);
		}


		foreach (var item in tilePrefabs) {
			tilePrefabsDict.Add(item.type, item.prefab);
		}

		keyColorDict.Add(KeyType.RED, Color.red);
		keyColorDict.Add(KeyType.GREEN, Color.green);
		keyColorDict.Add(KeyType.BLUE, Color.blue);
	}

	public Transform GetTilePrefab(TileType type){
		return tilePrefabsDict[type];
	}


	public Color GetTileColor(KeyType keyType){
		return keyColorDict[keyType];
	}


	public static Vector2 GetTilePos(Vector2 pos){
		Vector2 tilePos = pos;
		tilePos.x = (int) tilePos.x + 0.5f;
		tilePos.y = (int) tilePos.y + 0.5f;

		return tilePos;
	}
}

[System.Serializable]
public class TilePrefab{
	public TileType type;
	public Transform prefab;
}