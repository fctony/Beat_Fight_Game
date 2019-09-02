using UnityEngine;
using System.Collections;

public class UIHelpText : MonoBehaviour {

	public float MenuDisplayTime = 7f;
	public float LerpTime = 2f;
	private InputManager inputmanager;
	public bool DestroyOnFinish = true;

	void Start () {
		inputmanager = GameObject.FindObjectOfType<InputManager>();
		StartCoroutine(HideMenu(MenuDisplayTime));
	}

	void Update(){
		
		//remove the text message when touchscreen input is activated
		if(inputmanager != null && inputmanager.UseTouchScreenInput) {
			Destroy(gameObject);
		}
	}

	//fade out
	IEnumerator HideMenu(float delay){
		yield return new WaitForSeconds(delay);
		RectTransform rect = GetComponent<RectTransform>();
		Vector2 startPos = rect.anchoredPosition;
		Vector2 endPos = new Vector2(startPos.x, startPos.y + rect.sizeDelta.y);

		float t = 0;
		while(t < 1) {
			if(rect != null) rect.anchoredPosition = Vector2.Lerp(startPos, endPos, MathUtilities.CoSinLerp(0, 1, t));
			t += Time.deltaTime/LerpTime;
			yield return null;
		}

		//destroy
		if(DestroyOnFinish)	Destroy(gameObject);
	}
}
