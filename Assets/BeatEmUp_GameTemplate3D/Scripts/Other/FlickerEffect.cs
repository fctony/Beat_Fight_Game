using UnityEngine;
using System.Collections;

public class FlickerEffect : MonoBehaviour {

	public float pauzeBeforeStart = 1.3f;
	public float flickerSpeedStart = 15f;
	public float flickerSpeedEnd = 35f;
	public float Duration = 2f;
	public bool DestroyOnFinish;

	public GameObject[] GFX;

	public void Start () {
		StartCoroutine(FlickerCoroutine());
	}
	
	IEnumerator FlickerCoroutine(){

		//pause before start
		yield return new WaitForSeconds (pauzeBeforeStart);

		//flicker
		float t =0;
		while(t < 1){
			float speed = Mathf.Lerp (flickerSpeedStart, flickerSpeedEnd, MathUtilities.Coserp(0,1,t));
			float i = Mathf.Sin(Time.time * speed);
			foreach(GameObject g in GFX) g.SetActive(i>0);
			t += Time.deltaTime/Duration;
			yield return null;
		}

		//hide
		foreach(GameObject g in GFX) g.SetActive(false);

		//destroy
		if (DestroyOnFinish) {
			Destroy (gameObject);
		}
	}
}
