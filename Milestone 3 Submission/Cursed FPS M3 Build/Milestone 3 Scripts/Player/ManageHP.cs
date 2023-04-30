using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManageHP : MonoBehaviour{
    private float shieldHealth; //amount of shields/health to factor with
    public Slider Bar;

    void Start(){
        Bar = GetComponent<Slider>();
        if(gameObject.name == "Shields"){
            shieldHealth = 70;
        } else {
            shieldHealth = 45;
        }
        Bar.maxValue = shieldHealth;
        Bar.value = shieldHealth;
    }

    public void SetBar(float curAmount){
        Bar.value = curAmount;
    }
}
