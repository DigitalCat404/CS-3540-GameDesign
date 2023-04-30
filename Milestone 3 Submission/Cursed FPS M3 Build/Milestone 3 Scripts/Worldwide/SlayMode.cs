using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlayMode : MonoBehaviour{
    private UIManager UI_Manager;    

    private int[] player1 = new int[2]; //0- Kills, 1- Deaths
    private int[] player2 = new int[2]; //0- Kills, 1- Deaths
    private int[] player3 = new int[2]; //0- Kills, 1- Deaths
    private int[] player4 = new int[2]; //0- Kills, 1- Deaths

    private string megaString;

    // Start is called before the first frame update
    void Start()
    {
        UI_Manager = GameObject.Find("HUD Canvas").GetComponent<UIManager>();

        for(int i = 0; i < player1.Length; i++){ //initialize counters
            player1[i] = 0;
            player2[i] = 0;
            player3[i] = 0;
            player4[i] = 0;
        }

        BoardUpdate("","");
    }

    public void BoardUpdate(string killer, string victim){
        Debug.Log(killer+" "+victim);
        int shooter = 0;
        int receiver = 0;
        int points = 0;
        int shootDeaths = 0;
        int victPoints = 0;
        int deaths = 0;
        int scoreMod = 1;

        if(killer == victim){ //if a suicide
            scoreMod = -1;
        } else if(victim == "MONEY"){
            scoreMod = 15;
        }

        if(killer == "Player"){
            player1[0] += scoreMod;
            points = player1[0];
            shooter = 1;
            shootDeaths = player1[1];
        } else if(killer == "Player 2"){
            player2[0] += scoreMod;
            points = player2[0];
            shooter = 2;
            shootDeaths = player2[1];
        } else if(killer == "Player 3"){
            player3[0] += scoreMod;
            points = player3[0];
            shooter = 3;
            shootDeaths = player3[1];
        } else if(killer == "Player 4"){
            player4[0] += scoreMod;
            points = player4[0];
            shooter = 4;
            shootDeaths = player4[1];
        }

        if(victim == "Player"){
            player1[1] += scoreMod;
            deaths = player1[1];
            receiver = 1;
            victPoints = player1[0];
        } else if(victim == "Player 2"){
            player2[1] += scoreMod;
            deaths = player2[1];
            receiver = 2;
            victPoints = player2[0];
        } else if(victim == "Player 3"){
            player3[1] += scoreMod;
            deaths = player3[1];
            receiver = 3;
            victPoints = player3[0];
        } else if(victim == "Player 4"){
            player4[1] += scoreMod;
            deaths = player4[1];
            receiver = 4;
            victPoints = player4[0];
        }

        if(shooter != 0){
            if(shooter == 1){ killer = "Player 1"; }
            if(receiver == 1){ victim = "Player 1"; }

            if(victim == "MONEY"){
                UI_Manager.UpdateAnnouncements(killer +" made bank!");
            } else {
                UI_Manager.UpdateAnnouncements(killer +" defeated "+ victim);
            }
        }

        UI_Manager.UpdateBoard(shooter, points, shootDeaths, receiver, victPoints, deaths);

        if(player1[0] >= 15){
            UI_Manager.Victory("Player 1- Green");
        } else if(player2[0] >= 15){
            UI_Manager.Victory("Player 2- Blue");
        } else if(player3[0] >= 15){
            UI_Manager.Victory("Player 3- Red");
        } else if(player4[0] >= 15){
            UI_Manager.Victory("Player 4- Gold");
        }
        
    }
}
