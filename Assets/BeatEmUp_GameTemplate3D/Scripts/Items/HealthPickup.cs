using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HealthPickup : MonoBehaviour {

	public int RestoreHP = 1;
	public string pickupSFX = "";
	public GameObject pickupEffect;
	public float pickUpRange = 1;
	private GameObject[] Players;

	void Start(){
		Players = GameObject.FindGameObjectsWithTag("Player");
	}

	void LateUpdate(){
		foreach(GameObject player in Players) {
			if(player) {
				float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

				//player is in pickup range
				if(distanceToPlayer < pickUpRange)
					AddHealthToPlayer(player);
			}
		}
	}

	//add health to player
	void AddHealthToPlayer(GameObject player){
		HealthSystem hs = player.GetComponent<HealthSystem> ();

		if (hs != null) {

			//restore hp to unit
			hs.AddHealth(RestoreHP);

		} else {
			Debug.Log("no health system found on GameObject '" + player.gameObject.name + "'.");
		}

		//show pickup effect
		if (pickupEffect != null) {
			GameObject effect = GameObject.Instantiate (pickupEffect);
			effect.transform.position = transform.position;
		}

		//play sfx
		if (pickupSFX != "") {
			GlobalAudioPlayer.PlaySFXAtPosition (pickupSFX, transform.position);
		}

		Destroy(gameObject);
	}

	#if UNITY_EDITOR 
	void OnDrawGizmos(){

		//Show pickup range
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (transform.position, pickUpRange); 

	}
	#endif
}
