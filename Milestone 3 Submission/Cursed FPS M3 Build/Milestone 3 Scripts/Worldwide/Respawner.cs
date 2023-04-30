using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    public GameObject[] respawns;

    private UIManager UI_Manager;

    public bool[] cooldowns;

    private int respawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        respawns = GameObject.FindGameObjectsWithTag("Respawn");
        UI_Manager = GameObject.Find("HUD Canvas").GetComponent<UIManager>();
        cooldowns = new bool[respawns.Length];
    }

    public void RespawnMe(GameObject entity){ //respawns AI entity that died
        respawnPoint = Random.Range(0, respawns.Length - 1);

        respawnPoint = CooldownCheck(respawnPoint);

        if(entity.tag == "Player"){
            UI_Manager.DeathOn();
            StartCoroutine(RespawnPlayer(entity, respawnPoint));
        } else {
            StartCoroutine(RespawnAI(entity, respawnPoint));
        }
    }

    private IEnumerator RespawnPlayer(GameObject entity, int rPoint){ //return player to life in 3 seconds
        yield return new WaitForSeconds(3);
        UI_Manager.DeathOff();
        entity.SetActive(true);
        entity.transform.position = respawns[respawnPoint].transform.position;
        entity.transform.rotation = respawns[respawnPoint].transform.rotation;

        Debug.Log("Respawned " + entity.name + " at "+ respawns[respawnPoint].name);
    }

    private IEnumerator RespawnAI(GameObject entity, int rPoint){ //return entity to life in 3 seconds
		yield return new WaitForSeconds(3);
		entity.SetActive(true);

        entity.transform.position = respawns[respawnPoint].transform.position;
        entity.transform.rotation = respawns[respawnPoint].transform.rotation;

        Debug.Log("Respawned " + entity.name + " at "+ respawns[respawnPoint].name);
	}

    private int CooldownCheck(int rPoint){ //prevents reuse of spawn location for short period of time
        if(cooldowns[rPoint] == true){ //if selected rPoint is on cooldown, adjust rPoint

            if(rPoint >= (cooldowns.Length) / 2){ //if in the upper half of rPoint list, check down for an open spawnpoint
                for(int i = rPoint; i > 0; i--){
                    if(cooldowns[i] != true){
                        StartCoroutine(enterCooldown(i));
                        return i;
                    }
                }

            } else { //if in the bottom half of rPoint list, check up for an open spawnpoint
                for(int i = rPoint; i < cooldowns.Length; i++){
                    if(cooldowns[i] != true){
                        StartCoroutine(enterCooldown(i));
                        return i;
                    }
                }
            }

            return 0; //emergency case
                
        } else { //if rPoint is not on cooldown
            StartCoroutine(enterCooldown(rPoint));
            return rPoint;
        }
    }

    private IEnumerator enterCooldown(int rPoint){
        cooldowns[rPoint] = true;
        yield return new WaitForSeconds(3);
        cooldowns[rPoint] = false;
    }
}
