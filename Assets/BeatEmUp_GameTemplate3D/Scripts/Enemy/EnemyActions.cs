using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class EnemyActions : MonoBehaviour {

	[Space(10)]
	[Header ("Linked components")]
	public GameObject target; //current target
	public UnitAnimator animator; //animator component
	public GameObject GFX; //GFX of this unit
	public Rigidbody rb; //rigidbody component
	public CapsuleCollider capsule; //capsule collider

	[Header("Attack Data")]
	public DamageObject[] AttackList; //a list of attacks
	public bool PickRandomAttack; //choose a random attack from the list
	public float hitZRange = 2; //the z range of all attacks
	public float defendChance = 0; //the chance that an incoming attack is defended %
	public float hitRecoveryTime = .4f; //the timeout after a hit before the enemy can do an action
	public float standUpTime = 1.1f; //the time it takes for this enemy to stand up
	public bool canDefendDuringAttack; //true if the enemy is able to defend an incoming attack while he is doing his own attack
	public bool AttackPlayerAirborne; //attack a player while he is in the air
	private DamageObject lastAttack; //data from the last attack that has taken place
	private int AttackCounter = 0; //current attack number
	public bool canHitEnemies; //true is this enemy can hit other enemies
	public bool canHitDestroyableObjects; //true is this enemy can hit destroyable objects like crates, barrels.
	[HideInInspector]
	public float lastAttackTime; //time of the last attack

	[Header ("Settings")]
	public bool pickARandomName; //assign a random name
	public TextAsset enemyNamesList; //the list of enemy names
	public string enemyName = ""; //the name of this enemy
	public float attackRangeDistance = 1.4f; //the distance from the target where the enemy is able to attack
	public float closeRangeDistance = 2f; //the distance from the target at close range
	public float midRangeDistance = 3f; //the distance from the target at mid range
	public float farRangeDistance = 4.5f; //the distance from the target at far range
	public float RangeMarging = 1f; //the amount of space that is allowed between the player and enemy before we reposition ourselves
	public float walkSpeed = 1.95f; //the speed of a walk
	public float walkBackwardSpeed = 1.2f; //the speed of walking backwards
	public float sightDistance = 10f; //the distance when we can see the target
	public float attackInterval = 1.2f; //the time inbetween attacking
	public float rotationSpeed = 15f; //the rotation speed when switching directions
	public float lookaheadDistance; //the distance at which we check for obstacles in from of us
	public bool ignoreCliffs; //ignore cliff detection
	public float KnockdownTimeout = 0f; //the time before we stand up after a knockdown
	public float KnockdownUpForce = 5f; //the up force of a knockDown
	public float KnockbackForce = 4; //the horizontal force of a knockDown
	private LayerMask HitLayerMask; //the layermask for damagable objects
	public LayerMask CollisionLayer; //the layers we check collisions with
	public bool randomizeValues = true; //randomize values to avoid enemy synchronization
	[HideInInspector]
	public float zSpreadMultiplier = 2f; //multiplyer for the z distance between enemies

	[Header ("Stats")]
	public RANGE range;
	public ENEMYTACTIC enemyTactic;
	public UNITSTATE enemyState;
	public DIRECTION currentDirection; 
	public bool targetSpotted;
	public bool cliffSpotted;
	public bool wallspotted;
	public bool isGrounded;
	public bool isDead;
	private Vector3 moveDirection;
	public float distance;
	private Vector3 fixedVelocity;
	private bool updateVelocity;

	//list of states where the enemy cannot move
	private List<UNITSTATE> NoMovementStates = new List<UNITSTATE> {
		UNITSTATE.DEATH,
		UNITSTATE.ATTACK,
		UNITSTATE.DEFEND,
		UNITSTATE.GROUNDHIT,
		UNITSTATE.HIT,
		UNITSTATE.IDLE,
		UNITSTATE.KNOCKDOWNGROUNDED,
		UNITSTATE.STANDUP,
	};

	//list of states where the player can be hit
	private List<UNITSTATE> HitableStates = new List<UNITSTATE> {
		UNITSTATE.ATTACK,
		UNITSTATE.DEFEND,
		UNITSTATE.HIT,
		UNITSTATE.IDLE,
		UNITSTATE.KICK,
		UNITSTATE.PUNCH,
		UNITSTATE.STANDUP,
		UNITSTATE.WALK,
		UNITSTATE.KNOCKDOWNGROUNDED,
	};

	[HideInInspector]
	public float ZSpread; //the distance between enemies on the z-axis

	//[HideInInspector]
	public Vector3 distanceToTarget;

	private List<UNITSTATE> defendableStates = new List<UNITSTATE> { UNITSTATE.IDLE, UNITSTATE.WALK, UNITSTATE.DEFEND }; //a list of states where the enemy is able to defend an incoming attack

	//global event handler for enemies
	public delegate void UnitEventHandler(GameObject Unit);

	//global event Handler for destroying units
	public static event UnitEventHandler OnUnitDestroy;

	//---

	public void OnStart(){

		//assign a name to this enemy
		if(pickARandomName) enemyName = GetRandomName();

		//set player as target
		if(target == null) target = GameObject.FindGameObjectWithTag("Player");

		//tell enemymanager to update the list of active enemies
		EnemyManager.getActiveEnemies();

		//enable defending during an attack
		if (canDefendDuringAttack) defendableStates.Add (UNITSTATE.ATTACK);

		//set up HitLayerMask
		HitLayerMask = 1 << LayerMask.NameToLayer("Player");
		if(canHitEnemies)HitLayerMask |= (1 << LayerMask.NameToLayer("Enemy"));
		if(canHitDestroyableObjects)HitLayerMask |= (1 << LayerMask.NameToLayer("DestroyableObject"));
	}

	#region Update

	//late Update
	public void OnLateUpdate(){

		//apply any root motion offsets to parent
		if(animator && animator.GetComponent<Animator>().applyRootMotion && animator.transform.localPosition != Vector3.zero) {
			Vector3 offset = animator.transform.localPosition;
			animator.transform.localPosition = Vector3.zero;
			transform.position += offset * (int)currentDirection;
		}
	}

	//physics update
	public void OnFixedUpdate() {
		if(updateVelocity) {
			rb.velocity = fixedVelocity;
			updateVelocity = false;
		}
	}

	//set velocity on next fixed update
	void SetVelocity(Vector3 velocity) {
		fixedVelocity = velocity;
		updateVelocity = true;
	}
	#endregion

	#region Attack

	//Attack
	public void ATTACK() {

		//don't attack when player is jumping
		var playerMovement = target.GetComponent<PlayerMovement>();
		if (!AttackPlayerAirborne && playerMovement != null && playerMovement.jumpInProgress) {
			return;

		} else {

			//init
			enemyState = UNITSTATE.ATTACK;
			Move(Vector3.zero, 0f);
			LookAtTarget(target.transform);
			TurnToDir(currentDirection);

			//pick random attack
			if (PickRandomAttack) AttackCounter = Random.Range (0, AttackList.Length);

			//play animation
			animator.SetAnimatorTrigger (AttackList[AttackCounter].animTrigger);

			//go to the next attack in the list
			if (!PickRandomAttack) {
				AttackCounter += 1;
				if (AttackCounter >= AttackList.Length) AttackCounter = 0;
			}

			lastAttackTime = Time.time;
			lastAttack = AttackList [AttackCounter];
			lastAttack.inflictor = gameObject;

			//resume
			Invoke ("Ready", AttackList [AttackCounter].duration);
		}
	}

	#endregion

	#region We are Hit

	//Unit was hit
	public void Hit(DamageObject d){
		if(HitableStates.Contains(enemyState)) {

			//only allow ground attacks to hit us when we are knocked down
			if(enemyState == UNITSTATE.KNOCKDOWNGROUNDED && !d.isGroundAttack) return;

			CancelInvoke();
			StopAllCoroutines();
			animator.StopAllCoroutines();
			Move(Vector3.zero, 0f);

			//add attack time out so this enemy cannot attack instantly after a hit
			lastAttackTime = Time.time;

			//don't hit this unit when it's allready down
			if((enemyState == UNITSTATE.KNOCKDOWNGROUNDED || enemyState == UNITSTATE.GROUNDHIT) && !d.isGroundAttack)
				return;

			//defend an incoming attack
			if(!d.DefenceOverride && defendableStates.Contains(enemyState)) {
				int rand = Random.Range(0, 100);
				if(rand < defendChance) {
					Defend();
					return;
				}
			}

			//hit sfx
			GlobalAudioPlayer.PlaySFXAtPosition(d.hitSFX, transform.position);

			//hit particle effect
			ShowHitEffectAtPosition(new Vector3(transform.position.x, d.inflictor.transform.position.y + d.collHeight, transform.position.z));

			//camera Shake
			CamShake camShake = Camera.main.GetComponent<CamShake>();
			if(camShake != null)
				camShake.Shake(.1f);

			//activate slow motion camera
			if(d.slowMotionEffect) {
				CamSlowMotionDelay cmd = Camera.main.GetComponent<CamSlowMotionDelay>();
				if(cmd != null)
					cmd.StartSlowMotionDelay(.2f);
			}

			//substract health
			HealthSystem hs = GetComponent<HealthSystem>();
			if(hs != null) {
				hs.SubstractHealth(d.damage);
				if(hs.CurrentHp == 0)
					return;
			}

			//ground attack
			if(enemyState == UNITSTATE.KNOCKDOWNGROUNDED) {
				StopAllCoroutines();
				enemyState = UNITSTATE.GROUNDHIT;
				StartCoroutine(GroundHit());
				return;
			}
				
			//turn towards the direction of the incoming attack
			int dir = d.inflictor.transform.position.x > transform.position.x? 1 : -1;
			TurnToDir((DIRECTION)dir);

			//check for a knockdown
			if(d.knockDown) {
				StartCoroutine(KnockDownSequence(d.inflictor));
				return;

			} else {

				//default hit
				int rand = Random.Range(1, 3);
				animator.SetAnimatorTrigger("Hit" + rand);
				enemyState = UNITSTATE.HIT;

				//add small force from the impact
				LookAtTarget(d.inflictor.transform);
				animator.AddForce(-KnockbackForce);

				//switch  enemy state from passive to aggressive when attacked
				if(enemyTactic != ENEMYTACTIC.ENGAGE) {
					EnemyManager.setAgressive(gameObject);
				}

				Invoke("Ready", hitRecoveryTime);
				return;
			}
		}
	}

	//Defend
	void Defend(){
		enemyState = UNITSTATE.DEFEND;
		animator.ShowDefendEffect();
		animator.SetAnimatorTrigger ("Defend");
		GlobalAudioPlayer.PlaySFX ("DefendHit");
		animator.SetDirection (currentDirection);
	}

	#endregion

	#region Check for hit

	//checks if we have hit something (Animation Event)
	public void CheckForHit() {

		//draws a hitbox in front of the character to see which objects are overlapping it
		Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)currentDirection * lastAttack.collDistance);
		Vector3 boxSize = new Vector3 (lastAttack.CollSize/2, lastAttack.CollSize/2, hitZRange/2);
		Collider[] hitColliders = Physics.OverlapBox(boxPosition, boxSize, Quaternion.identity, HitLayerMask); 

		int i=0;
		while (i < hitColliders.Length) {

			//hit a damagable object
			IDamagable<DamageObject> damagableObject = hitColliders[i].GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
			if (damagableObject != null && damagableObject != (IDamagable<DamageObject>)this) {
				damagableObject.Hit(lastAttack);
			}
			i++;
		}
	}

	//Display hit box + lookahead sphere in Unity Editor (Debug)
	#if UNITY_EDITOR 
	void OnDrawGizmos(){

		//visualize hitbox
		if (lastAttack != null && (Time.time - lastAttackTime) < lastAttack.duration) {
			Gizmos.color = Color.red;
			Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)currentDirection * lastAttack.collDistance);
			Vector3 boxSize = new Vector3 (lastAttack.CollSize, lastAttack.CollSize, hitZRange);
			Gizmos.DrawWireCube (boxPosition, boxSize);
		}

		//visualize lookahead sphere
		Gizmos.color = Color.yellow;
		Vector3 offset = -moveDirection.normalized * lookaheadDistance;
		Gizmos.DrawWireSphere (transform.position + capsule.center - offset, capsule.radius); 
	}

	#endif

	#endregion

	#region KnockDown Sequence

	//knockDown sequence
	IEnumerator KnockDownSequence(GameObject inflictor) {
		enemyState = UNITSTATE.KNOCKDOWN;
		yield return new WaitForFixedUpdate();

		//look towards the direction of the incoming attack
		int dir = 1;
		if(inflictor != null) dir = inflictor.transform.position.x > transform.position.x? 1 : -1;
		currentDirection = (DIRECTION)dir;
		animator.SetDirection(currentDirection);
		TurnToDir(currentDirection);

		//add knockback force
		animator.SetAnimatorTrigger("KnockDown_Up");
		while(IsGrounded()){
			SetVelocity(new Vector3(KnockbackForce * -dir, KnockdownUpForce, 0));
			yield return new WaitForFixedUpdate();
		}

		//going up...
		while(rb.velocity.y >= 0) yield return new WaitForFixedUpdate();

		//going down
		animator.SetAnimatorTrigger ("KnockDown_Down");
		while(!IsGrounded()) yield return new WaitForFixedUpdate();

		//hit ground
		animator.SetAnimatorTrigger ("KnockDown_End");
		GlobalAudioPlayer.PlaySFXAtPosition("Drop", transform.position);
		animator.SetAnimatorFloat ("MovementSpeed", 0f);
		animator.ShowDustEffectLand();
		enemyState = UNITSTATE.KNOCKDOWNGROUNDED;
		Move(Vector3.zero, 0f);

		//cam shake
		CamShake camShake = Camera.main.GetComponent<CamShake>();
		if (camShake != null) camShake.Shake(.3f);

		//dust effect
		animator.ShowDustEffectLand();

		//stop sliding
		float t = 0;
		float speed = 2;
		Vector3 fromVelocity = rb.velocity;
		while (t<1){
			SetVelocity(Vector3.Lerp (new Vector3(fromVelocity.x, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, fromVelocity.z), new Vector3(0, rb.velocity.y, 0), t));
			t += Time.deltaTime * speed;
			yield return new WaitForFixedUpdate();
		}

		//knockDown Timeout
		Move(Vector3.zero, 0f);
		yield return new WaitForSeconds(KnockdownTimeout);

		//stand up
		enemyState = UNITSTATE.STANDUP;
		animator.SetAnimatorTrigger ("StandUp");
		Invoke("Ready", standUpTime);
	}

	//ground hit
	public IEnumerator GroundHit(){
		CancelInvoke();
		GlobalAudioPlayer.PlaySFXAtPosition ("EnemyGroundPunchHit", transform.position);
		animator.SetAnimatorTrigger ("GroundHit");
		yield return new WaitForSeconds(KnockdownTimeout);
		if(!isDead)	animator.SetAnimatorTrigger ("StandUp");
		Invoke("Ready", standUpTime);
	}

	#endregion

	#region Movement

	//walk to target
	public void WalkTo(float proximityRange, float movementMargin){
		Vector3 dirToTarget;
		LookAtTarget(target.transform);
		enemyState = UNITSTATE.WALK;

		//clamp zspread to attackDistance when ENGAGED, otherwise we might not be able to reach the player at all
		if (enemyTactic == ENEMYTACTIC.ENGAGE) {
			dirToTarget = target.transform.position - (transform.position + new Vector3 (0, 0, Mathf.Clamp(ZSpread, 0, attackRangeDistance)));
		} else {
			dirToTarget = target.transform.position - (transform.position + new Vector3 (0, 0, ZSpread));
		}

		//we are too far away, move closer
		if (distance >= proximityRange ) {
			moveDirection = new Vector3(dirToTarget.x,0,dirToTarget.z);
			if (IsGrounded() && !WallSpotted() && !PitfallSpotted()) {
				Move(moveDirection.normalized, walkSpeed);
				animator.SetAnimatorFloat ("MovementSpeed", rb.velocity.sqrMagnitude);
				return;
			}
		}

		//we are too close, move away
		if (distance <= proximityRange - movementMargin) {
			moveDirection = new Vector3(-dirToTarget.x,0,0);
			if (IsGrounded() && !WallSpotted() && !PitfallSpotted()) {
				Move(moveDirection.normalized, walkBackwardSpeed);
				animator.SetAnimatorFloat ("MovementSpeed", -rb.velocity.sqrMagnitude);
				return;
			}
		}

		//otherwise do nothing
		Move(Vector3.zero, 0f);
		animator.SetAnimatorFloat ("MovementSpeed", 0);
	}

	//move towards a vector
	public void Move(Vector3 vector, float speed){
		if(!NoMovementStates.Contains(enemyState)) {
			SetVelocity(new Vector3(vector.x * speed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, vector.z * speed));
		} else {
			SetVelocity(Vector3.zero);
		}
	}

	//returns true if there is an environment collider in front of us
	bool WallSpotted(){
		Vector3 Offset =  moveDirection.normalized * lookaheadDistance;
		Collider[] hitColliders = Physics.OverlapSphere (transform.position + capsule.center + Offset, capsule.radius, CollisionLayer);

		int i = 0;
		bool hasHitwall = false;
		while (i < hitColliders.Length) {
			if(CollisionLayer == (CollisionLayer | 1 << hitColliders[i].gameObject.layer)) {
				hasHitwall = true;
			}
			i++;
		}
		wallspotted = hasHitwall;
		return hasHitwall;
	}

	//returns true if there is a cliff in front of us
	bool PitfallSpotted(){
		if (!ignoreCliffs) {
			float lookDownDistance = 1f;
			Vector3 StartPoint = transform.position + (Vector3.up * .3f) + (Vector3.right * (capsule.radius + lookaheadDistance) * moveDirection.normalized.x);
			RaycastHit hit;

			#if UNITY_EDITOR 
			Debug.DrawRay (StartPoint, Vector3.down * lookDownDistance, Color.red);
			#endif

			if (!Physics.Raycast (StartPoint, Vector3.down, out hit, lookDownDistance, CollisionLayer)) {
				cliffSpotted = true;
				return true;
			}
		}
		cliffSpotted = false;
		return false;
	}

	//returns true if this unit is grounded
	public bool IsGrounded(){
		float colliderSize = capsule.bounds.extents.y - .1f;
		if (Physics.CheckCapsule (capsule.bounds.center, capsule.bounds.center + Vector3.down*colliderSize, capsule.radius, CollisionLayer)) {
			isGrounded = true;
			return true;
		} else {
			isGrounded = false;
			return false;
		}
	}

	//turn towards a direction
	public void TurnToDir(DIRECTION dir) {
		transform.rotation = Quaternion.LookRotation(Vector3.forward * (int)dir);
	}

	#endregion

	//show hit effect
	public void ShowHitEffectAtPosition(Vector3 pos) {
		GameObject.Instantiate (Resources.Load ("HitEffect"), pos, Quaternion.identity);
	}

	//unit is ready for new actions
	public void Ready()	{
		enemyState = UNITSTATE.IDLE;
		animator.SetAnimatorTrigger("Idle");
		animator.SetAnimatorFloat ("MovementSpeed", 0f);
		Move(Vector3.zero, 0f);
	}

	//look at the current target
	public void LookAtTarget(Transform _target){
		if(_target != null){
			Vector3 newDir = Vector3.zero;
			int dir = _target.transform.position.x >= transform.position.x ? 1 : -1;
			currentDirection = (DIRECTION)dir;
			if (animator != null) animator.currentDirection = currentDirection;
			newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * dir, rotationSpeed * Time.deltaTime, 0.0f);	
			transform.rotation = Quaternion.LookRotation(newDir);
		}
	}

	//randomizes values
	public void SetRandomValues(){
		walkSpeed *= Random.Range(.8f, 1.2f);
		walkBackwardSpeed *= Random.Range(.8f, 1.2f);
		attackInterval *= Random.Range(.7f, 1.5f);
		KnockdownTimeout *= Random.Range(.7f, 1.5f);
		KnockdownUpForce *= Random.Range(.8f, 1.2f);
		KnockbackForce *= Random.Range(.7f, 1.5f);
	}

	//destroy event
	public void DestroyUnit(){
		if(OnUnitDestroy != null) OnUnitDestroy(gameObject);
	}

	//returns a random name
	string GetRandomName(){
		if(enemyNamesList == null) {
			Debug.Log("no list of names was found, please create 'EnemyNames.txt' that contains a list of enemy names and put it in the 'Resources' folder.");
			return "";
		}

		//convert the lines of the txt file to an array
		string data = enemyNamesList.ToString();
		string cReturns = System.Environment.NewLine + "\n" + "\r"; 
		string[] lines = data.Split(cReturns.ToCharArray());

		//pick a random name from the list
		string name = "";
		int cnt = 0;
		while(name.Length == 0 && cnt < 100) {
			int rand = Random.Range(0, lines.Length);
			name = lines[rand];
			cnt += 1;
		}
		return name;
	}
}