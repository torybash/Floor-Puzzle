using UnityEngine;
using System.Collections;

public class StartFromMenu : MonoBehaviour {

	void Awake(){
		print ("GameManager.I: " + GameManager.I);
		if (GameManager.I == null) Application.LoadLevel("Menu");
	}
}
