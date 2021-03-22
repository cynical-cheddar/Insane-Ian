using System.Collections;
using System.Collections.Generic;
using Gamestate;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class UltimateUiManager : MonoBehaviour
{

    public UiBar driverBar;

    public UiBar gunnerBar;

    private bool cachedRole = false;

    private bool isDriver;

    private bool isGunner;

    public GameObject fullDriverBarObject;
    public GameObject fullGunnerBarObject;

    public void CacheRole()
    {
        if (!cachedRole)
        {
            GamestateTracker gs = FindObjectOfType<GamestateTracker>();
            PlayerEntry playerEntry = gs.players.Read( (short) PhotonNetwork.LocalPlayer.ActorNumber);
            if (playerEntry.role == (short) PlayerEntry.Role.Driver) isDriver = true;
            if (playerEntry.role == (short) PlayerEntry.Role.Gunner) isGunner = true;
        }
    }
    
    // Start is called before the first frame update
    public void UpdateDriverBar(float currentValue, float maxValue)
    {
        driverBar.SetProgressBar(currentValue/maxValue);
        driverBar.SetNumber(Mathf.RoundToInt((currentValue/maxValue)*100).ToString());
        if (currentValue >= maxValue && isDriver)
        {
            fullDriverBarObject.SetActive(true);
        }
        else if(isDriver)
        {
            fullDriverBarObject.SetActive(false);
        }
    }

    private float currentValueGunner;
    private float maxValueGunner;
    public void UpdateGunnerBar(float currentValue, float maxValue)
    {
        currentValueGunner = currentValue;
        maxValueGunner = maxValue;
        gunnerBar.SetProgressBar(currentValue/maxValue);
        gunnerBar.SetNumber(Mathf.RoundToInt((currentValue/maxValue)*100).ToString());
        if (currentValue >= maxValue && isGunner)
        {
            fullGunnerBarObject.SetActive(true);
        }
        else if(isGunner)
        {
            fullGunnerBarObject.SetActive(false);
        }
    }
}
