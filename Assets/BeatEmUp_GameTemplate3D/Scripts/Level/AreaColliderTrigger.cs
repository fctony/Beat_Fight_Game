using UnityEngine;

public class AreaColliderTrigger : MonoBehaviour {

	public EnemyWaveSystem EnemyWaveSystem;

	void OnTriggerEnter(Collider coll){
		if (coll.CompareTag ("Player")) {
			if (EnemyWaveSystem != null)
				EnemyWaveSystem.StartNewWave ();
			Destroy (gameObject);
		}
	}
}
