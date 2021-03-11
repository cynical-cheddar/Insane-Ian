using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTransformTracker : MonoBehaviour
{

    public float loopLength = 4f;
    
    [Serializable]
    public struct VehicleTransformTeamIdPair
    {
        public int teamId;
        public Transform vehicleTransform;

        public VehicleTransformTeamIdPair(int id, Transform t)
        {
            teamId = id;
            vehicleTransform = t;
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
            yield return new WaitForSecondsRealtime(loopLength);
            vehicleTransformPairs.Clear();
            NetworkPlayerVehicle[] networkPlayerVehicles = FindObjectsOfType<NetworkPlayerVehicle>();

            foreach (NetworkPlayerVehicle vehicle in networkPlayerVehicles)
            {
                // get id
                int id = vehicle.teamId;
                VehicleTransformTeamIdPair pair = new VehicleTransformTeamIdPair(id, vehicle.transform);
                vehicleTransformPairs.Add(pair);
            }

        }
        //yield return new WaitForEndOfFrame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
