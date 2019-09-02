using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnitState))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour {

	[Header("Linked Components")]
	private UnitAnimator animator;
	private Rigidbody rb;
	private UnitState playerState;
	private CapsuleCollider capsule;

	[Header("Settings")]
	public float walkSpeed = 3f;
	public float ZSpeed = 1.5f;
	public float JumpForce = 8f;
	public bool AllowAirControl;
	public bool AllowDepthJumping;
	public float AirAcceleration = 3f;
	public float AirMaxSpeed = 3f;
	public float rotationSpeed = 15f;
	public float jumpRotationSpeed = 30f;
	public float lookAheadDistance = .2f;
	public float landRecoveryTime = .1f;
	public LayerMask CollisionLayer;

	[Header("Audio")]
	public string jumpUpVoice = "";
	public string jumpLandVoice = "";

	[Header("Stats")]
	public DIRECTION currentDirection;
	public Vector2 inputDirection;
	public bool jumpInProgress;
	public bool isGrounded;

	private bool isDead = false;
	private Vector3 fixedVelocity;
	private bool updateVelocity;
	private Plane[] frustrumPlanes; //camera view frustrum
	public bool playerInCameraView; //true if the player bounds are inside the camera view
	public float frustrumDistance = 1f;

	//a list of states that this component can influence
	private List<UNITSTATE> MovementStates = new List<UNITSTATE> {
		UNITSTATE.IDLE,
		UNITSTATE.WALK,
		UNITSTATE.JUMPING,
		UNITSTATE.JUMPKICK,
		UNITSTATE.LAND,
	};

	//--

	void OnEnable() {
		InputManager.onCombatInputEvent += InputEventAction;
		InputManager.onInputEvent += InputEvent;
	}

	void OnDisable() {
		InputManager.onCombatInputEvent -= InputEventAction;
		InputManager.onInputEvent -= InputEvent;
	}
		
	void Start(){
		
		//find components
		if(!animator) animator = GetComponentInChildren<UnitAnimator>();
		if(!rb) rb = GetComponent<Rigidbody>();
		if(!playerState) playerState = GetComponent<UnitState>();
		if(!capsule) capsule = GetComponent<CapsuleCollider>();
			
		//error messages for missing components
		if(!animator) Debug.LogError("No animator found inside " + gameObject.name);
		if(!rb) Debug.LogError("No Rigidbody component found on " + gameObject.name);
		if(!playerState) Debug.LogError("No UnitState component found on " + gameObject.name);
		if(!capsule) Debug.LogError("No Capsule Collider found on " + gameObject.name);
	}
		
	//physics update
	void FixedUpdate() {

		//check if we are on the ground
		isGrounded = IsGrounded();

		if(animator) {

			//set grounded
			animator.SetAnimatorBool("isGrounded", isGrounded);

			//check if we're falling
			animator.SetAnimatorBool("Falling", !isGrounded && rb.velocity.y < 0.1f && playerState.currentState != UNITSTATE.KNOCKDOWN);

			//update animator direction
			animator.currentDirection = currentDirection;
		}

		//check if the player is inside the camera view area
		playerInCameraView = PlayerInsideCamViewArea();

		//update movement velocity
		if(updateVelocity && MovementStates.Contains(playerState.currentState)) {
			rb.velocity = fixedVelocity;
			updateVelocity = false;
		}
	}

	//set velocity on next fixed update
	void SetVelocity(Vector3 velocity) {
		fixedVelocity = velocity;
		updateVelocity = true;
	}

	//user input
	void InputEvent(Vector2 dir) {
		inputDirection = dir;
		if(MovementStates.Contains(playerState.currentState) && !isDead) {
			if(jumpInProgress) {
				MoveAirborne();
			} else {
				MoveGrounded();
			}
		}
	}

	//input actions
	void InputEventAction(INPUTACTION action) {

		//jump
		if(MovementStates.Contains(playerState.currentState) && !isDead) {
			if(action == INPUTACTION.JUMP) {
				if(playerState.currentState != UNITSTATE.JUMPING && IsGrounded()) {
					StopAllCoroutines();
					StartCoroutine(doJump());
				}
			}
		}
	}

	//jump
	IEnumerator doJump() {

		//set jump state
		jumpInProgress = true;
		playerState.SetState(UNITSTATE.JUMPING);

		//play animation
		animator.SetAnimatorBool("JumpInProgress", true);
		animator.SetAnimatorTrigger("JumpUp");
		animator.ShowDustEffectJump();

		//play sfx
		if(jumpUpVoice != "") {
			GlobalAudioPlayer.PlaySFXAtPosition(jumpUpVoice, transform.position);
		}

		//set state
		yield return new WaitForFixedUpdate();

		//start jump
		while(isGrounded) {
			SetVelocity(Vector3.up * JumpForce);
			yield return new WaitForFixedUpdate();
		}

		//continue until we hit the ground
		while(!isGrounded) {
			yield return new WaitForFixedUpdate();
		}

		//land
		playerState.SetState(UNITSTATE.LAND);
		SetVelocity(Vector3.zero);

		animator.SetAnimatorFloat("MovementSpeed", 0f);
		animator.SetAnimatorBool("JumpInProgress", false);
		animator.SetAnimatorBool("JumpKickActive", false);
		animator.ShowDustEffectLand();

		//sfx
		GlobalAudioPlayer.PlaySFX("FootStep");
		if(jumpLandVoice != "") GlobalAudioPlayer.PlaySFXAtPosition(jumpLandVoice, transform.position);

		jumpInProgress = false;

		if(playerState.currentState == UNITSTATE.LAND) {
			yield return new WaitForSeconds(landRecoveryTime);
			setPlayerState(UNITSTATE.IDLE);
		}
	}

	//returns true if the player is grounded
	public bool IsGrounded() {
		
		//check for capsule collisions with a 0.1 downwards offset from our capsule collider
		Vector3 bottomCapsulePos = transform.position + (Vector3.up) * (capsule.radius - 0.1f);
		if(Physics.CheckCapsule(transform.position + capsule.center, bottomCapsulePos, capsule.radius, CollisionLayer)) {
			isGrounded = true;
		} else {
			isGrounded = false;
		}
		return isGrounded;
	}

	//move while on the ground
	void MoveGrounded() {

		//don't move while landing from a jump
		if(playerState.currentState == UNITSTATE.LAND) {
			return;
		}

		//set rigidbody velocity
		if(rb != null && (inputDirection.sqrMagnitude > 0) && !WallInFront() && PlayerInsideCamViewArea()) {
			SetVelocity(new Vector3(inputDirection.x * -walkSpeed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, inputDirection.y * -ZSpeed));
			setPlayerState(UNITSTATE.WALK);
		} else {
			SetVelocity(new Vector3(0, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0));
			setPlayerState(UNITSTATE.IDLE);
		}

		//allow up/down movement when the player is at the edge of the screen
		if(!PlayerInsideCamViewArea() && Mathf.Abs(inputDirection.y) > 0) {
			Vector3 dirToCam = (transform.position - Camera.main.transform.position) * inputDirection.y;
			SetVelocity(new Vector3(dirToCam.x, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, dirToCam.z));
		}

		//set current direction based on the input vector. (ignore up and down by using 'mathf.sign' because we want the player to stay in the current direction when moving up/down)
		int dir = Mathf.RoundToInt(Mathf.Sign((float)-inputDirection.x));
		if(Mathf.Abs(inputDirection.x) > 0) {
			currentDirection = (DIRECTION)dir;
		}

		//send movement speed to animator
		if(animator) animator.SetAnimatorFloat("MovementSpeed", rb.velocity.magnitude);

		//look towards traveling direction
		LookToDir(currentDirection);
	}

	//move while in the air
	void MoveAirborne(){
		if(!WallInFront() && PlayerInsideCamViewArea()) {

			//keep turning while in the air
			int lastKnownDirection = (int)currentDirection;
			if(Mathf.Abs(inputDirection.x) > 0) {
				lastKnownDirection = Mathf.RoundToInt(-inputDirection.x);
			}
			LookToDir((DIRECTION)lastKnownDirection);

			//movement direction based on current input
			int dir = Mathf.Clamp(Mathf.RoundToInt(-inputDirection.x), -1, 1);
			float xpeed = Mathf.Clamp(rb.velocity.x + AirMaxSpeed * dir * Time.fixedDeltaTime * AirAcceleration, -AirMaxSpeed, AirMaxSpeed);

			//apply movement
			if(!updateVelocity){
				if(AllowDepthJumping) {
					SetVelocity(new Vector3(xpeed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, -inputDirection.y * ZSpeed));
				} else {
					SetVelocity(new Vector3(xpeed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0));
				}
			}
		} else {
			if(!updateVelocity) SetVelocity(new Vector3(0f, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0f));
		}
	}

	//look towards a direction
	public void LookToDir(DIRECTION dir) {
		Vector3 newDir = Vector3.zero;
		if(dir == DIRECTION.Right || dir == DIRECTION.Left) {
			if(isGrounded) { 
				newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * -(int)dir, rotationSpeed * Time.deltaTime, 0.0f);
			} else {
				newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * -(int)dir, jumpRotationSpeed * Time.deltaTime, 0.0f);
			}

			transform.rotation = Quaternion.LookRotation(newDir);
			currentDirection = dir;
		}
	}

	//update the direction based on the current input
	public void updateDirection() {
		LookToDir(currentDirection);
	}

	//the player has died
	void Death() {
		isDead = true;
		SetVelocity(Vector3.zero);
	}

	//returns true if there is a environment collider in front of us
	bool WallInFront() {
		var MovementOffset = new Vector3(inputDirection.x, 0, inputDirection.y) * lookAheadDistance;
		var c = GetComponent<CapsuleCollider>();
		Collider[] hitColliders = Physics.OverlapSphere(transform.position + Vector3.up * (c.radius + .1f) + -MovementOffset, c.radius, CollisionLayer);

		int i = 0;
		bool hasHitwall = false;
		while(i < hitColliders.Length) {
			if(CollisionLayer == (CollisionLayer | 1 << hitColliders[i].gameObject.layer)) {
				hasHitwall = true;
			}
			i++;
		}
		return hasHitwall;
	}

	//draw a lookahead sphere in the unity editor
	#if UNITY_EDITOR
	void OnDrawGizmos() {
		var c = GetComponent<CapsuleCollider>();
		Gizmos.color = Color.yellow;
		Vector3 MovementOffset = new Vector3(inputDirection.x, 0, inputDirection.y) * lookAheadDistance;
		Gizmos.DrawWireSphere(transform.position + Vector3.up * (c.radius + .1f) + -MovementOffset, c.radius);
	}
	#endif

	//returns current direction
	public DIRECTION getCurrentDirection() {
		return currentDirection;
	}

	//set current direction
	public void SetDirection(DIRECTION dir) {
		currentDirection = dir;
		LookToDir(currentDirection);
	}

	//set the playerstate
	public void setPlayerState(UNITSTATE state) {
		if(playerState != null) {
			playerState.SetState(state);
		} else {
			Debug.Log("no playerState found on this gameObject");
		}
	}

	//interrups an ongoing jump
	public void CancelJump(){
		jumpInProgress = false;
		StopAllCoroutines();
	}

	//returns true if the player is inside the camera viewing frustrum
	bool PlayerInsideCamViewArea(){
		if(Camera.main != null) {
			frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
			Bounds bounds = new Bounds(transform.position + Vector3.right * (int)currentDirection, Vector3.one * capsule.radius);
			return GeometryUtility.TestPlanesAABB(frustrumPlanes, bounds);
		}
		return true;
	}
}

public enum DIRECTION {
	Right = -1,
	Left = 1,
	Up = 2,
	Down = -2,
};
