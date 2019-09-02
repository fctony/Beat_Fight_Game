using UnityEngine;

public class StateMachineFunctions : StateMachineBehaviour {

	public bool NotifyAnimatorOnFinish;

	//notify on animation finish
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if(NotifyAnimatorOnFinish) animator.gameObject.SendMessage ("Ready", SendMessageOptions.DontRequireReceiver);
	}
}