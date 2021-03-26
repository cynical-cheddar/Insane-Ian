using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotPotatoManager : MonoBehaviour
{
    public bool isPotato = false;

    public DriverAbilityManager dam;
    public VehicleHealthManager vhm;
    public GunnerWeaponManager gwm;

    public void pickupPotato()
    {
        isPotato = true;
        InvokeRepeating("buffs", 2f, 2f);

    }
    public void removePotato()
    {
        isPotato = false;
        CancelInvoke("buffs");
        PickupHotPotato php = FindObjectOfType<PickupHotPotato>();
        php.Drop();

    }
    private void buffs()
    {
        Debug.Log("HERE");
        vhm.HealObject(3);
        dam.AdjustDriverUltimateProgress(5);
        gwm.AdjustGunnerUltimateProgress(5);
    }
}
