using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour {

	[Header("UI refs")]
	[SerializeField] Button continueButton;
	[SerializeField] RectTransform layoutGridLevels;
	[SerializeField] RectTransform mainPanel;
	[SerializeField] RectTransform levelsPanel;


	[Header("Prefabs")]
	[SerializeField] RectTransform levelButtonPrefab;



	MapLibrary mapLib;

	void Awake(){
		mapLib = GameObject.FindGameObjectWithTag("Library").GetComponent<MapLibrary>();

		GoToMainMenu();
	}

	void Start(){
		//TODO load game and
		//IF NO LOAD
		continueButton.GetComponent<Text>().text = "Start";

		//Init level selection
		for (int i = 0; i < mapLib.GetLevelCount(); i++) {
			Transform buttonT = Instantiate(levelButtonPrefab);
			buttonT.SetParent(layoutGridLevels);
			int id = i;
			buttonT.GetComponent<Button>().onClick.AddListener(() => StartLevelClicked(id));

			foreach (Transform child in buttonT) {
				if (child.name.Equals("Text")){
					child.GetComponent<Text>().text = "" + (id + 1);
				}
			}

//			if (buttonT.GetComponentInChildren<Text>() == null) continue;
//			buttonT.GetComponentInChildren<Text>().text = "" + (id + 1);
		}

	}

	public void ContinueClicked(){
		GameManager.I.levelToPlay = GameManager.I.levelReached;
		
		Application.LoadLevel("Main");
	}

	public void LevelsClicked(){
		mainPanel.gameObject.SetActive(false);
		levelsPanel.gameObject.SetActive(true);
	}

	public void SettingsClicked(){

	}


	public void StartLevelClicked(int id){
		GameManager.I.levelToPlay = id;

		Application.LoadLevel("Main");

	}

	public void GoToMainMenu(){
		mainPanel.gameObject.SetActive(true);
		levelsPanel.gameObject.SetActive(false);
	}
	
}
