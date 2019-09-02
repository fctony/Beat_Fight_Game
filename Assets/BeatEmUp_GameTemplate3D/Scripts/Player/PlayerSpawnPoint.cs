using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour {

	public GameObject defaultPlayerPrefab;

	void Awake(){

		//get selected player from character selection screen
		if(GlobalPlayerData.Player1Prefab) {
			loadPlayer(GlobalPlayerData.Player1Prefab);
			return;
		}	

		//otherwise load default character
		if(defaultPlayerPrefab) {
			loadPlayer(defaultPlayerPrefab);
		} else {
			Debug.Log("Please assign a default player prefab in the  playerSpawnPoint");
		}
	}

	//load a player prefab
	void loadPlayer(GameObject playerPrefab){
		GameObject player = GameObject.Instantiate(playerPrefab) as GameObject;
		player.transform.position = transform.position;
	}
}