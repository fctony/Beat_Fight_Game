using UnityEngine;

[System.Serializable]
public class AudioItem {

	public string name;
	public float volume = 1f;
	public float MinTimeBetweenCall = 0;
	public bool loop;
	public AudioClip[] clip;
	[HideInInspector]
	public float lastTimePlayed = 0;
}
