using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBoom : MonoBehaviour
{
	GameObject creator;

    void Awake(){
        Destroy(this.gameObject, 0.2f); //destroy quickly after creation
    }

    void OnTriggerEnter(Collider other) {
		PlayerStats player = other.GetComponent<PlayerStats>();

		if (player != null) {
			player.Hurt(35, creator); //player receives 35 explosion damage
		}
	}

	public void GetCreator(GameObject source){
		creator = source;
	}
}
