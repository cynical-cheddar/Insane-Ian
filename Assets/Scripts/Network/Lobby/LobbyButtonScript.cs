using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Gamestate;
using TMPro;

public class LobbyButtonScript : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    [SerializeField] public int teamId = 0 ;

    [SerializeField] public int driverPlayerId;

    [SerializeField] public int gunnerPlayerId;

    public bool driverSlotEmpty = true;
    public bool gunnerSlotEmpty = true;

    public TextMeshProUGUI driverPlayerText;
    public TextMeshProUGUI gunnerPlayerText;

    public Image driverFillImage;
    public Image gunnerFillImage;

    public ReadyToggle readyToggle;

    private LobbySlotMaster lobbySlotMaster;

    public GamestateTracker gamestateTracker;
    
    // new stuff

    void Start()
    {
        lobbySlotMaster = FindObjectOfType<LobbySlotMaster>();
        gamestateTracker = FindObjectOfType<GamestateTracker>();


    }

    
    // called by the lobby button master when we create a team
    public void CreateTeamEntry()
    {
        
        lobbySlotMaster = FindObjectOfType<LobbySlotMaster>();
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        
        
        // new gamestate tracker register team
        TeamEntry teamEntry = gamestateTracker.teams.Create(true, false);

        // add a listener to team record
      //  TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);

        
        teamEntry.Commit();
        GetComponent<PhotonView>().RPC(nameof(AddListenerToThisTeam), RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    void AddListenerToThisTeam()
    {
        TeamEntry teamEntry = gamestateTracker.teams.Read((short)teamId);
        teamEntry.AddListener(TeamListenerCallback);
     //   teamEntry.Commit();
    }
 
    
    
    // called whenever the team stuff changes
    // update the graphics of the button
    void TeamListenerCallback(TeamEntry teamEntry)
    {

        // display the player details of the driver and gunner in the buttons
        
        // driver stuff
        short driverId = teamEntry.driverId;
        if (driverId != 0)
        {
            PlayerEntry driverEntry = gamestateTracker.players.Get((short) driverId);
            driverPlayerText.text = driverEntry.name;
            driverEntry.Release();
            driverFillImage.color = new Color32(0x44, 0x91, 0xCA, 0xFF);
        }
        else
        {
            driverPlayerText.text = "Empty"; 
            driverFillImage.color = new Color32(0xB0, 0xB0, 0xB0, 0xFF);
        }
        
        // gunner stuff
        short gunnerId = teamEntry.gunnerId;
        if (gunnerId != 0)
        {
            PlayerEntry gunnerEntry = gamestateTracker.players.Get((short) gunnerId);
            gunnerPlayerText.text = gunnerEntry.name;
            gunnerEntry.Release();
        }
        else
        {
            gunnerPlayerText.text = "Empty";
        }
        
        teamEntry.Release();
    }



    // checks if the slot is already populated before making it selectable by someone else
    bool CanSelectDriver()
    {
        bool canSelect = true;
        // check if there is a player occupying the current slot
        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
        // if there is a valid player id in the driver slot, return false
        if (teamEntry.driverId != 0) canSelect = false;

        teamEntry.Release();

        return canSelect;
    }
    bool CanSelectGunner()
    {
        bool canSelect = true;
        // check if there is a player occupying the current slot
        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
        // if there is a valid player id in the driver slot, return false
        if (teamEntry.gunnerId != 0) canSelect = false;
        
        teamEntry.Release();

        return canSelect;
    }
    
    // functions to add:

    //  USED
    // select driver slot
    public void SelectDriver()
    {
        if (CanSelectDriver())
        {
            // get my player id
            short myId = (short) PhotonNetwork.LocalPlayer.ActorNumber;
            PlayerEntry playerEntry = gamestateTracker.players.Get(myId);
            
 
            // search for myself in the teams
            // if I already am in a team, remove me
            if (playerEntry.teamId != 0)
            {
                TeamEntry oldTeamEntry = gamestateTracker.teams.Get(playerEntry.teamId);
                if (playerEntry.role == (short)PlayerEntry.Role.Driver) oldTeamEntry.driverId = 0;
                if (playerEntry.role == (short)PlayerEntry.Role.Gunner) oldTeamEntry.gunnerId = 0;
                oldTeamEntry.Commit();
            }


            // set my roles
            playerEntry.role = (short) PlayerEntry.Role.Driver;

            // set my team
            playerEntry.teamId = (short) teamId;
            playerEntry.Commit();

            // set the driver team
            TeamEntry teamEntry = gamestateTracker.teams.Get((short) teamId);
            teamEntry.driverId = myId;
            teamEntry.Commit();
        }
    }

    //  USED
    public void SelectGunner()
    {

        if (CanSelectGunner())
        {
            
            
            // get my player id
            short myId = (short) PhotonNetwork.LocalPlayer.ActorNumber;
            PlayerEntry playerEntry = gamestateTracker.players.Get(myId);


            // search for myself in the teams
            // if I already am in a team, remove me
            if (playerEntry.teamId != 0)
            {
                TeamEntry oldTeamEntry = gamestateTracker.teams.Get(playerEntry.teamId);
                if (playerEntry.role == (short)PlayerEntry.Role.Driver) oldTeamEntry.driverId = 0;
                if (playerEntry.role == (short)PlayerEntry.Role.Gunner) oldTeamEntry.gunnerId = 0;
                oldTeamEntry.Commit();
            }


            // set my roles
            playerEntry.role = (short) PlayerEntry.Role.Gunner;

            // set my team
            playerEntry.teamId = (short) teamId;
            playerEntry.Commit();

            // set the driver team
            TeamEntry teamEntry = gamestateTracker.teams.Get((short) teamId);
            teamEntry.gunnerId = myId;
            teamEntry.Commit();
        }
    }
    
    
    

    
    // ------------------------------------------------------------------------ BOT STUFF
    // bot stuff

    //  USED
    // add gunner bot
    public void AddGunnerBot()
    {
        if (CanSelectGunner())
        {
            PlayerEntry bot = gamestateTracker.players.Create(true, true);
            bot.ready = true;
            bot.role = (short) PlayerEntry.Role.Gunner;
            bot.isBot = true;
            bot.name = "Bot " + -bot.id;
            bot.teamId = (short) teamId;
            bot.Commit();

            // now add the entry to the team
            TeamEntry teamEntry = gamestateTracker.teams.Get((short) teamId);
            teamEntry.gunnerId = bot.id;
            teamEntry.Commit();
        }
    }

    //  USED
    // add the driver bot
    public void AddDriverBot()
    {
        if (CanSelectDriver())
        {
            PlayerEntry bot = gamestateTracker.players.Create(true, true);
            bot.ready = true;
            bot.role = (short) PlayerEntry.Role.Driver;
            bot.isBot = true;
            bot.teamId = (short) teamId;
            bot.name = "Bot " + -bot.id;
            bot.Commit();

            // now add the entry to the team
            TeamEntry teamEntry = gamestateTracker.teams.Get((short) teamId);
            teamEntry.driverId = bot.id;
            teamEntry.Commit();
        }
    }

    //  USED
    public void RemoveGunnerBot()
    {
        // if there is a gunner bot on our team, then remove it from our team and remove it from the gamestate tracker
        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
        if (teamEntry.gunnerId != 0)
        {
            PlayerEntry playerEntry = gamestateTracker.players.Get(teamEntry.gunnerId);
            if (playerEntry.isBot)
            {
                playerEntry.Delete();
            }
            else
            {
                playerEntry.Release();
            }
        }

        teamEntry.gunnerId = 0;
        teamEntry.Commit();
    }

    //  USED
    public void RemoveDriverBot()
    {
        // if there is a gunner bot on our team, then remove it from our team and remove it from the gamestate tracker
        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
        if (teamEntry.driverId != 0)
        {
            PlayerEntry playerEntry = gamestateTracker.players.Get(teamEntry.driverId);
            if (playerEntry.isBot)
            {
                playerEntry.Delete();
            }
            else
            {
                playerEntry.Release();
            }
        }
        teamEntry.driverId = 0;
        teamEntry.Commit();
    }

    
    



    // called by the lobby button master when we create a team
    public bool TeamRemoveEntry()
    {
        lobbySlotMaster = FindObjectOfType<LobbySlotMaster>();
        gamestateTracker = FindObjectOfType<GamestateTracker>();

        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);


        bool canRemove = true;
        // look for the corresponding players in the team
        
        // get driver player (if they exist)
        short driverId = teamEntry.driverId;
            
        if (driverId != 0)
        {

            PlayerEntry driverEntry = gamestateTracker.players.Get((short) driverId);

            // if they are bots, then kick them
            if (driverEntry.isBot) driverEntry.Delete();
            // unready and unselect them
            else
            {
                canRemove = false;
                driverEntry.Release();
            }
        }



        // get gunner player (if they exist)
        short gunnerId = teamEntry.gunnerId;

        if (gunnerId != 0)
        {
            PlayerEntry gunnerEntry = gamestateTracker.players.Get((short) gunnerId);


            if (gunnerEntry.isBot) gunnerEntry.Delete();
            // unready and unselect them
            else
            {
                canRemove = false;
                gunnerEntry.Release();
            }
        }
        
        Debug.Log("Deleting team entry");
        
        
        if(canRemove)teamEntry.Delete();
        else teamEntry.Release();

        return canRemove;
    }



    void TeamRemovePlayerFailureCallback(PlayerEntry playerEntry, bool success)
    {
        if (success)
        {
            playerEntry.Release();
        }
        else
        {
            playerEntry.ready = false;
            playerEntry.role = (short) PlayerEntry.Role.None;
            playerEntry.teamId = 0;
        }
    }


    
    
    
    
    
    
    
    
    
    
    




    // Update is called once per frame
    void Update()
    {
        
    }
}
