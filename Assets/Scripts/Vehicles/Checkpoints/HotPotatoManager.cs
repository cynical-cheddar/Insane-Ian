using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Gamestate;

public class HotPotatoManager : MonoBehaviour
{
    public bool isPotato = false;
    public bool canPickupPotato = true;

    public DriverAbilityManager dam;
    public VehicleHealthManager vhm;
    public GunnerWeaponManager gwm;


    private PotatoEffects potatoEffects;

    private int myDriverId;
    private int myGunnerId;

    private GamestateTracker gamestateTracker;

    private void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
    }

    public void pickupPotato()
    {
        NetworkPlayerVehicle npv = GetComponent<NetworkPlayerVehicle>();
        myDriverId = npv.GetDriverID();
        myGunnerId = npv.GetGunnerID();
        
        
        isPotato = true;
        canPickupPotato = false;
        InvokeRepeating("buffs", 2f, 2f);
        GetComponent<PhotonView>().RPC(nameof(PickupPotatoEffects), RpcTarget.All);


        AnnouncerManager a = FindObjectOfType<AnnouncerManager>();
        a.PlayAnnouncerLine(a.announcerShouts.potatoPickup, myDriverId, myGunnerId);
    }

    [PunRPC]
    void PickupPotatoEffects()
    {
        potatoEffects = GetComponentInChildren<PotatoEffects>();
        potatoEffects.ActivatePotatoEffects(myDriverId, myGunnerId);
    }
    public bool removePotato()
    {
        if (isPotato)
        {

            AnnouncerManager a = FindObjectOfType<AnnouncerManager>();
            a.PlayAnnouncerLine(a.announcerShouts.potatoDrop, myDriverId, myGunnerId);


            isPotato = false;
            canPickupPotato = false;
            Invoke(nameof(ReactivatePickupPotato), 5f);
            GetComponent<PhotonView>().RPC(nameof(RemovePotato_RPC), RpcTarget.AllBuffered);


            CancelInvoke("buffs");
            Vector3 pos = gameObject.transform.position + new Vector3(0.0f, 1.5f, 0.0f);
            PhotonNetwork.Instantiate("HotPotatoGO", pos, Quaternion.identity, 0);
            return true;
        }
        return false;
    }

    void ReactivatePickupPotato()
    {
        canPickupPotato = true;
    }

    [PunRPC]
    void RemovePotato_RPC()
    {
        PhotonView otherpv = GetComponent<PhotonView>();
        NetworkPlayerVehicle npv = otherpv.GetComponentInParent<NetworkPlayerVehicle>();
        HealthManager hm = otherpv.gameObject.GetComponentInChildren<HealthManager>();
        TeamNameSetup tns = otherpv.gameObject.GetComponentInParent<TeamNameSetup>();
        HotPotatoManager hpm = otherpv.gameObject.GetComponentInParent<HotPotatoManager>();


        myDriverId = npv.GetDriverID();
        myGunnerId = npv.GetGunnerID();
        

        tns.ChangeColour(false);
        potatoEffects = GetComponentInChildren<PotatoEffects>();
        potatoEffects.DeactivatePotatoEffects(myDriverId, myGunnerId);
    }
    private void buffs()
    {
        Debug.Log("HERE");
        vhm.HealObject(5);
        dam.AdjustDriverUltimateProgress(5);
        gwm.AdjustGunnerUltimateProgress(5);
        TeamEntry team = gamestateTracker.teams.Get((short)vhm.teamId);
        team.checkpoint++;
        team.Increment();
    }
}
