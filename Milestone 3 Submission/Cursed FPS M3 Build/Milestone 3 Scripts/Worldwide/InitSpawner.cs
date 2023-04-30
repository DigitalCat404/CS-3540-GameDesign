using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//RESTORED (from creator's memory)

public class InitSpawner : MonoBehaviour
{
    GameObject target;

    void Awake()
    {
        switch(gameObject.name){
            case "Initial Spawnpoint- Green":
                target = GameObject.Find("Player 1- Green");
                
                if(target != null){
                    target.transform.position = gameObject.transform.position;
                    target.transform.rotation = gameObject.transform.rotation;
                    target.SetActive(false);
                }
            break;

            case "Initial Spawnpoint- Blue":
                target = GameObject.Find("Player 2- Blue");
                
                if(target != null){
                    target.transform.position = gameObject.transform.position;
                    target.transform.rotation = gameObject.transform.rotation;
                    target.SetActive(false);
                }
            break;

            case "Initial Spawnpoint- Red":
                target = GameObject.Find("Player 3- Red");
                
                if(target != null){
                    target.transform.position = gameObject.transform.position;
                    target.transform.rotation = gameObject.transform.rotation;
                    target.SetActive(false);
                }
            break;

            case "Initial Spawnpoint- Gold":
                target = GameObject.Find("Player 4- Gold");
                
                if(target != null){
                    target.transform.position = gameObject.transform.position;
                    target.transform.rotation = gameObject.transform.rotation;
                    target.SetActive(false);
                }
            break;
        }
    }
}
