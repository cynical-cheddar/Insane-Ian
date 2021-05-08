using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PickupHotPotato : PickupItem
{
    
    public float healthIncrease = 100f;

    public GameObject nutsNBoltsPrefab;


    [PunRPC]
    void PickupPotato_RPC(int driverId, int gunnerId, int teamId){
        if(driverId == PhotonNetwork.LocalPlayer.ActorNumber || (driverId < 0 && PhotonNetwork.IsMasterClient)){

            PhotonView otherpv = FindObjectOfType<PlayerTransformTracker>().GetVehicleTransformFromTeamId(teamId).GetComponent<PhotonView>();

            NetworkPlayerVehicle npv = otherpv.GetComponentInParent<NetworkPlayerVehicle>();
            HealthManager hm = otherpv.gameObject.GetComponentInChildren<HealthManager>();
            TeamNameSetup tns = otherpv.gameObject.GetComponentInParent<TeamNameSetup>();
            HotPotatoManager hpm = otherpv.gameObject.GetComponentInParent<HotPotatoManager>();

            
            this.GetComponent<PhotonView>().RPC(nameof(PunPickup), RpcTarget.AllViaServer, npv.GetDriverID(), npv.GetGunnerID());
            hm.HealObject(healthIncrease);
            tns.ChangeColour(true);
            hpm.pickupPotato();

            GameObject a = Instantiate(nutsNBoltsPrefab, transform.position, transform.rotation);
            Destroy(a, 4f);
            
            if(PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(this.gameObject);
            if(GetComponent<PhotonView>().IsMine) PhotonNetwork.Destroy(this.gameObject);
        }
    }
    public override void TriggerEnter(PhysXCollider other)
    {
        // we only call Pickup() if "our" character collides with this PickupItem.
        // note: if you "position" remote characters by setting their translation, triggers won't be hit.

        // get gunner and driver id of pickup people.

        // execute if the driver id is mine or if I am the master client and driver id < 0
        
        if(PhotonNetwork.IsMasterClient){
            NetworkPlayerVehicle npv = other.GetComponentInParent<NetworkPlayerVehicle>();
            if(npv != null){
                if(this.SentPickup) return;
                int gunnerID = npv.GetGunnerID();
                int driverID = npv.GetDriverID();
                HotPotatoManager hpm = other.gameObject.GetComponentInParent<HotPotatoManager>();
                if (hpm.canPickupPotato)
                {
                    SentPickup = true;
                    GetComponent<PhotonView>().RPC(nameof(PickupPotato_RPC), RpcTarget.All, driverID, gunnerID, npv.teamId);
                }
            }
        }
    
    }
    
    
    public override void Pickup(PhotonView otherpv)
    {
        if (this.SentPickup)
        {
            // skip sending more pickups until the original pickup-RPC got back to this client
            return;
        }
        this.SentPickup = true;
        
       
        
    }
    

}
