using UnityEngine;
using UnityEngine.UI;

public class HUDPlayerPortrait : MonoBehaviour {

	public Image playerPortrait;

	void Start () {

		//loads the icon of the player that was selected in the character selection screen
		if(playerPortrait && GlobalPlayerData.PlayerHUDPortrait){
			playerPortrait.overrideSprite = GlobalPlayerData.PlayerHUDPortrait;
		}
	}
}
