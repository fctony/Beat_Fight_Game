using UnityEngine;

public class PlaySFXOnStart : MonoBehaviour {

	public string sfx;

	void Start () {
		GlobalAudioPlayer.PlaySFXAtPosition (sfx, transform.position, transform);
	}
}
