using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UICharSelectionPortrait : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	[Header("The Player Character Prefab")]
	public GameObject PlayerPrefab;
	[Space(15)]

	public Image Border;
	public Color BorderColorDefault;
	public Color BorderColorOver;
	public Color BorderColorHighlight;
	public string PlaySFXOnClick = "";
	public bool Selected;

	[Header("HUD Portrait")]
	public Sprite HUDPortrait;

	//on mouse enter
	public void OnPointerEnter(PointerEventData eventData){
		Select();
	}
		
	//on mouse exit
	public void OnPointerExit(PointerEventData eventData){
		Deselect();
	}

	//on click
	public void OnPointerClick(PointerEventData eventData){
		OnClick();
	}

	//select
	public void Select(){
		if(Border && !Selected) Border.color = BorderColorOver;
	}

	//deselect
	public void Deselect(){
		if(Border && !Selected) Border.color = BorderColorDefault;
	}

	//On Click
	public void OnClick(){
		ResetAllButtons();
		Selected = true;
		if(Border) Border.color = BorderColorHighlight;

		//play sfx
		GlobalAudioPlayer.PlaySFX(PlaySFXOnClick);

		//set selected player prefab
		CharSelection Cs = GameObject.FindObjectOfType<CharSelection>();
		if(Cs) Cs.SelectPlayer(PlayerPrefab);

		//set hud icon for this player
		GlobalPlayerData.PlayerHUDPortrait = HUDPortrait;
	}

	//reset all button states
	public void ResetAllButtons(){
		UICharSelectionPortrait[] allButtons = GameObject.FindObjectsOfType<UICharSelectionPortrait>();
		foreach(UICharSelectionPortrait button in allButtons) { 
			button.Border.color = button.BorderColorDefault;
			button.Selected = false;
		}
	}
}