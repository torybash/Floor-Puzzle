using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapLibrary : MonoBehaviour {

	[SerializeField] List<ScriptableObject> levelObjects;

	static MapLibrary I;

	void Awake(){
		if (I == null){
			I = this;
			DontDestroyOnLoad(gameObject);
		}else{
			GameObject.Destroy(gameObject);
		}
	}

	public ScriptableObject testLvlObject;

	public int GetLevelCount(){
		return levelObjects.Count;
	}


	public Map GetMap(int id){
		if (id == -1) return (Map) testLvlObject;
		return (Map) levelObjects[id];
	}
	
}
