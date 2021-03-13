using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateUiManager : MonoBehaviour
{

    public UiBar driverBar;

    public UiBar gunnerBar;
    // Start is called before the first frame update
    public void UpdateDriverBar(float currentValue, float maxValue)
    {
        driverBar.setProgressBar(currentValue/maxValue);
        driverBar.setnumber(Mathf.RoundToInt((currentValue/maxValue)*100).ToString());
    }
    
    public void UpdateGunnerBar(float currentValue, float maxValue)
    {
        gunnerBar.setProgressBar(currentValue/maxValue);
        gunnerBar.setnumber(Mathf.RoundToInt((currentValue/maxValue)*100).ToString());
    }
}
