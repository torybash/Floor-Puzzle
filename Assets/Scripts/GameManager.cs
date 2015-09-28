using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager I;

	public int levelToPlay;

	public int levelReached;

	void Awake(){
		if (I == null){
			I = this;
			DontDestroyOnLoad(gameObject);
		}else{
			GameObject.Destroy(gameObject);
		}
	}
}
