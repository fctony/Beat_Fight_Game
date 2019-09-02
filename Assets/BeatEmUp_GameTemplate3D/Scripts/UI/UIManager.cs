using UnityEngine;

public class UIManager : MonoBehaviour {

	public UIFader UI_fader;
	public UI_Screen[] UIMenus;

	void Awake(){
		DisableAllScreens();

		//don't destroy
		DontDestroyOnLoad(gameObject);
	}
		
	//shows a menu by name
	public void ShowMenu(string name, bool disableAllScreens){
		if(disableAllScreens) DisableAllScreens();
		GameObject UI_Gameobject = null;
		foreach (UI_Screen UI in UIMenus){
			if (UI.UI_Name == name) {
				UI_Gameobject = UI.UI_Gameobject;
			}
		}

		if (UI_Gameobject != null) {
			UI_Gameobject.SetActive (true);
		} else {
			Debug.Log ("no menu found with name: " + name);
		}

		//fadeIn
		if (UI_fader != null) UI_fader.gameObject.SetActive (true);
		UI_fader.Fade (UIFader.FADE.FadeIn, 1f, .3f);
	}

	public void ShowMenu(string name){
		ShowMenu(name, true);
	}

	//close a menu by name
	public void CloseMenu(string name){
		foreach (UI_Screen UI in UIMenus){
			if (UI.UI_Name == name)	UI.UI_Gameobject.SetActive (false);
		}
	}
		
	//disable all the menus
	public void DisableAllScreens(){
		foreach (UI_Screen UI in UIMenus){ 
			if(UI.UI_Gameobject != null) 
				UI.UI_Gameobject.SetActive(false);
			else 
				Debug.Log("Null ref found in UI with name: " + UI.UI_Name);
		}
	}
}
	
[System.Serializable]
public class UI_Screen {
	public string UI_Name;
	public GameObject UI_Gameobject;
}
