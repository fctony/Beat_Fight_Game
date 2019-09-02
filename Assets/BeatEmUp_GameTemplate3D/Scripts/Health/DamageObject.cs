using UnityEngine;

[System.Serializable]
public class DamageObject {

	public string animTrigger = "";
	public int damage;
	public float duration = 1f;
	public float comboResetTime = .5f;
	public string hitSFX = "";
	public bool knockDown;
	public bool slowMotionEffect;
	public bool DefenceOverride;
	public bool isGroundAttack;

	[Header ("Hit Collider Settings")]
	public float CollSize;
	public float collDistance;
	public float collHeight;

	[HideInInspector]
	public GameObject inflictor;

	public DamageObject(int _damage, GameObject _inflictor){
		damage =  _damage;
		inflictor = _inflictor;
	}
}

public enum AttackType {
	Default = 0,
	SoftPunch = 10,
	MediumPunch = 20,
	KnockDown = 30,
	SoftKick = 40,
	HardKick = 50,
	SpecialMove = 60,
	DeathBlow = 70,
};
