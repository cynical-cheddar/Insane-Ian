using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HotPotatoManager : MonoBehaviour
{
    public bool isPotato = false;
    public bool canPickupPotato = true;

    public DriverAbilityManager dam;
    public VehicleHealthManager vhm;
    public GunnerWeaponManager gwm;

    public void pickupPotato()
    {
        isPotato = true;
        canPickupPotato = false;
        InvokeRepeating("buffs", 2f, 2f);

    }
    public void removePotato()
    {
        isPotato = false;
        CancelInvoke("buffs");
        Vector3 pos = gameObject.transform.position + new Vector3(0.0f, 1.5f, 0.0f);
        PhotonNetwork.Instantiate("HotPotatoGO", pos, Quaternion.identity, 0);


    }
    private void buffs()
    {
        Debug.Log("HERE");
        vhm.HealObject(5);
        dam.AdjustDriverUltimateProgress(5);
        gwm.AdjustGunnerUltimateProgress(5);
    }
}
