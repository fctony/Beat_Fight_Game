using UnityEngine;

public class WeaponPickup : MonoBehaviour {

	[Header("Weapon Settings")]
	public Weapon weapon;

	[Header("Pickup Settings")]
	public string SFX = "";
	public GameObject pickupEffect;
	public float pickUpRange = 1;

	private GameObject[] Players;
	private GameObject playerinRange;

	void Start(){
		Players = GameObject.FindGameObjectsWithTag("Player");
	}

	//Checks if this item is in pickup range
	void LateUpdate(){
		foreach(GameObject player in Players) {
			if(player) {
				float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

				//item in pickup range
				if(distanceToPlayer < pickUpRange && playerinRange == null) {
					playerinRange = player;
					player.SendMessage("ItemInRange", gameObject, SendMessageOptions.DontRequireReceiver);
					return;

				}

				//item out of pickup range
				if(distanceToPlayer > pickUpRange && playerinRange != null) {
					player.SendMessage("ItemOutOfRange", gameObject, SendMessageOptions.DontRequireReceiver);
					playerinRange = null;
				}
			}
		}
	}

	//pick up this item
	public void OnPickup(GameObject player){

		//show pickup effect
		if (pickupEffect) {
			GameObject effect = GameObject.Instantiate (pickupEffect);
			effect.transform.position = transform.position;
		}

		//play sfx
		if(SFX != null) GlobalAudioPlayer.PlaySFX(SFX);

		//give weapon to player
		GiveWeaponToPlayer(player);

		//remove pickup
		Destroy(gameObject);
	}

	public void GiveWeaponToPlayer(GameObject player){
		PlayerCombat pc = player.GetComponent<PlayerCombat>();
		if(pc) pc.equipWeapon(weapon);
	}

	#if UNITY_EDITOR 

	//Show pickup range
	void OnDrawGizmos(){
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (transform.position, pickUpRange); 
	}
	#endif
}