using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class KillBox : MonoBehaviour {

	public bool RestartOnPlayerKill;
	public bool RestartOnEnemyKill;

	//destroy everything that enters this trigger
	void OnTriggerEnter(Collider coll){

		//restart level on player kill
		if(RestartOnPlayerKill && coll.CompareTag("Player")) StartCoroutine(RestartLevel());

		//restart level on enemy kill
		if(RestartOnEnemyKill && coll.CompareTag("Enemy")) StartCoroutine(RestartLevel());

		//destroy gameobject
		Destroy (coll.gameObject);
	}

	//restart level
	IEnumerator RestartLevel(){
		
		//fade to black
		UIManager UI = GameObject.FindObjectOfType<UIManager>();
		if (UI != null) {
			float fadeOutTime = 0.7f;
			UI.UI_fader.Fade (UIFader.FADE.FadeOut, fadeOutTime, 0);
			yield return new WaitForSeconds (fadeOutTime);
		}

		//load level
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}
}