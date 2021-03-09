using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Gamestate;

public class LobbyButtonScript : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    [SerializeField] public int teamId = 0 ;

    [SerializeField] public int driverPlayerId;

    [SerializeField] public int gunnerPlayerId;

    public bool driverSlotEmpty = true;
    public bool gunnerSlotEmpty = true;

    public Text driverPlayerText;
    public Text gunnerPlayerText;

    public ReadyToggle readyToggle;

    private LobbySlotMaster lobbySlotMaster;

    public GamestateTracker gamestateTracker;

    public Image addBotDriver;
    public Image kickBotDriver;

    public Image addBotGunner;
    public Image kickBotGunner;
    
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
        gamestateTracker.teams.Create(true, false);

        // add a listener to team record
        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);

        teamEntry.AddListener(TeamListenerCallback);
        teamEntry.Commit();
        
        
        
       
        
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
        }
        else
        {
            driverPlayerText.text = "Empty";
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
                oldTeamEntry.driverId = 0;
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
                oldTeamEntry.gunnerId = 0;
                oldTeamEntry.Commit();
            }


            // set my roles
            playerEntry.role = (short) PlayerEntry.Role.Gunner;

            // set my team
            playerEntry.teamId = (short) teamId;
            playerEntry.Commit();

            // set the driver team
            TeamEntry teamEntry = gamestateTracker.teams.Get((short) teamId);
            teamEntry.driverId = myId;
            teamEntry.Commit();
        }
    }
    
    
    

    
    // ------------------------------------------------------------------------ BOT STUFF
    // bot stuff
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
    public void TeamRemoveEntry()
    {
        
        lobbySlotMaster = FindObjectOfType<LobbySlotMaster>();
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        Debug.Log((short)teamId + " short team id");
        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
        
        // look for the corresponding players in the team
        
        // get driver player (if they exist)
        short driverId = teamEntry.driverId;
        
        PlayerEntry driverEntry = gamestateTracker.players.Get((short) driverId);
        
        // if they are bots, then kick them
        if(driverEntry.isBot) driverEntry.Delete();
        // unready and unselect them
        else
        {
            driverEntry.ready = false;
            driverEntry.role = (short)PlayerEntry.Role.None;
            driverEntry.teamId = 0;
            driverEntry.Commit(TeamRemovePlayerFailureCallback);
        }
        
        
        
        // get gunner player (if they exist)
        short gunnerId = teamEntry.gunnerId;
        PlayerEntry gunnerEntry = gamestateTracker.players.Get((short) gunnerId);

       
        if(gunnerEntry.isBot) gunnerEntry.Delete();
        // unready and unselect them
        else
        {
            gunnerEntry.ready = false;
            gunnerEntry.role = (short)PlayerEntry.Role.None;
            gunnerEntry.teamId = 0;
            gunnerEntry.Commit(TeamRemovePlayerFailureCallback);
        }
        
        teamEntry.Delete();
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
