using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhysX;


[RequireComponent(typeof(PhotonView))]
public class PickupItem : MonoBehaviour, ITriggerEnterEvent
{
    public bool requiresData { get { return true; } }
    
    public float SecondsBeforeRespawn = 2;

    public bool PickupIsMine;


    // these values are internally used. they are public for debugging only

    /// If this client sent a pickup. To avoid sending multiple pickup requests before reply is there.
    public bool SentPickup;

    /// <summary>Timestamp when to respawn the item (compared to PhotonNetwork.time). </summary>
    public double TimeOfRespawn;    // needed when we want to update new players when a PickupItem respawns

    public static HashSet<PickupItem> DisabledPickupItems = new HashSet<PickupItem>();
    public AudioClip pickupSound;
    public GameObject audioSourcePrefab2d;
    public GameObject audioSourcePrefab3d;
    [Range(0.0f, 1.0f)]
    public float pickupSoundVolume = 1f;

    public virtual void TriggerEnter(PhysXCollider other)
    {
        // we only call Pickup() if "our" character collides with this PickupItem.
        // note: if you "position" remote characters by setting their translation, triggers won't be hit.

        PhotonView otherpv = other.GetComponentInParent<PhotonView>();
        if (otherpv != null && otherpv.IsMine)
        {
            Pickup(otherpv);
        }   
    }

    public void TriggerEnter() {}


    protected void PlayItemSoundToTeam(int driverId, int gunnerId)
    {
        if (driverId == PhotonNetwork.LocalPlayer.ActorNumber || gunnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            GameObject a = Instantiate(audioSourcePrefab2d, transform.position, transform.rotation);
            a.GetComponent<AudioSource>().PlayOneShot(pickupSound, pickupSoundVolume);
            Destroy(a, pickupSound.length);
        }
        else
        {
            GameObject a = Instantiate(audioSourcePrefab3d, transform.position, transform.rotation);
            a.GetComponent<AudioSource>().PlayOneShot(pickupSound, pickupSoundVolume);
            Destroy(a, pickupSound.length);
        }
    }
    


/*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // read the description in SecondsBeforeRespawn

        
        if (stream.IsWriting && SecondsBeforeRespawn <= 0)
        {
            stream.SendNext(this.gameObject.transform.position);
        }
        else
        {
            // this will directly apply the last received position for this PickupItem. No smoothing. Usually not needed though.
            Vector3 lastIncomingPos = (Vector3)stream.ReceiveNext();
            this.gameObject.transform.position = lastIncomingPos;
        }
    }
*/


    public virtual void Pickup(PhotonView otherpv)
    {
        if (this.SentPickup)
        {
            // skip sending more pickups until the original pickup-RPC got back to this client
            return;
        }
        this.SentPickup = true;
        
        Debug.Log("Picked up base item");
        NetworkPlayerVehicle npv = otherpv.GetComponentInParent<NetworkPlayerVehicle>();


        this.GetComponent<PhotonView>().RPC(nameof(PunPickup), RpcTarget.AllViaServer, npv.GetDriverID(), npv.GetGunnerID());
    }


    /// <summary>Makes use of RPC PunRespawn to drop an item (sent through server for all).</summary>
    public void Drop()
    {
        if (this.PickupIsMine)
        {
            this.GetComponent<PhotonView>().RPC(nameof(PunRespawn), RpcTarget.AllViaServer);
        }
    }

    /// <summary>Makes use of RPC PunRespawn to drop an item (sent through server for all).</summary>
    public void Drop(Vector3 newPosition)
    {
        if (this.PickupIsMine)
        {
            this.GetComponent<PhotonView>().RPC(nameof(PunRespawn), RpcTarget.AllViaServer, newPosition);
        }
    }


    [PunRPC]
    public void PunPickup(int driverId, int gunnerId)
    {
        
        PlayItemSoundToTeam(driverId, gunnerId);
        // when this client's RPC gets executed, this client no longer waits for a sent pickup and can try again



        // In this solution, picked up items are disabled. They can't be picked up again this way, etc.
        // You could check "active" first, if you're not interested in failed pickup-attempts.
        if (!this.gameObject.activeSelf)
        {
            return;     // makes this RPC being ignored
        }




        
        // setup a respawn (or none, if the item has to be dropped)
        if (SecondsBeforeRespawn <= 0)
        {
            this.PickedUp(0.0f);    // item doesn't auto-respawn. must be dropped
        }
        else
        {
            // how long it is until this item respanws, depends on the pickup time and the respawn time
            double timeSinceRpcCall = 0;//(PhotonNetwork.Time - msgInfo.timestamp);
            double timeUntilRespawn = SecondsBeforeRespawn - timeSinceRpcCall;

            //Debug.Log("msg timestamp: " + msgInfo.timestamp + " time until respawn: " + timeUntilRespawn);

            if (timeUntilRespawn > 0)
            {
                this.PickedUp((float)timeUntilRespawn);
            }
        }
    }

    protected void PickedUp(float timeUntilRespawn)
    {
        // this script simply disables the GO for a while until it respawns.
        this.gameObject.SetActive(false);
        PickupItem.DisabledPickupItems.Add(this);
        this.TimeOfRespawn = 0;

        if (timeUntilRespawn > 0)
        {
            this.TimeOfRespawn = PhotonNetwork.Time + timeUntilRespawn;
            Invoke(nameof(PunRespawn), timeUntilRespawn);
        }
    }


    [PunRPC]
    protected void PunRespawn(Vector3 pos)
    {
        Debug.Log("PunRespawn with Position.");
        this.PunRespawn();
        this.gameObject.transform.position = pos;
    }

    [PunRPC]
    protected void PunRespawn()
    {
        /*
        #if DEBUG
        // debugging: in some cases, the respawn is "late". it's unclear why! just be aware of this.
        double timeDiffToRespawnTime = PhotonNetwork.Time - this.TimeOfRespawn;
        if (timeDiffToRespawnTime > 0.1f) Debug.LogWarning("Spawn time is wrong by: " + timeDiffToRespawnTime + " (this is not an error. you just need to be aware of this.)");
        #endif
        */


        // if this is called from another thread, we might want to do this in OnEnable() instead of here (depends on Invoke's implementation)
        PickupItem.DisabledPickupItems.Remove(this);
        this.TimeOfRespawn = 0;
        this.PickupIsMine = false;
        SentPickup = false;
        if (this.gameObject != null)
        {
            this.gameObject.SetActive(true);
        }
    }
}