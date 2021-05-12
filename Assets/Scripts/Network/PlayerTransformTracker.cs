using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerTransformTracker : MonoBehaviour
{

    public float loopLength = 4f;

    PickupHotPotato gubbinz;
    
    [Serializable]
    public struct VehicleTransformTeamIdPair
    {
        public int teamId;
        public Transform vehicleTransform;

        public HotPotatoManager hotPotatoManager;

        public VehicleTransformTeamIdPair(int id, Transform t, HotPotatoManager hpt)
        {
            teamId = id;
            vehicleTransform = t;
            hotPotatoManager = hpt;
        }
    }

    [SerializeField] public List<VehicleTransformTeamIdPair> vehicleTransformPairs = new List<VehicleTransformTeamIdPair>();
    
    // find everyone with a network veh
    // Start is called before the first frame update
    void Start()
    {
        // on start, start the coroutine that periodically looks for players
        StartCoroutine(SetVehicleTransformPairsLoop());
    }

    public Transform GetVehicleTransformFromTeamId(int id)
    {
        Transform vehicle = null;
        foreach (VehicleTransformTeamIdPair pair in vehicleTransformPairs)
        {
            if (pair.teamId == id)
            {
                vehicle = pair.vehicleTransform;
                break;
            }
        }

        return vehicle;
    }

    IEnumerator SetVehicleTransformPairsLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(loopLength /2);
            vehicleTransformPairs.Clear();
            NetworkPlayerVehicle[] networkPlayerVehicles = FindObjectsOfType<NetworkPlayerVehicle>();

            foreach (NetworkPlayerVehicle vehicle in networkPlayerVehicles)
            {
                // get id
                int id = vehicle.teamId;
                VehicleTransformTeamIdPair pair = new VehicleTransformTeamIdPair(id, vehicle.transform, vehicle.GetComponent<HotPotatoManager>());
                vehicleTransformPairs.Add(pair);
            }

             bool potatoDropped = false;



            int count = 0;
            int lastIndex = 0;
            int i = 0;
            foreach(VehicleTransformTeamIdPair pair in vehicleTransformPairs){
                if(pair.hotPotatoManager.isPotato) {
                    count ++;
                    lastIndex = i;
                }
                i++;
            }
            if(count > 1){
                // find first one with potato
                vehicleTransformPairs[lastIndex].vehicleTransform.GetComponent<PhotonView>().RPC(nameof(HotPotatoManager.RemovePotatoNoDrop_RPC),RpcTarget.All);
            }
            else if (count >= 1 && FindObjectOfType<PickupHotPotato>() != null){
                PhotonNetwork.Destroy(FindObjectOfType<PickupHotPotato>().gameObject);
            }



        }
        //yield return new WaitForEndOfFrame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
