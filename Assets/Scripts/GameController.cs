using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {

	public MapController mapCtrl;
	public CameraController camCtrl;

	State state;

	[SerializeField] Text levelNameText;

	void Awake(){
		mapCtrl = GetComponent<MapController>();
		camCtrl = GetComponent<CameraController>();
		mapCtrl.Init ();
	}


	void Start(){
		StartLevel();
	}


	void Update(){

		mapCtrl.CheckTiles();


		switch (state) {
		case State.NORMAL:
			MoveFloor();
			break;
		default:
			break;
		}
		
	}

	private void MoveFloor(){
		Vector2 input = GetInput();
		if (input.magnitude == 1){
			mapCtrl.MoveFloor(input);
			state = State.MOVING;
		}
	}


	bool buttonDown = false;
	Vector2 clickPos;
	private Vector2 GetInput(){
		Vector2 input = Vector2.zero;

		if (SystemInfo.deviceType == DeviceType.Desktop){
			input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		}else{

			if (!buttonDown && Input.GetMouseButtonDown(0)){
				buttonDown = true;
				clickPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

				print ("Down mouse - clickPos: " + clickPos);
			}else if (buttonDown && Input.GetMouseButtonUp(0)){
				Vector2 dir = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - clickPos;

				if (dir.magnitude > 1){
					if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)){
						input.x = dir.x;
					}else{
						input.y = dir.y;
					}
					input.Normalize();

				}
				buttonDown = false;

				print ("UP click - clickPos: " + (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + ", dir: " + dir);
			}
		}

		return input;
	}


	public void FloorMoved(){
		state = State.NORMAL;

		//TODO CHECK FOR VICTORY?

	}


	public void StartLevel(){
		int levelID = GameManager.I.levelToPlay;
		state = State.NORMAL;
		Map map = mapCtrl.LoadMap(levelID);
		mapCtrl.InitMap(map);

		levelNameText.text = "" + (levelID + 1) + ". " + map.title;
	}


	public void WonLevel(){
		if (GameManager.I.levelToPlay == -1){ //was testing lvl
			Application.LoadLevel("MapMaker");
		}else{
			print ("WON");
			GameManager.I.levelToPlay += 1;
			GameManager.I.levelReached = GameManager.I.levelToPlay;

			StartLevel();
		}
	}


	public void MenuClicked(){
		if (GameManager.I.levelToPlay == -1){ //was testing lvl
			Application.LoadLevel("MapMaker");
		}else{
			Application.LoadLevel("Menu");
		}
	}
}


public enum State{
	NORMAL = 0,
	MOVING = 1,
	GAME_MENU = 2
}