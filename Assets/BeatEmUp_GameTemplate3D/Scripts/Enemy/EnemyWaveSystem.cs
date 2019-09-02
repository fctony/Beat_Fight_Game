using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyWaveSystem : MonoBehaviour {

	public int MaxAttackers = 3; //the maximum number of enemies that can attack the player simultaneously 

	[Header ("List of enemy Waves")]
	public EnemyWave[] EnemyWaves;
	public int currentWave;

	[Header ("Slow Motion Settings")]
	public bool activateSlowMotionOnLastHit;
	public float effectDuration = 1.5f;

	[Header ("Load level On Finish")]
	public bool loadNewLevel;
	public string levelName;

	void OnEnable(){
		EnemyActions.OnUnitDestroy += onUnitDestroy;
	}

	void OnDisable(){
		EnemyActions.OnUnitDestroy -= onUnitDestroy;
	}

	void Awake(){
		if (enabled) DisableAllEnemies();
	}

	void Start(){
		currentWave = 0;
		UpdateAreaColliders();
		StartNewWave();
	}

	//Disable all the enemies
	void DisableAllEnemies(){
		foreach(EnemyWave wave in EnemyWaves){
			for(int i=0; i<wave.EnemyList.Count; i++){
				if (wave.EnemyList[i] != null){

					//deactivate enemy
					wave.EnemyList[i].SetActive(false);
				} else {
					
					//remove empty fields from the list
					wave.EnemyList.RemoveAt(i);
				}
			}
			foreach(GameObject g in wave.EnemyList){
				if (g != null) g.SetActive(false);
			}
		}
	}
		
	//Start a new enemy wave
	public void StartNewWave(){

		//hide UI hand pointer
		HandPointer hp = GameObject.FindObjectOfType<HandPointer>();
		if (hp != null)	hp.DeActivateHandPointer ();

		//activate enemies
		foreach (GameObject g in EnemyWaves[currentWave].EnemyList) {
			if(g!=null)	g.SetActive (true);
		}
		Invoke("SetEnemyTactics", .1f);
	}

	//Update Area Colliders
	void UpdateAreaColliders(){

		//switch current area collider to a trigger
		if (currentWave > 0) {
			BoxCollider areaCollider = EnemyWaves [currentWave - 1].AreaCollider;
			if (areaCollider != null) {
				areaCollider.enabled = true;
				areaCollider.isTrigger = true;
				AreaColliderTrigger act = areaCollider.gameObject.AddComponent<AreaColliderTrigger> ();
				act.EnemyWaveSystem = this;
			}
		}

		//set next collider as camera area restrictor
		if(EnemyWaves[currentWave].AreaCollider != null) { 
			EnemyWaves[currentWave].AreaCollider.gameObject.SetActive(true);
		}

		CameraFollow cf = GameObject.FindObjectOfType<CameraFollow>();
		if (cf != null)	cf.CurrentAreaCollider = EnemyWaves [currentWave].AreaCollider;

		//show UI hand pointer
		HandPointer hp = GameObject.FindObjectOfType<HandPointer>();
		if (hp != null)	hp.ActivateHandPointer ();
	}

	//An enemy has been destroyed
	void onUnitDestroy(	GameObject g){
		if(EnemyWaves.Length > currentWave){
			EnemyWaves[currentWave].RemoveEnemyFromWave(g);
			if(EnemyWaves[currentWave].waveComplete()){
				currentWave += 1;
				if(!allWavesCompleted()){ 
					UpdateAreaColliders();
				} else{
					StartCoroutine (LevelComplete());
				}
			}
		}
	}

	//True if all the waves are completed
	bool allWavesCompleted(){
		int waveCount = EnemyWaves.Length;
		int waveFinished = 0;

		for(int i=0; i<waveCount; i++){
			if(EnemyWaves[i].waveComplete()) waveFinished += 1;
		}

		if(waveCount == waveFinished) 
			return true;
		else 
			return false;
	}

	//Update enemy tactics
	void SetEnemyTactics(){
		EnemyManager.SetEnemyTactics();
	}

	//Level complete
	IEnumerator LevelComplete(){

		//activate slow motion effect
		if (activateSlowMotionOnLastHit) {
			CamSlowMotionDelay cmd = Camera.main.GetComponent<CamSlowMotionDelay>();
			if (cmd != null) {
				cmd.StartSlowMotionDelay (effectDuration);
				yield return new WaitForSeconds (effectDuration);
			}
		}

		//Timeout before continuing
		yield return new WaitForSeconds (1f);

		//Fade to black
		UIManager UI = GameObject.FindObjectOfType<UIManager>();
		if (UI != null) {
			UI.UI_fader.Fade (UIFader.FADE.FadeOut, 2f, 0);
			yield return new WaitForSeconds (2f);
		}

		//Disable players
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject p in players) {
			Destroy(p);
		}

		//Go to next level or show GAMEOVER screen
		if (loadNewLevel) {
			if (levelName != "")
				SceneManager.LoadScene (levelName);

		} else {

			//Show game over screen
			if (UI != null) {
				UI.DisableAllScreens ();
				UI.ShowMenu ("LevelComplete");
			}
		}
	}
}