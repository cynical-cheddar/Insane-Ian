using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PickupHotPotato : PickupItem
{
    
    public float healthIncrease = 100f;
    
    
    public override void OnTriggerEnter(Collider other)
    {
        // we only call Pickup() if "our" character collides with this PickupItem.
        // note: if you "position" remote characters by setting their translation, triggers won't be hit.

        PhotonView otherpv = other.GetComponentInParent<PhotonView>();
        if (otherpv != null && otherpv.IsMine)
        {
            HealthManager hm = otherpv.gameObject.GetComponent<HealthManager>();
            if(hm == null) hm = otherpv.gameObject.GetComponentInChildren<HealthManager>();
            if (hm != null)
            {
                Pickup(otherpv);
            }

        }
    }
    
    
    public override void Pickup(PhotonView otherpv)
    {
        Debug.Log("HERE");
        if (this.SentPickup)
        {
            // skip sending more pickups until the original pickup-RPC got back to this client
            return;
        }
        this.SentPickup = true;
        
       
        NetworkPlayerVehicle npv = otherpv.GetComponentInParent<NetworkPlayerVehicle>();
        HealthManager hm = otherpv.gameObject.GetComponentInChildren<HealthManager>();
        TeamNameSetup tns = otherpv.gameObject.GetComponentInParent<TeamNameSetup>();
        HotPotatoManager hpm = otherpv.gameObject.GetComponentInParent<HotPotatoManager>();

        hm.HealObject(healthIncrease);
        tns.ChangeColour(true);
        hpm.pickupPotato();
        this.GetComponent<PhotonView>().RPC(nameof(PunPickup), RpcTarget.AllViaServer, npv.GetDriverID(), npv.GetGunnerID());
    }
    
}
