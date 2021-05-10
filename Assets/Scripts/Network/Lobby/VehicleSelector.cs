using Gamestate;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleSelector : MonoBehaviour {
    // Start is called before the first frame update

    public Transform spawnPoint;

    public TextMeshProUGUI displayName;
    public TextMeshProUGUI speed;
    public TextMeshProUGUI strength;
    public TextMeshProUGUI weapon;
    public TextMeshProUGUI ultimate;

    private bool priority = false;
    private short otherId = 0;

    private short ourTeamId = 0;

    private List<string> vehicleNames;

    private string currentVehicle = "";

    private string prefix = "VehicleDummyPrefabs/";

    private GamestateTracker gamestateTracker;
    private GamestateVehicleLookup gamestateVehicleLookup;

    public List<VehicleButtonScript> vehicleButtons;

    private GameObject currentVehicleInstance;

    private short currentVehicleId;

    public Button lockButton;

    public GameObject statsPanel;

    public struct textGui{
        public string displayName;
        public int speed; 
        public int strength;
        public string weapon;
        public string ultimate;

        public textGui(string displayName, int speed, int strength, string weapon, string ultimate){
            this.displayName = displayName;
            this.speed = speed;
            this.strength= strength;
            this.weapon = weapon;
            this.ultimate = ultimate;
            }
    }
    // if you are the driver on the team, or a gunner with only a bot, then you may select the vehicle

    // whenever we select a vehicle, destroy the last one (if it exists), then spawn the new one
    // make sure we disable all drive scripts

    // send rpcs to all, but reference the gunner id if it is not zero. only actually do the stuff on the gunner end if the id is ours


    // if we have priority, then allow the buttons to be clickable
    void SetButtonsInteractable(bool set) {
        foreach (VehicleButtonScript btn in vehicleButtons) {
            btn.GetComponent<Button>().interactable = set;
        }
    }

    public void SelectVehicle(short vehicleId, VehicleGuiData guiData) {
        // update the selected team vehicle in the gamestate tracker
        currentVehicleId = vehicleId;
        lockButton.interactable = true;
        textGui vehicleData = new textGui(guiData.displayName, guiData.speed, guiData.strength, guiData.weapon, guiData.ultimate);
        GetComponent<PhotonView>().RPC(nameof(DisplaySelectedVehicle_RPC), RpcTarget.All, vehicleId, otherId, (short)PhotonNetwork.LocalPlayer.ActorNumber, JsonUtility.ToJson(vehicleData));
    }

    //  USED
    public void LockSelectedVehicle() {
        SetButtonsInteractable(false);
        TeamEntry teamEntry = gamestateTracker.teams.Get(ourTeamId);
        teamEntry.vehicle = currentVehicleId;
        teamEntry.hasSelectedVehicle = true;
        teamEntry.Commit();
    }

    [PunRPC]
    void DisplaySelectedVehicle_RPC(short vehicleId, short otherPlayerId, short myId, string guiDataJSON) {

        if (otherPlayerId == PhotonNetwork.LocalPlayer.ActorNumber || myId == PhotonNetwork.LocalPlayer.ActorNumber) {
            currentVehicle = gamestateVehicleLookup.sortedVehicleNames[vehicleId];
            // destroy last vehicle
            textGui guiData = JsonUtility.FromJson<textGui>(guiDataJSON);
            Debug.Log("Display Selected");
            if (currentVehicleInstance != null) {
                Destroy(currentVehicleInstance);
            }
            // spawn new vehicle
            statsPanel.SetActive(true);
            currentVehicleInstance = Instantiate(Resources.Load(prefix + currentVehicle) as GameObject, spawnPoint.position, spawnPoint.rotation);
            displayName.text = guiData.displayName;
            speed.text = $"Speed: {guiData.speed}";
            strength.text = $"Strength: {guiData.strength}";
            weapon.text = $"Weapon: {guiData.weapon}";
            ultimate.text = $"Ultimate: {guiData.ultimate}";
        }
    }

    void SetupButtons() {
        int maxVehicles = vehicleNames.Count;
        for (int i = 0; i < vehicleButtons.Count; i++) {
            vehicleButtons[i].SetupButton(vehicleNames[i], (short)i, this);
        }

        for (int i = maxVehicles; i < vehicleButtons.Count; i++) {
            vehicleButtons[i].gameObject.SetActive(false);
        }
    }


    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        gamestateVehicleLookup = FindObjectOfType<GamestateVehicleLookup>();
        vehicleNames = gamestateVehicleLookup.sortedVehicleNames;

        PlayerEntry playerEntry = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        PlayerEntry.Role ourRole = (PlayerEntry.Role)playerEntry.role;
        ourTeamId = playerEntry.teamId;
        playerEntry.Release();


        // get the team we are in
        TeamEntry teamEntry = gamestateTracker.teams.Get(ourTeamId);
        short driverId = teamEntry.driverId;
        short gunnerId = teamEntry.gunnerId;
        teamEntry.Release();


        // if we are a gunner, check if the driver is a bot.
        // if so, get priority
        if (ourRole == PlayerEntry.Role.Gunner) {
            PlayerEntry driverEntry = gamestateTracker.players.Get(driverId);
            if (driverEntry.isBot) priority = true;
            driverEntry.Release();
        }

        // if we are a driver, get priority
        if (ourRole == PlayerEntry.Role.Driver) priority = true;


        // assign other player id, only needed if the other player is human
        // hence we only check for if we are a driver
        if (priority && ourRole == PlayerEntry.Role.Driver) {
            // get gunner id
            PlayerEntry gunnerEntry = gamestateTracker.players.Get(gunnerId);
            otherId = gunnerEntry.id;
            gunnerEntry.Release();
        }

        SetButtonsInteractable(priority);
        SetupButtons();
    }
}
