using UnityEngine;
using System.Collections;
using Image = UnityEngine.UI.Image;

public class UIFader : MonoBehaviour {

	public Image img;
	public enum FADE { FadeIn, FadeOut }

	public void Fade(FADE fadeDir, float fadeDuration, float StartDelay){
		if(img != null){

			if (fadeDir == FADE.FadeIn){ 
				StartCoroutine(FadeCoroutine(1f, 0f, fadeDuration, StartDelay, true));
			}

			if (fadeDir == FADE.FadeOut){ 
				StartCoroutine(FadeCoroutine(0f, 1f, fadeDuration, StartDelay, false));

			}
		}
	}

	IEnumerator FadeCoroutine(float From, float To, float Duration, float StartDelay, bool DisableOnFinish){
		yield return new WaitForSeconds(StartDelay);
		
		float t=0;
		Color col = img.color;
		img.enabled = true;
		img.color = new Color(col.r, col.g, col.b, From);

		while(t<1){
			float alpha = Mathf.Lerp (From, To, t);
			img.color = new Color(col.r, col.g, col.b, alpha);
			t += Time.deltaTime/Duration;
			yield return 0;
		}

		img.color = new Color(col.r, col.g, col.b, To);
		img.enabled = !DisableOnFinish;
	}
}
