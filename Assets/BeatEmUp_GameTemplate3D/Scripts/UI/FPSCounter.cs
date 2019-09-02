using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour {

	public float frequency = 0.5f;
	public int FramesPerSec { get; protected set; }
	private Text text;

	private void Start() {
		text = GetComponent<Text>();

		GameSettings settings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
		if(settings != null & settings.showFPSCounter) {
			text.enabled = true;
			StartCoroutine(FPS());
		} else {
			Destroy(gameObject);
		}
	}

	private IEnumerator FPS() {
		for(;;){
			// Capture frame-per-second
			int lastFrameCount = Time.frameCount;
			float lastTime = Time.realtimeSinceStartup;
			yield return new WaitForSeconds(frequency);
			float timeSpan = Time.realtimeSinceStartup - lastTime;
			int frameCount = Time.frameCount - lastFrameCount;

			// Display
			FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
			text.text = "FPS:" + FramesPerSec.ToString();
		}
	}
}