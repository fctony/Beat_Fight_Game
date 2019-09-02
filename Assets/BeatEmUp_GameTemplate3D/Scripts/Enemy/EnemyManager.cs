using UnityEngine;
using System.Collections.Generic;

public static class EnemyManager {

	public static List<GameObject> enemyList = new List<GameObject>(); //the total list of enemies in the current level
	public static List<GameObject> enemiesAttackingPlayer = new List<GameObject>(); //a list of enemies that are currently attacking
	public static List<GameObject> activeEnemies = new List<GameObject>(); //a list of enemies that are currently active

	//Removes an enemy from the enemy list
	public static void RemoveEnemyFromList( GameObject g ){
		enemyList.Remove(g);
		SetEnemyTactics();
	}


	//Sets the tactics for each enemy in the current wave
	public static void SetEnemyTactics(){
		getActiveEnemies();
		if(activeEnemies.Count > 0){
			for(int i=0; i<activeEnemies.Count; i++){
				if(i < MaxEnemyAttacking()){
					activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.ENGAGE);
				} else {
					activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.KEEPMEDIUMDISTANCE);
				}
			}
		}
	}

	//Force all enemies to use a certain tactic
	public static void ForceEnemyTactic(ENEMYTACTIC tactic){
		getActiveEnemies();
		if(activeEnemies.Count > 0){
			for(int i=0; i<activeEnemies.Count; i++){
				activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(tactic);
			}
		}
	}

	//Disables all enemy AI's
	public static void DisableAllEnemyAIs(){
		getActiveEnemies();
		if(activeEnemies.Count > 0){
			for(int i=0; i<activeEnemies.Count; i++){
				activeEnemies [i].GetComponent<EnemyAI> ().enableAI = false;
			}
		}
	}

	//Returns a list of enemies that are currently active
	public static void getActiveEnemies(){
		activeEnemies.Clear();
		foreach(GameObject enemy in enemyList){
			if(enemy != null && enemy.activeSelf)activeEnemies.Add(enemy);
		}
	}

	//Player has died
	public static void PlayerHasDied(){
		DisableAllEnemyAIs();
		enemyList.Clear();
	}

	//Returns the maximum number of enemies that can attack the player at once (Tools/GameSettings)
	static int MaxEnemyAttacking(){
		EnemyWaveSystem EWS = GameObject.FindObjectOfType<EnemyWaveSystem>();
		if(EWS != null) return EWS.MaxAttackers;
		return 3;
	}

	//Set an enemy to agressive state
	public static void setAgressive(GameObject enemy){
		enemy.GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.ENGAGE);

		//set another enemy to passive state
		foreach(GameObject e in activeEnemies){
			if (e != enemy) {
				e.GetComponent<EnemyAI>().SetEnemyTactic (ENEMYTACTIC.KEEPMEDIUMDISTANCE);
				return;
			}
		}
	}
}
