using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamestateVehicleLookup : MonoBehaviour
{
    // to be attached to gamestate tracker
    public enum Vehicle { None, Interceptor, InterceptorBeam, InterceptorMachineGun }

    public List<string> sortedVehicleNames = new List<string>();
    void Awake()
    {
        GameObject[] vehicles = Resources.LoadAll<GameObject>("VehiclePrefabs");
        List<string> vehicleNames = new List<string>();
        
        foreach (GameObject vehicle in vehicles)
        {
            vehicleNames.Add(vehicle.name);
        }
        vehicleNames.Sort();
        sortedVehicleNames = vehicleNames;
    }
    
    
}
