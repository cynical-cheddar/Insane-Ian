using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PickupHealth : PickupItem
{
    
    public float healthIncrease = 25f;
    
    
    public override void TriggerEnter(PhysXCollider other)
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
                if (!hm.isAtFullHealth) Pickup(otherpv);
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
        
       
        NetworkPlayerVehicle npv = otherpv.GetComponentInParent<NetworkPlayerVehicle>();
        HealthManager hm = otherpv.gameObject.GetComponentInChildren<HealthManager>();
        
        
        hm.TakeDamage(-healthIncrease);
        this.GetComponent<PhotonView>().RPC(nameof(PunPickup), RpcTarget.AllViaServer, npv.GetDriverID(), npv.GetGunnerID());
    }
    
}
