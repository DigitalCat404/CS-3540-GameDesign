using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//RESTORED (from creator memory)

public class LiftGuide : MonoBehaviour
{
    void OnTriggerEnter(Collider entity){
        WanderingAI AI = entity.GetComponent<WanderingAI>();

        if(AI != null){
            // cooldown to prevent lift use loops      and only if grounded
            if((Time.time - AI.giveLastLift() > 15)&&(AI.giveGrounded())){ 
                AI.PointTo(gameObject.transform.parent.transform.position);
            }
        }
    }
}
