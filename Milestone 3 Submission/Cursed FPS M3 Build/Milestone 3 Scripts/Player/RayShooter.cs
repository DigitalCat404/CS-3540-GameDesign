using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

//RESTORED (taken from Milestone 2 implementation and updated according to M3's WanderingAI)

public class RayShooter : MonoBehaviour {
	private Camera _camera;

	public int textX = 5;
	public int textY = 0;
	public int textWidth = 200;
	public int textHeight = 18;

	//outside objects
	[SerializeField] private GameObject markerPrefab;
	[SerializeField] private GameObject rocketPrefab;
	private GameObject marker;
	private UIManager UI_Manager;

	//weapon variables
	private bool life; //if true, allow action. Else, don't
	private int weapon = 1;
	private bool shooting = false; //if trigger is pulled and thus weapon is active
	private bool cooldown = false; //rocket launcher refire timer
	private float lastShot = 0; //tracks last time gun was fired
	private float temp; //holds the difference between lastShot and current time

	private float burstRate = 1 / (2.4f * 2); //battle rifle fires 2.4 bursts per second originally
	private float rocketRate = 0.4f; //The real rocket launcher has a 0.6 second delay
	private int _burst = 0; //counter for bullets fired from Burst Rifle
	private float deviation;
	private float angle;

	RaycastHit hit;

	void Start() {
		life = false;
		_camera = GetComponent<Camera>();
		UI_Manager = GameObject.Find("HUD Canvas").GetComponent<UIManager>();
		UI_Manager.SetReticle("x");
	}

	void Update() {	
		//weapon swap check
		/*if(life){*/

		if((Input.GetKey(KeyCode.Alpha1))&&!(shooting)){
			weapon = 1;
			UI_Manager.SetReticle("x");
		} else if((Input.GetKey(KeyCode.Alpha2))&&!(shooting)){
			weapon = 2;
			UI_Manager.SetReticle("o");
		} else if(Input.GetKeyDown(KeyCode.Escape)){
			Application.Quit();
			Debug.Log("Quit out");
		}
		
		temp = Time.time - lastShot; //time since last shot

		if(temp >= rocketRate){ //end firing cooldown on micro-rocket if time has passed
			cooldown = false;
		}

		//trigger pull check
		if (Input.GetMouseButtonDown(0)&&!(shooting)) { //if pulling trigger and weapon is not shooting
			shooting = true;

			if(weapon == 1){ //prevent oddities with firing burst rifle
				temp = 0;

			} else if((weapon == 2)&&(cooldown)){ //if micro-rocket is on cooldown, don't shoot
				shooting = false;
				//play click noise!
			}
		}
		

		if((weapon == 1)&&(shooting)){ //if firing Burst Rifle
			//based upon code from https://answers.unity.com/questions/1553903/raycast-bullet-spread-c-1.html
			//with reference to https://answers.unity.com/questions/1143243/rapid-fire-how-to-accurately-wait-for-a-very-short.html

			Vector3 forwardVector = Vector3.forward;

			for(float i = 0; i <= temp; i += burstRate){ //fire a shot for every "missed" projectile from last shot
				if(_burst == 108){ //if at end of burst, return gun to inactive and reset burst
					shooting = false;
					_burst = 0;
					break;
				} else {
					_burst++;

					deviation = Random.Range(0f, _burst/25); //deviation determined by current bullet count
					angle = Random.Range(0f, 360f);

					forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
					forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;
					forwardVector = transform.rotation * forwardVector;

					Debug.DrawRay(Vector3.forward, forwardVector, Color.black);
					if (Physics.Raycast(transform.position, forwardVector, out hit)) {
						GameObject hitObject = hit.transform.gameObject;

						//if hitting a player that isn't yourself, deal damage
						if((hitObject.GetComponent<PlayerStats>())&&(hitObject.tag != "Player")){
							hitObject.GetComponent<PlayerStats>().Hurt(6, gameObject.transform.parent.gameObject);
						
						//else, if not hitting a bullet hole, rocket or gun pointer, draw hole
						} else if((hitObject.tag != "Bullet Hole")&&(hitObject.tag != "Rocket")&&(hitObject.tag != "Player")){
							//Debug.Log("Hit: "+hitObject.name);
							StartCoroutine(HitIndicator(hit.point, hit));
						}
					}
				}
			}
			lastShot = Time.time;


		} else if((weapon == 2)&&(shooting)&&(!cooldown)){ //if firing micro-rockets and cooldown is not active
			GameObject rocket = Instantiate(rocketPrefab) as GameObject;

			rocket.GetComponent<MicroRocket>().CreatorIdentity(gameObject);
			//Debug.Log("Creator (user end): "+gameObject.transform.parent.gameObject.tag);
			
			rocket.transform.position = transform.TransformPoint(Vector3.forward * 1f);

			Rigidbody rocketBody = rocket.GetComponent<Rigidbody>();
			rocketBody.velocity = Camera.main.transform.forward * rocket.GetComponent<MicroRocket>().speed;
			rocketBody.transform.rotation = Quaternion.LookRotation(rocketBody.velocity);

			cooldown = true;
			lastShot = Time.time;
			shooting = false;
		}
		/*} //end of if-alive*/
	}

	private IEnumerator HitIndicator(Vector3 pos, RaycastHit hit) { //creates indicator of hit on target
		GameObject marker = Instantiate(markerPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));

		yield return new WaitForSeconds(3);
		Destroy(marker);
	}

	public void SetLife(bool state){ //engage or disengage player action
		life = state;
	}
}