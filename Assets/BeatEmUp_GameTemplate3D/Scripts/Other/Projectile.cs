using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Collider))]
public class Projectile : MonoBehaviour {

	public float speed = 10;
	public DIRECTION direction;
	public bool destroyOnHit;
	public GameObject EffectOnSpawn;
	private DamageObject damage;

	void Start () {
		GetComponent<Rigidbody>().velocity = new Vector2((int)direction * speed, 0);
		GetComponent<Collider>().isTrigger = true;

		//show an effect when this projectile is spawned
		if(EffectOnSpawn) {
			GameObject effect = GameObject.Instantiate(EffectOnSpawn) as GameObject;
			effect.transform.position = transform.position;
		}
	}

	//tell the player that an item is in range
	void OnTriggerEnter(Collider coll) {
		if(coll.CompareTag("Enemy")) {

			//hit a damagable object
			IDamagable<DamageObject> damagableObject = coll.GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
			if(damagableObject != null) {
				damagableObject.Hit(damage);
				if(destroyOnHit) Destroy(gameObject);
			}
		}
	}

	//sets the damage of this projectile
	public void SetDamage(DamageObject d){
		damage = d;
	}
}
