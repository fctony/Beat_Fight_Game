using UnityEngine;

public class UnitState : MonoBehaviour {

	public UNITSTATE currentState = UNITSTATE.IDLE;

	public void SetState(UNITSTATE state){
		currentState = state;
	}
}

public enum UNITSTATE {
	IDLE,
	WALK,
	JUMPING,
	LAND,
	JUMPKICK,
	PUNCH,
	KICK,
	ATTACK,
	DEFEND,
	HIT,
	DEATH,
	THROW,
	PICKUPITEM,
	KNOCKDOWN,
	KNOCKDOWNGROUNDED,
	GROUNDPUNCH,
	GROUNDKICK,
	GROUNDHIT,
	STANDUP,
	USEWEAPON,
};