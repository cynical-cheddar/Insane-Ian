using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VehicleSelect : MonoBehaviour
{
    private Dropdown dropdown;
    private GameObject[] vehicles;
    private GameObject selectedVehicle;
    private GamestateTracker gamestateTracker;
    private int teamId;

    // Start is called before the first frame update
    void Start()
    {
        gamestateTracker = FindObjectOfType<GamestateTracker>();

        dropdown = GetComponent<Dropdown>();
        dropdown.ClearOptions();

        vehicles = Resources.LoadAll<GameObject>("VehiclePrefabs");

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < vehicles.Length; i++) {
            options.Add(new Dropdown.OptionData(vehicles[i].name));
        }
        dropdown.AddOptions(options);
    }

    public void SelectVehicle(int i) {
        selectedVehicle = vehicles[i];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
