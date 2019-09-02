using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : EnemyActions, IDamagable<DamageObject>{

	[Space(10)]
	public bool enableAI;

	//a list of states where the AI is executed
	private List<UNITSTATE> ActiveAIStates = new List<UNITSTATE> { 
		UNITSTATE.IDLE, 
		UNITSTATE.WALK 
	};

	void Start(){

		//add this enemy to the enemylist
		EnemyManager.enemyList.Add(gameObject);

		//set z spread (zspread is used to keep space between the enemies)
		ZSpread = (EnemyManager.enemyList.Count-1);
		Invoke ("SetZSpread", .1f);	

		//randomize values to avoid synchronous movement
		if(randomizeValues) SetRandomValues();

		OnStart();
	}

	void FixedUpdate(){
		OnFixedUpdate();
	}

	void LateUpdate(){
		OnLateUpdate();
	}

	void Update(){

		//do nothing when there is no target or when AI is disabled
		if (target == null || !enableAI) {
			Ready ();
			return;

		} else {

			//get range to target
			range = GetDistanceToTarget ();
		}
			
		if(!isDead && enableAI){
			if(ActiveAIStates.Contains(enemyState) && targetSpotted) {

				//AI active 
				AI();

			} else {

				//try to spot the player
				if(distanceToTarget.magnitude < sightDistance) targetSpotted = true;
			}
		}
	}

	void AI(){
		LookAtTarget(target.transform);
		if (range == RANGE.ATTACKRANGE){

			//attack the target
			if (!cliffSpotted){
				if (Time.time - lastAttackTime > attackInterval) {
					ATTACK();
				} else {
					Ready();
				}
				return;
			}

			//actions for ATTACKRANGE distance
			if (enemyTactic == ENEMYTACTIC.KEEPCLOSEDISTANCE) WalkTo(closeRangeDistance, 0f);
			if (enemyTactic == ENEMYTACTIC.KEEPMEDIUMDISTANCE) WalkTo(midRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.KEEPFARDISTANCE) WalkTo(farRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.STANDSTILL) Ready ();

		} else {

			//actions for CLOSERANGE, MIDRANGE & FARRANGE distances
			if (enemyTactic == ENEMYTACTIC.ENGAGE) WalkTo (attackRangeDistance, 0f);
			if (enemyTactic == ENEMYTACTIC.KEEPCLOSEDISTANCE) WalkTo(closeRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.KEEPMEDIUMDISTANCE) WalkTo(midRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.KEEPFARDISTANCE) WalkTo(farRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.STANDSTILL) Ready();
		}
	}

	//update the current range
	private RANGE GetDistanceToTarget(){
		if (target != null) {

			//get distance from the target
			distanceToTarget = target.transform.position - transform.position;
			distance = Vector3.Distance (target.transform.position, transform.position);

			float distX = Mathf.Abs(distanceToTarget.x);
			float distZ = Mathf.Abs(distanceToTarget.z);

			//AttackRange
			if(distX <= attackRangeDistance){
				if(distZ < (hitZRange/2f)) 
					return RANGE.ATTACKRANGE;
				else
					return RANGE.CLOSERANGE;
			}

			//Close Range
			if (distX > attackRangeDistance && distX < midRangeDistance) return RANGE.CLOSERANGE;

			//Mid range
			if(distX > closeRangeDistance && distance < farRangeDistance) return RANGE.MIDRANGE;

			//Far range
			if(distX > farRangeDistance) return RANGE.FARRANGE;

		}
		return RANGE.FARRANGE;
	}
		
	//set an enemy tactic
	public void SetEnemyTactic(ENEMYTACTIC tactic){
		enemyTactic = tactic;
	}

	//spread enemies out in z distance
	void SetZSpread(){
		ZSpread = (ZSpread - (float)(EnemyManager.enemyList.Count - 1) / 2f) * (capsule.radius*2) * zSpreadMultiplier;
		if (ZSpread > attackRangeDistance) ZSpread = attackRangeDistance - 0.1f;
	}

	//Unit has died
	void Death(){
		StopAllCoroutines();
		CancelInvoke();

		enableAI = false;
		isDead = true;
		animator.SetAnimatorBool("isDead", true);
		Move(Vector3.zero, 0);
		EnemyManager.RemoveEnemyFromList(gameObject);
		gameObject.layer = LayerMask.NameToLayer ("Default");

		//ground death
		if(enemyState == UNITSTATE.KNOCKDOWNGROUNDED) {
			StartCoroutine(GroundHit());
		} else {

			//normal death
			animator.SetAnimatorTrigger("Death");
		}

		GlobalAudioPlayer.PlaySFXAtPosition("EnemyDeath", transform.position);
		StartCoroutine (animator.FlickerCoroutine(2));
		enemyState = UNITSTATE.DEATH;
		DestroyUnit();
	}
}

public enum ENEMYTACTIC {
	ENGAGE = 0,
	KEEPCLOSEDISTANCE = 1,
	KEEPMEDIUMDISTANCE = 2,
	KEEPFARDISTANCE = 3,
	STANDSTILL = 4,
}

public enum RANGE {
	ATTACKRANGE,
	CLOSERANGE,
	MIDRANGE,
	FARRANGE,
}