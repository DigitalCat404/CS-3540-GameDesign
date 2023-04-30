using UnityEngine;
using System.Collections;

//RESTORED (reusing old code from previous Milestone submission)

// basic WASD-style movement control

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPS Input")]
public class FPSInput : MonoBehaviour {

	//one 3ds max unit from Halo 2 = 0.03048 meters in Unity
	//1 world unit = 100 3ds max units = 10 feet
	//therefore 1 world unit = 3.048 meters in Unity

	public float speed = 6.858f; //2.25 wups (forward) Halo 2 movement
	//Halo 2 player movement is 2.0 wups in any other direction, not implemented (yet?)

	//max jump height is 80 3ds max units, or 2.4384(? very short) meters in Unity
	public float jumpSpeed = 12.0799f; //found through testing
	public float gravity = -18.2777f; //found through testing
	public float termVel = -300.0f; //temp val, "unused" in map design

	private float _vertSpeed;
	private bool grounded = false; //checks if on the ground
	private bool _gLift = false; //checks if in a gravity lift

	private CharacterController _charController;
	private ControllerColliderHit _contact;
	//public UIController _UIcontrol;

	RaycastHit hit;

	void Start() {
		_charController = GetComponent<CharacterController>();
	}
	
	void Update() {
		float deltaX = Input.GetAxis("Horizontal") * speed;
		float deltaZ = Input.GetAxis("Vertical") * speed;
		Vector3 movement = new Vector3(deltaX, 0, deltaZ);

		//aerial effects
		grounded = false;

			//check if falling
		//known bug: can't jump on slopes sometimes?
		if(_vertSpeed < 0 && Physics.Raycast(transform.position, Vector3.down, out hit)){
			float check = 3.0f; //value that checks distance from ground
			grounded = hit.distance <= check;
		}
			//check if grounded and not standing in a lift
		if((grounded)&&(_gLift == false)){
			if(Input.GetKey(KeyCode.Space)){
				_vertSpeed = jumpSpeed;

			} else { //stop gravity when grounded
				_vertSpeed = 0;
			}

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

			if(_charController.isGrounded){ //if grounded but raycasting says not, nudge
				//known bug: can get stuck on certain edges for some reason?
				//known bug: slides player backwards on slopes?
				if(Vector3.Dot(movement, _contact.normal) < 0){ //nudge depending on facing direction
					movement = _contact.normal * speed;
				} else {
					movement += _contact.normal * speed;
				}
			}
		}
		movement.y = _vertSpeed;

			//need to find an alternative to this, clamps Y axis too
		//movement = Vector3.ClampMagnitude(movement, speed);
		
		movement *= Time.deltaTime;
		movement = transform.TransformDirection(movement);
		_charController.Move(movement);
	}

	void OnControllerColliderHit(ControllerColliderHit hit){
		_contact = hit;
	}

	public void LiftOn(){ //engages lift gravity
		_gLift = true;
	}

	public void LiftOff(){ //disengages lift gravity
		_gLift = false;
	}
}