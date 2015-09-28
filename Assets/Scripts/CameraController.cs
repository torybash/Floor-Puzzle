using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	Transform camTrans;

	void Awake () {
		camTrans = Camera.main.transform;
	}



	public void MoveTo(Vector2 pos){
		Vector3 newPos = (Vector3) pos;
		newPos.z = -10;
		camTrans.position = newPos;
	}
}
