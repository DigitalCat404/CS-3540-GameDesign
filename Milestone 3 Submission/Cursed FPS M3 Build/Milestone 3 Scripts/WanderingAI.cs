using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class WanderingAI : MonoBehaviour {
	private GameObject _camera; //fake camera for placing bullets

	//core behavior
	public float speed = 6.858f;
	public float gravity = -18.2777f;
	public float termVel = -300.0f;
	public float setDistance = 60f; //maximum player detection distance
	public float maxDistance; //current detection range

	//handling gravity and gravity lifts
	private float _vertSpeed;
	private bool grounded = false; //checks if on the ground
	private bool _gLift = false; //checks if in a gravity lift
	private float actLiftHeight; //checks height AI enter gLift in
	private float lastLift = -15f; //used to track when to start "bouncing" off objects again

	public float obstacleRange = 6.858f / 2.5f; //max distance from wall before changing direction
	
	//outside objects required
	[SerializeField] private GameObject markerPrefab;
	[SerializeField] private GameObject rocketPrefab;

	private GameObject[] allPlayers = new GameObject[4];
	private GameObject[] enemyPlayers = new GameObject[3];
	private GameObject curTarget;
	private GameObject aggressor;

	private Vector3 liftTarget; //AI pathing node for entering lifts
	private Vector3 lookAtMe; //the coordinates of where AI is being directed

	//weapon variables
	private bool shooting = false; //if trigger is pulled and thus weapon is active
	private bool clamp = false; //if clamped, AI does not move
	private bool cooldown = false; //rocket launcher refire timer
	private float lastShot = 0; //tracks last time gun was fired
	private float temp; //holds the difference between lastShot and current time
	private int weapon = 1; //1 = burst rifle, 2 = rocket launcher

	private float burstRate = 1 / (2.4f * 2); //battle rifle fires 2.4 bursts per second originally
	private float rocketRate = 0.6f; //slowed compared to player for less aggression
	private int _burst = 0; //counter for bullets fired from Burst Rifle
	private float deviation;
	private float angle;

	private float FoV = 80f; //player's Field of View angle
	
	//AI states
	private bool life = true; //if allowed to be active
	private string state; //current state of AI
		//states: 'patrol' to roam, 'engage' when attacking, 'directed' when moving to specified point

	//local objects
	private CharacterController _aiController; //itself
	private ControllerColliderHit _contact;  //for on-edge nudging

	RaycastHit hit;
	RaycastHit floor;

	void Start() {
		state = "spawn";
		obstacleRange = speed / 2.25f;
		weapon = Random.Range(1,4);
		if(weapon >= 3){
			weapon = 2;
		} else {
			weapon = 1;
		}

		_camera = transform.Find("Camera").gameObject;
		_aiController = GetComponent<CharacterController>();
		liftTarget = GameObject.Find("LiftTarget").transform.position;

		//find all players and add them to list
		allPlayers[0] = GameObject.Find("Player 1- Green");
		allPlayers[1] = GameObject.Find("Player 2- Blue");
		allPlayers[2] = GameObject.Find("Player 3- Red");
		allPlayers[3] = GameObject.Find("Player 4- Gold");

		int j = 0;
		//identify who "myself" is and make them null
		for(int i = 0; i < allPlayers.Length; i++){
			if(allPlayers[i] != gameObject){
				enemyPlayers[j] = allPlayers[i];
				j++;
			} /*else {
				Debug.Log(gameObject.tag+" is: "+allPlayers[i].tag);
			}*/
		}
	}
	
	void Update() {
		if(life) {
			Vector3 movement;

			if(weapon == 1){
				maxDistance = setDistance / 3;
			} else {
				maxDistance = setDistance;
			}

				//check if grounded or falling
			grounded = false;
			if(_vertSpeed < 0 && Physics.Raycast(transform.position, Vector3.down, out floor)){
				float check = 3.0f; //value that checks distance from ground
				grounded = floor.distance <= check;
			}

			movement = new Vector3(0,0,speed);
			movement.y = VerticalMove();

				//need to find an alternative to this, clamps Y axis too
			//movement = Vector3.ClampMagnitude(movement, speed);

			//do the movement thing
			movement *= Time.deltaTime;

			if((clamp)&&(grounded)){ //if clamped and on the ground, stay still
				movement.x = -movement.x;
				movement.z = -movement.z;
				//Debug.Log("Clamping");

				if(weapon == 2){ //reset clamp
					clamp = false;
				}
			}

			movement = transform.TransformDirection(movement);
			_aiController.Move(movement);

			if((_gLift == true)&&(actLiftHeight < liftTarget.y)){
				//rotate towards center to use gravity lift IF below second floor
				Direction(liftTarget);
			}
			
			if(state == "spawn"){ //set random spawn direction before resetting AI state
				transform.Rotate(0, Random.Range(-50, 50), 0);
				state = "patrol";
			}

			//check if player(s) in sightline, change state accordingly
			state = WatchPlayers();

			switch(state){
				case "directed": //if being guided by an AI cube
					Direction(lookAtMe);
				break;

				case "engage": //do follow-and-attack behavior
					Engage();
					Shoot();
				break;

				case "react": //look at source of damage received
					Direction(aggressor.transform.position);
				break;

				default: //when in doubt, patrol
					_camera.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
					PathingRotation();
				break;
			} //end of switch-state
		} //end of if-Alive
	}


	public void SetLife(bool status){ //enables setting of alive state
		life = status;
	}

	public void Reset(){ //tells AI to reset to spawn-in behavior
		state = "spawn";
		weapon = Random.Range(1,4);
		if(weapon >= 3){
			weapon = 2;
		} else {
			weapon = 1;
		}
	}

	public void LiftOn(){ //engages lift gravity
		_gLift = true;
		actLiftHeight = transform.position.y;

		//if below threshold where AI wants to follow lift, and not on cooldown for shooting rocket
		if(actLiftHeight < liftTarget.y){
			state = "directed";
			lookAtMe = liftTarget;
		}
	}

	public void LiftOff(){ //disengages lift gravity
		if(state != "engage"){
			state = "patrol";

			//give random exit direction
			transform.Rotate(0, Random.Range(-30, 30), 0);
		}
		
		_gLift = false;
		lastLift = Time.time;
	}

	public void PointTo(Vector3 spot){ //enables AI to redirect towards point
		lookAtMe = spot;
		state = "directed";
	}

	public float giveLastLift(){ //prints when last grav lift was used
		return lastLift;
	}

	public bool giveGrounded(){ //prints state of AI's grounded variable
		return grounded;
	}


	private float VerticalMove(){ //handles vertical movement, including Gravity Lift state
			//check if grounded and not standing in a lift
		if((grounded)&&(_gLift == false)){
				//if implementing AI jump, put it here

				//stop gravity when grounded
				_vertSpeed = 0;

		} else if(_gLift == true) { //if standing in a lift, reverse gravity
			_vertSpeed += -gravity * Time.deltaTime * 2;

			if(_vertSpeed > -gravity * 0.75f){ //set cap to upwards velocity
				_vertSpeed = -gravity * 0.75f;
			}

		} else { //if not grounded and not in a lift, apply gravity
			_vertSpeed += gravity * Time.deltaTime;

			if(_vertSpeed < termVel){ //set cap to falling speed
				_vertSpeed = termVel;
			}
		}

		return _vertSpeed;
	}

	private string WatchPlayers(){ //tracks player positions and sees if any can be hit
		//don't mess with gravLift use or engaging an enemy in Burst Rifle state
		if((state != "directed")&&(Time.time - lastLift > 0.5)&&((!(_burst > 0)||!(_burst < 108)))){
			//based upon code from https://answers.unity.com/questions/474145/shooting-a-ray-at-an-object.html
			//with reference to https://answers.unity.com/questions/1130557/raycast-using-euler-angle.html

			Ray[] shots = new Ray[3]; //track where all other players are
			RaycastHit rayHit;
			Vector3 targetDir;
			float targetsAngle;
			float closestTarget = 1000f;

			//shows max distance AI can see when not targeting
			if(!curTarget){
				//Debug.DrawRay(_camera.transform.position, _camera.transform.forward * maxDistance, Color.yellow);
			}

			//show leftmost angle AI can look
			//Debug.DrawRay(transform.position, direction * maxDistance, Color.yellow);
			//show rightmost angle AI can look


			//watch all enemies and check for who to engage
			for(int i = 0; i < enemyPlayers.Length; i++){
				targetDir = enemyPlayers[i].transform.position - _camera.transform.position;

				//fire ray towards a player
				//Debug.DrawRay(_camera.transform.position, targetDir, Color.blue);
				shots[i] = new Ray(_camera.transform.position, targetDir);

				//find angle of ray
				targetsAngle = Vector3.Angle(targetDir, transform.forward);
				//if(i == 0){Debug.Log(enemyPlayers[i].tag+" angle: "+angle);}


				//See if that player is close and in sight
				if(Physics.Raycast(shots[i], out rayHit, maxDistance)){
					//	if seeing a player							in Field of View		is closer than another target
					if((rayHit.collider.GetComponent<PlayerStats>())&&(targetsAngle <= FoV/2)&&(rayHit.distance < closestTarget)){ 
						curTarget = enemyPlayers[i];
					}
				}

				if(curTarget != null){
					//Debug.Log("Target: "+curTarget.name);

					if((weapon == 2)&&(rayHit.distance < 10f)){
						clamp = true;
					}
					return "engage";
				}
			}
		} //end of if-not to be messed with
		
		return state; //when in doubt, don't modify state
	}

	private void PathingRotation(){ //calculates basic pathing rotation
		Ray ray = new Ray(transform.position, transform.forward);
			if (Physics.SphereCast(ray, 0.75f, out hit)) {
				GameObject hitObject = hit.transform.gameObject;
				//Debug.DrawRay(transform.position, transform.forward * obstacleRange, Color.green);

				if ((hit.distance < obstacleRange)&&(hitObject.tag != "Slope")&&(_gLift == false)&&(Time.time - lastLift > 0.15)) { //if to bounce off wall
					float angle = Random.Range(-110, 110);
					transform.Rotate(0, angle, 0);
				}
			}
	}

	private void Direction(Vector3 spot){ //directs AI to rotate towards spot (no vertical)
		Vector3 goHere = new Vector3(spot.x, transform.position.y, spot.z);
		transform.LookAt(goHere);
	}

	private void Engage(){ //trigger pull mechanics
		//point towards the selected target
		Direction(curTarget.transform.position);
		_camera.transform.LookAt(curTarget.transform.position);

		temp = Time.time - lastShot; //time since last shot

		if(temp >= rocketRate){ //end firing cooldown on micro-rocket if time has passed
			cooldown = false;
		}

		//trigger pull check
		if (!shooting) { //if pulling trigger and weapon is not shooting
			shooting = true;

			if(weapon == 1){ //prevent oddities with firing burst rifle
				temp = 0;
				clamp = true;

			} else if((weapon == 2)&&(cooldown)){ //if micro-rocket is on cooldown, don't shoot
				shooting = false;
			}
		}
	}

	private void Shoot(){ //deciphers what weapon to shoot
		if((weapon == 1)&&(shooting)){ //shoot or continue to shoot Burst Rifle
			BurstRifle();

		} else if((weapon == 2)&&(shooting)&&(!cooldown)){ //if firing cooldown is not active, shoot
			microRocket();
		}
	}

	private void BurstRifle(){ //shoot the Burst Rifle, AI style
		//based upon code from https://answers.unity.com/questions/1553903/raycast-bullet-spread-c-1.html
		//with reference to https://answers.unity.com/questions/1143243/rapid-fire-how-to-accurately-wait-for-a-very-short.html

			RaycastHit impact;
			Vector3 direction = curTarget.transform.position - _camera.transform.position;
			Vector3 forwardVector = _camera.transform.forward;

			for(float i = 0; i <= temp; i += burstRate){ //fire a shot for every "missed" projectile from last shot
				if(_burst == 108){ //if at end of burst, return gun to inactive and reset burst
					Debug.Log("Resetting BR");
					curTarget = null;
					shooting = false;
					state = "patrol";
					clamp = false;
					_burst = 0;
					break;
				} else {
					_burst++;

					//intended bullet route
					//Debug.DrawRay(_camera.transform.position, direction, Color.red);

					//AI's starting spread is larger to simulate inaccuracy
					deviation = Mathf.Max(1f,Random.Range(0f, _burst/25)); //deviation determined by current bullet count
					angle = Random.Range(0f, 360f);

					forwardVector = Quaternion.AngleAxis(deviation, _camera.transform.up) * forwardVector;
					forwardVector = Quaternion.AngleAxis(angle, _camera.transform.forward) * forwardVector;
					//forwardVector = _camera.transform.rotation * forwardVector; breaks it for some reason??

					//actual route
					Debug.DrawRay(_camera.transform.position, forwardVector * Vector3.Distance(curTarget.transform.position,_camera.transform.position), Color.black);
					if (Physics.Raycast(_camera.transform.position, forwardVector.normalized, out impact)) {
						GameObject hitObject = impact.transform.gameObject;
						//Debug.Log(hitObject.name);

						//if hitting a player that isn't yourself, deal damage
						if((hitObject.GetComponent<PlayerStats>())&&(hitObject.tag != gameObject.tag)){
							hitObject.GetComponent<PlayerStats>().Hurt(6, gameObject);
							Debug.Log("Direct hit");

						//else if hitting a player's pointer, deal damage
						} else if((hitObject.name == "Pointer")){
							hitObject.transform.parent.GetComponent<PlayerStats>().Hurt(6, gameObject);
							Debug.Log("Pointer hit");

						//else, if not hitting a bullet hole, rocket or gun pointer, draw hole
						} else if((hitObject.tag != "Bullet Hole")&&(hitObject.tag != "Rocket")){
							StartCoroutine(HitIndicator(impact.point, impact));
							Debug.Log("Bullet holed");
						} /*temp debug whiff check else {
							Debug.Log("Whiff");
						}*/
					}
				}
			}
			lastShot = Time.time;
	}

	private void microRocket(){ //shoot the Micro Rocket, AI style
			GameObject rocket = Instantiate(rocketPrefab) as GameObject;
			//Physics.IgnoreCollision(rocket.GetComponent<Collider>(), gameObject.GetComponent<Collider>());
			Physics.IgnoreCollision(rocket.GetComponent<Collider>(), _aiController);

			rocket.GetComponent<MicroRocket>().CreatorIdentity(_camera.gameObject);
			//Debug.Log("Creator (user end): "+_camera.transform.parent.gameObject.tag);
			
			//intended rocket direction
			//Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 10, Color.red);

			rocket.transform.position = _camera.transform.position;

			Rigidbody rocketBody = rocket.GetComponent<Rigidbody>();
			rocketBody.velocity = _camera.transform.forward * rocket.GetComponent<MicroRocket>().speed;
			rocketBody.transform.rotation = Quaternion.LookRotation(rocketBody.velocity);

			cooldown = true;
			lastShot = Time.time;
			shooting = false;
			curTarget = null;
			state = "patrol";
	}

	private IEnumerator HitIndicator(Vector3 pos, RaycastHit hit) { //creates indicator of hit on impact
		GameObject marker = Instantiate(markerPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));

		yield return new WaitForSeconds(3);
		Destroy(marker);
	}
}
