using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour {
	//with help from https://weeklyhow.com/how-to-make-a-health-bar-in-unity/#Using_Slider_UI_for_the_Health_Points

	private float _health;
	private float _shields;
	
	private bool player = false; //if true, then update UI with info
	GameObject shieldTarget;
	GameObject healthTarget;

	private float lastDamage;
	private float timeSinceLD;
	private float lastRecharge; //for shield recharge rate
	private float lastHeal; //for health regen rate
	private bool healing; //if true, restore 9 health per second. True after 10 seconds undamaged
	private bool recharging; //if true, restore 40 shields per second. True after 5 seconds undamaged

	private Respawner _respawner;
	private SlayMode _slayer;

	void Start(){
		_respawner = GameObject.Find("RespawnHub").GetComponent<Respawner>();
		_slayer = GameObject.Find("SlayerMode").GetComponent<SlayMode>();

		if(gameObject.tag == "Player"){
			player = true;
			shieldTarget = GameObject.Find("Shields");
			healthTarget = GameObject.Find("Health");
		}

		_health = 45f;
		_shields = 70f;

		lastDamage = Time.time;
	}

	void Update(){
		if(gameObject.activeSelf){ //possibly redundant?
			timeSinceLD = Time.time - lastDamage;

			if((timeSinceLD >= 5)&&(_shields != 70)){ //activate recharge once active
				if(recharging == true){ //recharge since last recharge tick
					_shields += (Time.time - lastRecharge) * 40f;

				} else { //recharge since the recharging initialized
					_shields += (Time.time - lastDamage - 5f) * 40f;
					recharging = true;
				}

				if(_shields > 70){ //catch shield overflow
					_shields = 70f;
					//Debug.Log(gameObject.name +" recharge to "+ _shields);
				}

				if(player){
					shieldTarget.GetComponent<ManageHP>().SetBar(_shields);
				}
				lastRecharge = Time.time;
			}

			if((timeSinceLD >= 10)&&(_health != 45)){ //active healing once active
				if(healing == true){ //healing since last heal tick
					_health += (Time.time - lastHeal) * 9f;

				} else { //healing since the heal initialized
					_shields += (Time.time - lastDamage - 10f) * 9f;
					healing = true;
				}

				if(_health > 45){ //catch health overflow
					_health = 45f;
					//Debug.Log(gameObject.name +" regened to "+ _health);
				}

				if(player){
					healthTarget.GetComponent<ManageHP>().SetBar(_health);
				}
				lastHeal = Time.time;
			}
		}
	}

	public void Hurt(float damage, GameObject source){ //damage receiving for rockets and explosions
		if(gameObject.activeSelf){
			_shields -= damage;

			lastDamage = Time.time;
			recharging = false;
			healing = false;

			if(_shields <= 0){ //bleedthrough damage
				_health += _shields;
				_shields = 0;
			}

			if(player){
				shieldTarget.GetComponent<ManageHP>().SetBar(_shields);
				healthTarget.GetComponent<ManageHP>().SetBar(_health);
			}

			//Debug.Log(gameObject.tag+"- Health: "+_health+" Shields: "+_shields + " from "+source.name);

			if(_health <= 0){ //kill player entity
				//Grant +1 score to killer (left), +1 death to victim (right)
				_slayer.BoardUpdate(source.tag, gameObject.tag);

				_respawner.RespawnMe(gameObject);
				Reset();
				
				gameObject.SetActive(false);
				
			}
		} //end of if active
	}

	public float GetShields(){
		return _shields;
	}

	public float GetHealth(){
		return _health;
	}

	private void Reset(){ //called when respawned
		_health = 45f;
		_shields = 70f;

		if(player){
			shieldTarget.GetComponent<ManageHP>().SetBar(_shields);
			healthTarget.GetComponent<ManageHP>().SetBar(_health);
		}

		lastDamage = Time.time;
	}
}
