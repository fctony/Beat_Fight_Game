using UnityEngine;

public class TimeToLive : MonoBehaviour {

	public float LifeTime = 1;
	
	void Start(){
		Invoke("DestroyGO", LifeTime);
	}

	void DestroyGO(){
		Destroy(gameObject);
	}
}
