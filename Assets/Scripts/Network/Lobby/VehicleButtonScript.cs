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
    public Image image;
    private VehicleSelector vehicleSelector;
    private GameObject car;
    private string prefix = "VehiclePrefabs/";
    public void SetupButton(string vehicleNameLocal, short vehicleIdLocal, VehicleSelector vehicleSelectorLocal)
    {
        vehicleName = vehicleNameLocal;
        vehicleId = vehicleIdLocal;
        vehicleSelector = vehicleSelectorLocal;
        if (showVehicleNameText)
        {
            vehicleNameText.text = vehicleName;
        }
        
        // load any information about the characters here:
        car = Resources.Load(prefix + vehicleName) as GameObject;
        if (car.GetComponent<VehicleGuiData>() != null)
        {
            VehicleGuiData guiData = car.GetComponent<VehicleGuiData>();
            icon = guiData.icon;
            image.sprite = icon;
        }
    }

    //  USED
    public void SelectVehicle()
    {
        vehicleSelector.SelectVehicle(vehicleId);
    }
}
