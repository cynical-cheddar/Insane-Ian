using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Photon.Pun;
public class TelecastManager : MonoBehaviour
{

    Camera telecastCam;
    RawImage rawImage;

    public CinemachineVirtualCamera vcam;
    public Transform defaultPos;

    bool potatoPickedUp = false;

    PlayerTransformTracker playerTransformTracker;

    // Start is called before the first frame update
    void Start()
    {
      //  telecastCam = FindObjectOfType<TelecastCam>().GetComponent<Camera>();
        rawImage = FindObjectOfType<TelecastImage>().GetComponent<RawImage>();
        playerTransformTracker = FindObjectOfType<PlayerTransformTracker>();
    }

    // Update is called once per frame
    public void PickupPotato(NetworkPlayerVehicle npv){
        GetComponent<PhotonView>().RPC(nameof(PickupPotato_RPC), RpcTarget.All, npv.teamId);
    }

    [PunRPC]
    void PickupPotato_RPC(int teamId){
        potatoPickedUp = true;
        vcam.m_Follow = playerTransformTracker.GetVehicleTransformFromTeamId(teamId).gameObject.GetComponentInChildren<DriverCinematicCam>().transform;
        vcam.m_LookAt = playerTransformTracker.GetVehicleTransformFromTeamId(teamId);
    }
    
    public void DropPotato(){
        GetComponent<PhotonView>().RPC(nameof(DropPotato_RPC), RpcTarget.All);
    }
    [PunRPC]
    void DropPotato_RPC(){
        potatoPickedUp = false;
        vcam.m_Follow = defaultPos;
        if(FindObjectOfType<PickupHotPotato>() != null){
            vcam.m_LookAt = FindObjectOfType<PickupHotPotato>().transform;
        }
        else{
            PlayerTransformTracker playerTransformTracker = FindObjectOfType<PlayerTransformTracker>();
            foreach(PlayerTransformTracker.VehicleTransformTeamIdPair pair in playerTransformTracker.vehicleTransformPairs){
                Transform vehicle = pair.vehicleTransform;
                bool potato = vehicle.GetComponent<HotPotatoManager>().isPotato;
                if(potato){
                    vcam.m_LookAt =vehicle;
                }
            }
        }
    }
}
