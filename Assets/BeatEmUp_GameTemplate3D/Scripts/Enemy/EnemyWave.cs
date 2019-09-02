using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyWave {
	
	public string WaveName = "Wave";
	public BoxCollider AreaCollider; //a collider that keeps the player from leaving an area
	public List<GameObject> EnemyList = new List<GameObject> ();

	public bool waveComplete() {
		return EnemyList.Count == 0;
	}

	public void RemoveEnemyFromWave(GameObject g) {
		if (EnemyList.Contains(g)) {
			EnemyList.Remove(g);
		}
	}
}