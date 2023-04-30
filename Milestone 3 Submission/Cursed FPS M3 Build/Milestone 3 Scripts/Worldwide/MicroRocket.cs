using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//RESTORED (implemented code sent to professor for a question and adjusted for changes)

//based upon code from https://answers.unity.com/questions/1658741/how-to-fire-projectile-in-direction-character-is-f.html

public class MicroRocket : MonoBehaviour{
    public int speed = 40 * 2; //according to Halopedia, the rocket travels 40 m/s
    public int lifetime = 10; //according to Halopedia (400m range divided 40 m/s)

    [SerializeField] private GameObject explosionPrefab;
    private GameObject creator; //whoever is firing the rocket
    
    private Rigidbody rocket_rigid;
    private float _travelTime; //how long rocket has been alive
    private float _createTime; //when rocket was made

    private Vector3 lastPosition;
    private Vector3 LastAngularVelocity;
    
    void Awake(){
        rocket_rigid = GetComponent<Rigidbody>();
    }

    void Start(){
        _createTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    void Update(){
        _travelTime = Time.time - _createTime;

        //deactive "ignore creator" after time passes
        /*if(_travelTime > 0.1){
            Physics.IgnoreCollision(GetComponent<Collider>(), creator.GetComponent<Collider>(), false);
        }*/
    }

    void OnCollisionEnter(Collision collision) {
		PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();

        ContactPoint contact = collision.contacts[0];

        //   if hitting an object not to be ignored and isn't creator       if hitting creator well after creation
        if(((collision.gameObject.tag != "GunIgnore")&&(collision.gameObject.tag != creator.tag))||((_travelTime > 0.1)&&(collision.gameObject.tag == creator.tag))){
        //impact if criteria is met

            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;

            Debug.Log("Travel: "+_travelTime +" Hit: "+collision.gameObject.tag);

            if (player != null) { //if hitting a player entity
                player.Hurt(15, creator); //deal 15 damage for a direct hit

            }

            GameObject explosion = Instantiate(explosionPrefab, pos, rot) as GameObject;
            explosion.GetComponent<RocketBoom>().GetCreator(creator);
            //Instantiate(explosionPrefab, pos, rot);
            Destroy(this.gameObject);

        //if impacting user by mistake, announce
        } else if(((_travelTime < 0.1)&&(collision.gameObject.tag == creator.tag))){
            Debug.Log("Hit user");
        }
	}

    public void CreatorIdentity(GameObject source){
        creator = source.transform.parent.gameObject;
        
        Debug.Log("Creator (rocket end): "+creator.tag);
    }
}
