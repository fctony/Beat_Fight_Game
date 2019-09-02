using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
  
public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	
	public INPUTACTION actionDown;
	public INPUTACTION actionUp;
	public bool updateEveryFrame = false;
	private bool pressed;

	public void OnPointerDown(PointerEventData eventData){
		InputManager.CombatInputEvent(actionDown);
		pressed = true;
	}

	public void OnPointerUp(PointerEventData eventData){
		InputManager.CombatInputEvent(actionUp);
		pressed = false;
	}

	void Update(){
		if(updateEveryFrame) InputManager.OnDefendButtonPress(pressed);
	}
}

