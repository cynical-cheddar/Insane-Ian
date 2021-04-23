using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PickupDriverUlt : PickupItem
{
    public float ultIncrease = 25f;


    public override void TriggerEnter(PhysXCollider other)
    {
        Debug.Log("AAAAAAAAA");
        Debug.Log("driver ult pickup");
        // we only call Pickup() if "our" character collides with this PickupItem.
        // note: if you "position" remote characters by setting their translation, triggers won't be hit.

        PhotonView otherpv = other.GetComponentInParent<PhotonView>();
        if (otherpv != null && otherpv.IsMine)
        {
            Pickup(otherpv);
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
        DriverAbilityManager dam = otherpv.gameObject.GetComponent<DriverAbilityManager>();


        if (dam != null) {
            if (!dam.usingUltimate) {
                dam.AdjustDriverUltimateProgress(ultIncrease);
                this.GetComponent<PhotonView>().RPC(nameof(PunPickup), RpcTarget.AllViaServer, npv.GetDriverID(),
                    npv.GetGunnerID());
            }
        }
        
    }
}
