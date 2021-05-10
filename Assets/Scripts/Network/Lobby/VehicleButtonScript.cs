using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleButtonScript : MonoBehaviour
{
    public bool showVehicleNameText = true;
    public TextMeshProUGUI vehicleNameText;
    string vehicleName;
    short vehicleId;
    Sprite icon;
    
    VehicleGuiData vehicleGuiData;

    public Image image;
    private VehicleSelector vehicleSelector;
    private GameObject car;
    private string prefix = "VehiclePrefabs/";
    public void SetupButton(string vehicleNameLocal, short vehicleIdLocal, VehicleSelector vehicleSelectorLocal)
    {
        vehicleName = vehicleNameLocal;
        vehicleId = vehicleIdLocal;
        vehicleSelector = vehicleSelectorLocal;

        
        // load any information about the characters here:
        car = Resources.Load(prefix + vehicleName) as GameObject;
        if (car.GetComponent<VehicleGuiData>() != null)
        {
            Debug.Log("Get Component");
            VehicleGuiData guiData = car.GetComponent<VehicleGuiData>();
            icon = guiData.icon;
            image.sprite = icon;
            if (showVehicleNameText)
                {
                    vehicleNameText.text = guiData.displayName;
                }


            vehicleGuiData = guiData;
        }
    }


    //  USED
    public void SelectVehicle()
    {
        vehicleSelector.SelectVehicle(vehicleId, vehicleGuiData);
    }
}
