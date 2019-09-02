using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour {

	public Text nameField;
	public Slider HpSlider;
	public bool isPlayer;

	void OnEnable() {
		HealthSystem.onHealthChange += UpdateHealth;
	}

	void OnDisable() {
		HealthSystem.onHealthChange -= UpdateHealth;
	}

	void Start(){
		HpSlider.gameObject.SetActive(isPlayer);
	}

	void UpdateHealth(float percentage, GameObject go){
		if(isPlayer && go.CompareTag("Player")){
			HpSlider.value = percentage;
		} 	

		if(!isPlayer && go.CompareTag("Enemy")){
			HpSlider.gameObject.SetActive(true);
			HpSlider.value = percentage;
			nameField.text = go.GetComponent<EnemyActions>().enemyName;
			if(percentage == 0) Invoke("HideOnDestroy", 2);
		}
	}

	void HideOnDestroy(){
		HpSlider.gameObject.SetActive(false);
		nameField.text = "";
	}
}
