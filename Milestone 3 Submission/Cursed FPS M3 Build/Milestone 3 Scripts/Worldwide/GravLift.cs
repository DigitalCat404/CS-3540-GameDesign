using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//RESTORED (implemented Milestone 2 code then updated to Milestone 3's state)

public class GravLift : MonoBehaviour{

    void OnTriggerEnter(Collider other){
        FPSInput FPSInput = other.GetComponent<FPSInput>();
        WanderingAI AI = other.GetComponent<WanderingAI>();

        if(FPSInput != null){
            FPSInput.LiftOn();
        }
        if(AI != null){
            AI.LiftOn();
        }
    }

    void OnTriggerExit(Collider other){
        FPSInput FPSInput = other.GetComponent<FPSInput>();
        WanderingAI AI = other.GetComponent<WanderingAI>();

        if(FPSInput != null){
            FPSInput.LiftOff();
        }
        if(AI != null){
            AI.LiftOff();
        }
    }
}
