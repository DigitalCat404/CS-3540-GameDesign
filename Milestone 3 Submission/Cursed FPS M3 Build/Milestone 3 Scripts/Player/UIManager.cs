using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour{
    //HUD
    private GameObject Announcer;
    private GameObject Shields;
    private GameObject Health;
    private GameObject Reticle;
    private GameObject DeathCam;
    private GameObject OverloadText;
    public GameObject Scoreboard; //had to be set to public, could not find in Start() for some reason

    //main menu management
    private GameObject Title;
    private GameObject Play;
    private GameObject Quit;
    private GameObject menuCam;

    //player management
    private GameObject Player1;
    private GameObject Player2;
    private GameObject Player3;
    private GameObject Player4;
    public GameObject InitSpawns;

    //variables
    private bool menuUp = true; //if true, open main menu
    private string[] announces = new string[4];
    private string[] lines = new string[4];
    private float[] createTime = new float[4]; //time when announcement was made
    float temp; //holds change of time
    private string superString = ""; //combination of all strings in announces
    private float deathTime;
    private bool victory = false;

    private RayShooter rayShooter;

    // Start is called before the first frame update
    void Start(){
        Announcer = GameObject.Find("Announcer");
        Shields = GameObject.Find("Shields");
        Health = GameObject.Find("Health");
        Reticle = GameObject.Find("Reticle");
        Title = GameObject.Find("TitleCard");
        Play = GameObject.Find("Play Button");
        Quit = GameObject.Find("Quit Button");
        menuCam = GameObject.Find("Top-Down Camera");
        DeathCam = GameObject.Find("Death Cam");
        OverloadText = GameObject.Find("Overload Text");

        Player1 = GameObject.Find("Player 1- Green");
        Player2 = GameObject.Find("Player 2- Blue");
        Player3 = GameObject.Find("Player 3- Red");
        Player4 = GameObject.Find("Player 4- Gold");

        for(int i = 0; i < announces.Length; i++){
            announces[i] = "Spawning player "+(i+1).ToString()+"...\n";
            createTime[i] = Time.time;
            superString += announces[i];
        }
        Announcer.GetComponent<Text>().text = superString;

        rayShooter = GameObject.Find("Player Camera").GetComponent<RayShooter>();

        Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

        DeathCam.SetActive(false);
        InitSpawns.SetActive(true);
        
        HUDOff();
    }

    void Update(){
        temp = Time.time - createTime[0];

        //if at least 3 seconds passed since creation, clean announcements by one
        if((temp >= 5)&&(announces[0] != "")){
            //Debug.Log(announces[0]+ " is now removed");
            UpdateAnnouncements("");
        }

        //update death cam timer if true
        if(!victory){
            OverloadText.GetComponent<Text>().text = "";
        } else if((DeathCam.activeSelf)&&(deathTime != 0)){
            OverloadText.GetComponent<Text>().text = "You died! Respawning in... "+ (3 + deathTime - Time.time).ToString("0");
            deathTime = 0;
        }

        if((victory == true)&&(Input.GetKeyDown(KeyCode.Return))){
            Application.Quit();
            Debug.Log("Quit game");
        }

        //cheat codes
        if(!menuUp){
            if(Input.GetKeyDown(KeyCode.Tab)&&(!victory)){ //Press tab to instant win
                GameObject.Find("SlayerMode").GetComponent<SlayMode>().BoardUpdate("Player","MONEY");
                deathTime = 0;
            }
            if(Input.GetKeyDown(KeyCode.Backslash)&&(!victory)){
                GameObject.Find("SlayerMode").GetComponent<SlayMode>().BoardUpdate("Player "+Random.Range(2,4).ToString("0"),"MONEY");
            }

            if(Input.GetKeyDown(KeyCode.U)&&(Player1.activeSelf)){ //kill player 1
                Player1.GetComponent<PlayerStats>().Hurt(115, Player1);
            }
            if(Input.GetKeyDown(KeyCode.I)&&(Player1.activeSelf)){ //kill player 2
                Player2.GetComponent<PlayerStats>().Hurt(115, Player2);
            }
            if(Input.GetKeyDown(KeyCode.O)&&(Player1.activeSelf)){ //kill player 3
                Player3.GetComponent<PlayerStats>().Hurt(115, Player3);
            }
            if(Input.GetKeyDown(KeyCode.P)&&(Player1.activeSelf)){ //kill player 4
                Player4.GetComponent<PlayerStats>().Hurt(115, Player4);
            }
        }
    }

    public void HUDOff(){ //disables standard HUD completely
        Announcer.SetActive(false);
        Shields.SetActive(false);
        Health.SetActive(false);
        Reticle.SetActive(false);
        Scoreboard.SetActive(false);
    }

    public void HUDOn(){ //enables standard HUD completely
        Announcer.SetActive(true);
        Shields.SetActive(true);
        Health.SetActive(true);
        Reticle.SetActive(true);
        Scoreboard.SetActive(true);
    }

    public void ToggleMenu(){
        if(menuUp){
            Title.SetActive(false);
            Play.SetActive(false);
            Quit.SetActive(false);
            menuCam.SetActive(false);
            HUDOn();
            menuUp = false;
            Cursor.lockState = CursorLockMode.Locked;
		    Cursor.visible = false;
        } else {
            Title.SetActive(true);
            Play.SetActive(true);
            Quit.SetActive(true);
            menuCam.SetActive(true);
            HUDOff();
            menuUp = true;
            Cursor.lockState = CursorLockMode.None;
		    Cursor.visible = true;
        }
    }

    public void DeathOn(){ //disables active player HUD elements and sets death cam up
        deathTime = Time.time;
        Shields.SetActive(false);
        Health.SetActive(false);
        Reticle.SetActive(false);
        DeathCam.SetActive(true);
        DeathCam.transform.position = GameObject.Find("Player Camera").transform.position;
        DeathCam.transform.rotation = GameObject.Find("Player Camera").transform.rotation;
    }

    public void DeathOff(){ //enables active player HUD elements
        Shields.SetActive(true);
        Health.SetActive(true);
        Reticle.SetActive(true);
        DeathCam.SetActive(false);
    }

    public void SetReticle(string targeter){ //sets weapon reticle
        Reticle.GetComponent<Text>().text = targeter;
    }

    public void UpdateAnnouncements(string newString){
        newString += "\n";
        for(int i = 0; i < announces.Length; i++){
            if((announces[i] == "\n")&&(newString != "\n")){ //if empty, add to empty
                //Debug.Log(newString+" added to "+i.ToString());
                announces[i] = newString;
                createTime[i] = Time.time;
                break;

            } else if(i == 3){ //if on last and it's all full, move text "up"
                for(int j = 1; j < announces.Length; j++){
                    announces[j-1] = announces[j];
                    createTime[j-1] = createTime[j];
                }
                announces[3] = newString;
                createTime[3] = Time.time;
            }
        }

        superString = "";
        for(int i = 0; i < announces.Length; i++){
            if(announces[i] != ""){
                superString += announces[i];
            }
        }
        Announcer.GetComponent<Text>().text = superString;
    }

    public void UpdateBoard(int shooter, int points, int shootDeaths, int receiver, int recPoints, int deaths){
        if(shooter == 0){ //if being initialized
            lines[0] = "Player 1: Kills- 0  Deaths- 0\n";
            lines[1] = "Player 2: Kills- 0  Deaths- 0\n";
            lines[2] = "Player 3: Kills- 0  Deaths- 0\n";
            lines[3] = "Player 4: Kills- 0  Deaths- 0";
    
        } else if(shooter != 4){ //if not last
            lines[shooter-1] = "Player "+ shooter.ToString() +": Kills- "+ points.ToString() +"  Deaths- "+ shootDeaths.ToString() +"\n";
        } else {
            lines[shooter-1] = "Player "+ shooter.ToString() +": Kills- "+ points.ToString() +"  Deaths- "+ shootDeaths.ToString();
        }

        if((shooter != 0)&&(receiver != 0)&&(receiver != 4)){ //if not being initialized and not in back
            lines[receiver-1] = "Player "+ receiver.ToString() +": Kills- "+ recPoints.ToString() +"  Deaths- "+ deaths.ToString() +"\n";
        } else if((shooter != 0)&&(receiver != 0)){
            lines[receiver-1] = "Player "+ receiver.ToString() +": Kills- "+ recPoints.ToString() +"  Deaths- "+ deaths.ToString();
        }

        string megaString = "";
        for(int i = 0; i < lines.Length; i++){
            megaString += lines[i];
        }

        if(Scoreboard != null){
            Scoreboard.GetComponent<Text>().text = megaString;
        } else{
            Debug.Log("ERROR!");
        }
    }

    public void worldStart(){ //allows activity in-game
        Debug.Log("Pressed");
        ToggleMenu();
        Player1.SetActive(true);
        Player2.SetActive(true);
        Player3.SetActive(true);
        Player4.SetActive(true);
    }

    private void worldPause(){ //stops activity in-game
        DeathCam.SetActive(true);
        deathTime = 0;
        SetReticle("");
        DeathCam.transform.position = GameObject.Find("Player Camera").transform.position;
        DeathCam.transform.rotation = GameObject.Find("Player Camera").transform.rotation;
        
        Player1.SetActive(false);
        Player2.GetComponent<WanderingAI>().SetLife(false);
        Player3.GetComponent<WanderingAI>().SetLife(false);
        Player4.GetComponent<WanderingAI>().SetLife(false);
    }

    public void Victory(string winner){ //announces winner and ends game on input
        OverloadText.GetComponent<Text>().text = winner+" wins the Slayer game!!!\nPress Enter to quit.";
        OverloadText.GetComponent<Text>().color = Color.yellow;
        
        victory = true;

        worldPause();
    }

    public void Leave(){
        Application.Quit();
        Debug.Log("Quit game");
    }
}
