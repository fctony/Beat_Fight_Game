using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharSelection : MonoBehaviour {

	public GameObject ContinueButton;
	public string ContinueButtonSFXOnClick = "";
	public string loadLevelOnExit = "Game";
	private bool rightButtonDown;
	private bool leftButtonDown;
	private UICharSelectionPortrait[] portraits;

	void OnEnable(){
		InputManager.onInputEvent += InputEvent;
		InputManager.onCombatInputEvent += CombatInputEvent;
		if(ContinueButton) ContinueButton.SetActive(false);
	}

	void OnDisable() {
		InputManager.onInputEvent -= InputEvent;
		InputManager.onCombatInputEvent -= CombatInputEvent;
	}

	void Start(){
		portraits = GetComponentsInChildren<UICharSelectionPortrait>();

		//select a portrait by default when keyboard or joypad controls are used
		InputManager im = GameObject.FindObjectOfType<InputManager>();
		if(im && (im.UseJoypadInput || im.UseKeyboardInput)) GetComponentInChildren<UICharSelectionPortrait>().OnClick();
	}

	//select a player
	public void SelectPlayer(GameObject playerPrefab){
		GlobalPlayerData.Player1Prefab = playerPrefab;
		setContinueButtonVisible();
	}

	//continue
	public void OnContinueButtonClick(){
		GlobalAudioPlayer.PlaySFX(ContinueButtonSFXOnClick);
		UIManager UI = GameObject.FindObjectOfType<UIManager>();
		if(UI) {
			UI.UI_fader.Fade(UIFader.FADE.FadeOut, .3f, 0f);
			Invoke("loadLevel", .5f);
		}
	}

	void setContinueButtonVisible(){
		if(ContinueButton) ContinueButton.SetActive(true);
	}

	//load level
	void loadLevel(){
		if(!string.IsNullOrEmpty(loadLevelOnExit)) { 
			SceneManager.LoadScene(loadLevelOnExit);
		} else {
			Debug.Log("please define a level to load on character selection screen exit");
		}
	}

	//joypad or keyboard input event
	void InputEvent(Vector2 dir) {
		if(Mathf.Abs(dir.x) > 0){
			if(!leftButtonDown && dir.x < 0) OnLeftButtonDown();
			if(!rightButtonDown && dir.x > 0) OnRightButtonDown();
			return;
		}
		leftButtonDown = rightButtonDown = false;
	}
		
	//select portrait on the left
	void OnLeftButtonDown(){
		leftButtonDown = true;

		for(int i = 0; i < portraits.Length; i++) {
			if(portraits[i].Selected) {
				if(i-1 >= 0) {
					portraits[i].ResetAllButtons();
					portraits[i-1].OnClick();
					return;
				}
			}
		}
	}

	//select portrait on the right
	void OnRightButtonDown(){
		rightButtonDown = true;

		for(int i = 0; i < portraits.Length; i++) {
			if(portraits[i].Selected) {
				if(i+1 < portraits.Length) {
					portraits[i].ResetAllButtons();
					portraits[i+1].OnClick();
					return;
				}
			}
		}
	}

	//joypad or keyboard event
	private void CombatInputEvent(INPUTACTION action) {
		OnContinueButtonClick();
	}

	void Update(){

		//alternative input event
		if(Input.GetKeyDown(KeyCode.Return)){
			OnContinueButtonClick();
		}
	}
}
